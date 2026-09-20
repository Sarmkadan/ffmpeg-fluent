#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

/// <summary>
/// Default implementation of <see cref="IFFmpegLocator"/> that resolves FFmpeg and FFprobe executables.
/// </summary>
/// <remarks>
/// This implementation follows a resolution order:
/// 1. Explicit path provided to the constructor
/// 2. FFMPEG_PATH environment variable
/// 3. PATH environment variable probe
///
/// Once located, the executable is verified by running 'ffmpeg -version' or 'ffprobe -version'.
/// The resolved path and parsed version are cached for performance.
/// </remarks>
public sealed class FFmpegLocator : IFFmpegLocator
{
    private static readonly object _syncLock = new();
    private static readonly TraceSource _traceSource = new("FFmpegFluent.Locator");
    private static FFmpegLocator? _defaultInstance;

    private readonly string? _explicitFFmpegPath;
    private readonly string? _explicitFFprobePath;
    private string? _ffmpegPath;
    private string? _ffprobePath;
    private FFmpegVersion _ffmpegVersion;
    private FFmpegVersion _ffprobeVersion;
    private bool _ffmpegVerified;
    private bool _ffprobeVerified;

    /// <summary>
    /// Gets the default singleton instance of <see cref="FFmpegLocator"/>.
    /// </summary>
    public static IFFmpegLocator Instance
    {
        get
        {
            if (_defaultInstance is null)
            {
                lock (_syncLock)
                {
                    _defaultInstance ??= new FFmpegLocator();
                }
            }
            return _defaultInstance;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegLocator"/> class with default resolution behavior.
    /// </summary>
    public FFmpegLocator()
    {
        _explicitFFmpegPath = null;
        _explicitFFprobePath = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegLocator"/> class with explicit paths.
    /// </summary>
    /// <param name="explicitFFmpegPath">The explicit path to the FFmpeg executable. Cannot be null, empty, or whitespace.</param>
    /// <param name="explicitFFprobePath">The explicit path to the FFprobe executable. Cannot be null, empty, or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="explicitFFmpegPath"/> or <paramref name="explicitFFprobePath"/> is null, empty, or whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified executable file does not exist.</exception>
    public FFmpegLocator(string? explicitFFmpegPath, string? explicitFFprobePath)
    {
        if (string.IsNullOrWhiteSpace(explicitFFmpegPath))
        {
            throw new ArgumentException("FFmpeg path cannot be null, empty, or whitespace.", nameof(explicitFFmpegPath));
        }
        if (string.IsNullOrWhiteSpace(explicitFFprobePath))
        {
            throw new ArgumentException("FFprobe path cannot be null, empty, or whitespace.", nameof(explicitFFprobePath));
        }

        if (!File.Exists(explicitFFmpegPath))
        {
            throw new FileNotFoundException($"FFmpeg executable not found at the specified path: '{Path.GetFullPath(explicitFFmpegPath)}'.", explicitFFmpegPath);
        }
        if (!File.Exists(explicitFFprobePath))
        {
            throw new FileNotFoundException($"FFprobe executable not found at the specified path: '{Path.GetFullPath(explicitFFprobePath)}'.", explicitFFprobePath);
        }

        _explicitFFmpegPath = explicitFFmpegPath;
        _explicitFFprobePath = explicitFFprobePath;
    }

    /// <inheritdoc/>
    public string FFmpegPath
    {
        get
        {
            if (_ffmpegPath is null && !_ffmpegVerified)
            {
                _ffmpegPath = ResolveFFmpegPath();
                _ffmpegVersion = GetVersion(_ffmpegPath);
                _ffmpegVerified = true;
            }
            return _ffmpegPath;
        }
    }

    /// <inheritdoc/>
    public string FFprobePath
    {
        get
        {
            if (_ffprobePath is null && !_ffprobeVerified)
            {
                _ffprobePath = ResolveFFprobePath();
                _ffprobeVersion = GetVersion(_ffprobePath);
                _ffprobeVerified = true;
            }
            return _ffprobePath;
        }
    }

    /// <inheritdoc/>
    public FFmpegVersion Version => _ffmpegVerified ? _ffmpegVersion : throw new FFmpegNotFoundException("ffmpeg", Array.Empty<string>());

    /// <inheritdoc/>
    public FFmpegVersion FFprobeVersion => _ffprobeVerified ? _ffprobeVersion : throw new FFmpegNotFoundException("ffprobe", Array.Empty<string>());

    /// <inheritdoc/>
    public FFmpegCommand CreateCommand() => FFmpegCommand.Create(this);

    /// <summary>
    /// Attempts to locate the FFmpeg executable without throwing an exception.
    /// </summary>
    /// <param name="path">When this method returns, contains the located path if successful; otherwise, null.</param>
    /// <returns>true if the executable was located; otherwise, false.</returns>
    public bool TryLocate(out string? path)
    {
        try
        {
            path = ResolveFFmpegPath();
            return true;
        }
        catch (FFmpegNotFoundException)
        {
            path = null;
            return false;
        }
    }

    /// <summary>
    /// Asynchronously locates the FFmpeg executable by probing candidates and verifying their version.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A tuple containing the resolved path and the parsed version.</returns>
    /// <exception cref="FFmpegNotFoundException">Thrown when no working FFmpeg executable is found.</exception>
    public async Task<(string Path, FFmpegVersion Version)> LocateAsync(CancellationToken cancellationToken = default)
    {
        var searchedLocations = new List<string>();

        // 1. Try explicit path
        if (_explicitFFmpegPath is not null)
        {
            searchedLocations.Add($"explicit: {_explicitFFmpegPath}");
            TraceCandidate("ffmpeg", "explicit", _explicitFFmpegPath);
            if (File.Exists(_explicitFFmpegPath))
            {
                try
                {
                    var version = await GetVersionAsync(_explicitFFmpegPath, cancellationToken).ConfigureAwait(false);
                    TraceResolved("ffmpeg", _explicitFFmpegPath);
                    return (_explicitFFmpegPath, version);
                }
                catch (Exception ex)
                {
                    TraceFallback("ffmpeg", "environment variable", _explicitFFmpegPath);
                    _traceSource.TraceEvent(TraceEventType.Warning, 0, FormattableString.Invariant(
                        $"resolution=failed explicit path executable=ffmpeg error='{ex.Message}'"));
                }
            }
        }

        // 2. Try FFMPEG_PATH environment variable
        var envPath = Environment.GetEnvironmentVariable("FFMPEG_PATH");
        searchedLocations.Add($"FFMPEG_PATH env var: {envPath ?? "<unset>"}");
        TraceEnvironmentOverride("FFMPEG_PATH", envPath);
        if (envPath is not null && File.Exists(envPath))
        {
            try
            {
                var version = await GetVersionAsync(envPath, cancellationToken).ConfigureAwait(false);
                TraceResolved("ffmpeg", envPath);
                return (envPath, version);
            }
            catch (Exception ex)
            {
                TraceFallback("ffmpeg", "PATH", envPath);
                _traceSource.TraceEvent(TraceEventType.Warning, 0, FormattableString.Invariant(
                    $"resolution=failed env var executable=ffmpeg error='{ex.Message}'"));
            }
        }

        // 3. Probe PATH
        var executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";
        var pathResult = Which(executableName);
        if (pathResult is not null && File.Exists(pathResult))
        {
            try
            {
                var version = await GetVersionAsync(pathResult, cancellationToken).ConfigureAwait(false);
                TraceResolved("ffmpeg", pathResult);
                return (pathResult, version);
            }
            catch (Exception ex)
            {
                TraceFallback("ffmpeg", "PATH", pathResult);
                _traceSource.TraceEvent(TraceEventType.Warning, 0, FormattableString.Invariant(
                    $"resolution=failed path executable=ffmpeg error='{ex.Message}'"));
            }
        }

        var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
        searchedLocations.Add($"PATH search: {string.Join(", ", pathDirs)}");
        TraceFailure("ffmpeg", _explicitFFmpegPath, envPath);
        throw new FFmpegNotFoundException("ffmpeg", searchedLocations.ToArray());
    }

    private string ResolveFFmpegPath()
    {
        var searchedLocations = new List<string>();

        // 1. Try explicit path
        if (_explicitFFmpegPath is not null)
        {
            searchedLocations.Add($"explicit: {_explicitFFmpegPath}");
            TraceCandidate("ffmpeg", "explicit", _explicitFFmpegPath);
            if (File.Exists(_explicitFFmpegPath))
            {
                TraceResolved("ffmpeg", _explicitFFmpegPath);
                return _explicitFFmpegPath;
            }
            TraceFallback("ffmpeg", "environment variable", _explicitFFmpegPath);
        }

        // 2. Try FFMPEG_PATH environment variable
        var envPath = Environment.GetEnvironmentVariable("FFMPEG_PATH");
        searchedLocations.Add($"FFMPEG_PATH env var: {envPath ?? "<unset>"}");
        TraceEnvironmentOverride("FFMPEG_PATH", envPath);
        if (envPath is not null && File.Exists(envPath))
        {
            TraceResolved("ffmpeg", envPath);
            return envPath;
        }

        TraceFallback("ffmpeg", "PATH", envPath ?? "FFMPEG_PATH=<unset>");

        // 3. Probe PATH
        var executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";
        var pathResult = Which(executableName);
        if (pathResult is not null && File.Exists(pathResult))
        {
            TraceResolved("ffmpeg", pathResult);
            return pathResult;
        }

        var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
        searchedLocations.Add($"PATH search: {string.Join(", ", pathDirs)}");
        TraceFailure("ffmpeg", _explicitFFmpegPath, envPath);
        throw new FFmpegNotFoundException("ffmpeg", searchedLocations.ToArray());
    }

    private string ResolveFFprobePath()
    {
        var searchedLocations = new List<string>();

        // If explicit path was provided for ffmpeg, try to find ffprobe alongside it
        if (_explicitFFmpegPath is not null)
        {
            var ffprobeCandidate = Path.Combine(Path.GetDirectoryName(_explicitFFmpegPath) ?? string.Empty,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe");
            searchedLocations.Add($"explicit ffmpeg directory: {ffprobeCandidate}");
            TraceCandidate("ffprobe", "explicit ffmpeg directory", ffprobeCandidate);
            if (File.Exists(ffprobeCandidate))
            {
                TraceResolved("ffprobe", ffprobeCandidate);
                return ffprobeCandidate;
            }

            TraceFallback("ffprobe", "environment variable directory", ffprobeCandidate);
        }

        // Try FFMPEG_PATH environment variable directory
        var envPath = Environment.GetEnvironmentVariable("FFMPEG_PATH");
        searchedLocations.Add($"FFMPEG_PATH env var directory: {envPath ?? "<unset>"}");
        TraceEnvironmentOverride("FFMPEG_PATH", envPath);
        if (envPath is not null)
        {
            var ffprobeCandidate = Path.Combine(Path.GetDirectoryName(envPath) ?? string.Empty,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe");
            searchedLocations.Add($"FFMPEG_PATH directory: {ffprobeCandidate}");
            TraceCandidate("ffprobe", "environment variable directory", ffprobeCandidate);
            if (File.Exists(ffprobeCandidate))
            {
                TraceResolved("ffprobe", ffprobeCandidate);
                return ffprobeCandidate;
            }

            TraceFallback("ffprobe", "PATH", ffprobeCandidate);
        }

        // Probe PATH
        var executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe";
        var pathResult = Which(executableName);
        if (pathResult is not null)
        {
            TraceResolved("ffprobe", pathResult);
            return pathResult;
        }

        var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
        searchedLocations.Add($"PATH search: {string.Join(", ", pathDirs)}");
        TraceFailure("ffprobe", _explicitFFmpegPath, envPath);
        throw new FFmpegNotFoundException("ffprobe", searchedLocations.ToArray());
    }

    private static string? Which(string executableName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // On Windows, check PATHEXT for possible extensions
            var pathext = Environment.GetEnvironmentVariable("PATHEXT") ?? ".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH";
            var extensions = pathext.Split(';');

            foreach (var pathEntry in Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>())
            {
                if (string.IsNullOrEmpty(pathEntry))
                {
                    continue;
                }

                TracePathEntry(executableName, pathEntry);

                foreach (var ext in extensions)
                {
                    var fullPath = Path.Combine(pathEntry, executableName + ext.Trim());
                    TraceCandidate(executableName, "PATH", fullPath);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }
        }
        else
        {
            // On Unix-like systems, just check PATH entries directly
            foreach (var pathEntry in Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>())
            {
                if (string.IsNullOrEmpty(pathEntry))
                {
                    continue;
                }

                TracePathEntry(executableName, pathEntry);
                var fullPath = Path.Combine(pathEntry, executableName);
                TraceCandidate(executableName, "PATH", fullPath);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return null;
    }

    private static void TraceCandidate(string executable, string source, string candidatePath)
    {
        var directory = Path.GetDirectoryName(candidatePath) ?? string.Empty;
        _traceSource.TraceEvent(TraceEventType.Verbose, 0, FormattableString.Invariant(
            $"probe=candidate executable={executable} source={source} directory='{directory}' path='{candidatePath}'"));
    }

    private static void TracePathEntry(string executable, string pathEntry)
    {
        _traceSource.TraceEvent(TraceEventType.Verbose, 0, FormattableString.Invariant(
            $"probe=path-entry executable={executable} directory='{pathEntry}'"));
    }

    private static void TraceEnvironmentOverride(string variable, string? value)
    {
        _traceSource.TraceEvent(TraceEventType.Verbose, 0, FormattableString.Invariant(
            $"probe=environment-variable variable={variable} value='{value ?? "<unset>"}'"));
    }

    private static void TraceResolved(string executable, string path)
    {
        _traceSource.TraceEvent(TraceEventType.Information, 0, FormattableString.Invariant(
            $"resolution=resolved executable={executable} fullPath='{Path.GetFullPath(path)}'"));
    }

    private static void TraceFallback(string executable, string nextSource, string searchedLocation)
    {
        _traceSource.TraceEvent(TraceEventType.Warning, 0, FormattableString.Invariant(
            $"resolution=fallback executable={executable} nextSource='{nextSource}' searchedLocations='{searchedLocation}'"));
    }

    private static void TraceFailure(string executable, string? explicitPath, string? environmentPath)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? "<unset>";
        _traceSource.TraceEvent(TraceEventType.Warning, 0, FormattableString.Invariant(
            $"resolution=failed executable={executable} searchedLocations='explicit={explicitPath ?? "<unset>"}; FFMPEG_PATH={environmentPath ?? "<unset>"}; PATH={path}'"));
    }

    private static FFmpegVersion GetVersion(string executablePath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // Combine stdout and stderr as version info can appear in either
            var versionOutput = output + error;

            // Parse version from output like:
            // ffmpeg version 5.1.2-0+deb12u1 Copyright (c) 2000-2023...
            // ffmpeg version n6.0-3-g8b7c9d123 Copyright (c)...
            var match = Regex.Match(versionOutput,
                @"ffmpeg\s+version\s+(?:[nv]?(\d+)\.(\d+)\.(\d+)(?:[\.-](\d+))?)");

            if (match.Success)
            {
                var major = int.Parse(match.Groups[1].Value);
                var minor = int.Parse(match.Groups[2].Value);
                var patch = int.Parse(match.Groups[3].Value);
                var build = match.Groups[4].Success ? (int?)int.Parse(match.Groups[4].Value) : null;
                return new FFmpegVersion(major, minor, patch, build);
            }

            throw new InvalidOperationException(
                $"Could not parse FFmpeg version from executable at '{executablePath}'. " +
                "Output did not match expected format.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to verify FFmpeg executable at '{executablePath}'.", ex);
        }
    }

    private static async Task<FFmpegVersion> GetVersionAsync(string executablePath, CancellationToken cancellationToken)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            // Combine stdout and stderr as version info can appear in either
            var versionOutput = output + error;

            // Parse version from output like:
            // ffmpeg version 5.1.2-0+deb12u1 Copyright (c) 2000-2023...
            // ffmpeg version n6.0-3-g8b7c9d123 Copyright (c)...
            var match = Regex.Match(versionOutput,
                @"ffmpeg\s+version\s+(?:[nv]?(\d+)\.(\d+)\.(\d+)(?:[\.-](\d+))?)");

            if (match.Success)
            {
                var major = int.Parse(match.Groups[1].Value);
                var minor = int.Parse(match.Groups[2].Value);
                var patch = int.Parse(match.Groups[3].Value);
                var build = match.Groups[4].Success ? (int?)int.Parse(match.Groups[4].Value) : null;
                return new FFmpegVersion(major, minor, patch, build);
            }

            throw new InvalidOperationException(
                $"Could not parse FFmpeg version from executable at '{executablePath}'. " +
                "Output did not match expected format.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to verify FFmpeg executable at '{executablePath}'.", ex);
        }
    }
}
