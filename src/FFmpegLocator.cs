#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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
        : this(explicitFFmpegPath: null, explicitFFprobePath: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegLocator"/> class with explicit paths.
    /// </summary>
    /// <param name="explicitFFmpegPath">The explicit path to the FFmpeg executable, or <see langword="null"/> to use resolution order.</param>
    /// <param name="explicitFFprobePath">The explicit path to the FFprobe executable, or <see langword="null"/> to use resolution order.</param>
    public FFmpegLocator(string? explicitFFmpegPath, string? explicitFFprobePath)
    {
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
                if (_ffmpegPath is not null)
                {
                    _ffmpegVersion = GetVersion(_ffmpegPath);
                }
                _ffmpegVerified = true;
            }
            return _ffmpegPath ?? throw new InvalidOperationException(
                "FFmpeg executable not found. Please ensure FFmpeg is installed and available in PATH, " +
                "or set the FFMPEG_PATH environment variable to the full path of the ffmpeg executable.");
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
                if (_ffprobePath is not null)
                {
                    _ffprobeVersion = GetVersion(_ffprobePath);
                }
                _ffprobeVerified = true;
            }
            return _ffprobePath ?? throw new InvalidOperationException(
                "FFprobe executable not found. Please ensure FFprobe is installed and available in PATH, " +
                "or set the FFMPEG_PATH environment variable to the full path of the ffmpeg executable (which typically includes ffprobe).");
        }
    }

    /// <inheritdoc/>
    public FFmpegVersion Version => FFmpegPath is not null ? _ffmpegVersion : throw new InvalidOperationException("FFmpeg executable not located.");

    /// <inheritdoc/>
    public FFmpegVersion FFprobeVersion => FFprobePath is not null ? _ffprobeVersion : throw new InvalidOperationException("FFprobe executable not located.");

    /// <inheritdoc/>
    public FFmpegCommand CreateCommand() => FFmpegCommand.Create(this);

    private string? ResolveFFmpegPath()
    {
        // 1. Try explicit path
        if (_explicitFFmpegPath is not null)
        {
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

        TraceFailure("ffmpeg", _explicitFFmpegPath, envPath);
        return null;
    }

    private string? ResolveFFprobePath()
    {
        // If explicit path was provided for ffmpeg, try to find ffprobe alongside it
        if (_explicitFFmpegPath is not null)
        {
            var ffprobeCandidate = Path.Combine(Path.GetDirectoryName(_explicitFFmpegPath) ?? string.Empty,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe");
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
        TraceEnvironmentOverride("FFMPEG_PATH", envPath);
        if (envPath is not null)
        {
            var ffprobeCandidate = Path.Combine(Path.GetDirectoryName(envPath) ?? string.Empty,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe");
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

        TraceFailure("ffprobe", _explicitFFmpegPath, envPath);
        return null;
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
}
