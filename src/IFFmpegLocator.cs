#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace FFmpegFluent;

/// <summary>
/// Represents a service that can locate the FFmpeg and FFprobe executables.
/// </summary>
/// <remarks>
/// The locator follows a resolution order:
/// 1. Explicit path provided to the locator
/// 2. FFMPEG_PATH environment variable
/// 3. PATH environment variable probe
///
/// Once located, the executable is verified by running 'ffmpeg -version' or 'ffprobe -version'.
/// The resolved path and parsed version are cached for performance.
/// </remarks>
public interface IFFmpegLocator
{
    /// <summary>
    /// Gets the resolved path to the FFmpeg executable.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when FFmpeg cannot be located.</exception>
    string FFmpegPath { get; }

    /// <summary>
    /// Gets the resolved path to the FFprobe executable.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when FFprobe cannot be located.</exception>
    string FFprobePath { get; }

    /// <summary>
    /// Gets the parsed version of the located FFmpeg executable.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when FFmpeg cannot be located or version cannot be parsed.</exception>
    FFmpegVersion Version { get; }

    /// <summary>
    /// Gets the parsed version of the located FFprobe executable.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when FFprobe cannot be located or version cannot be parsed.</exception>
    FFmpegVersion FFprobeVersion { get; }

    /// <summary>
    /// Creates a new <see cref="FFmpegCommand"/> instance using this locator.
    /// </summary>
    /// <returns>A new <see cref="FFmpegCommand"/> instance.</returns>
    FFmpegCommand CreateCommand();
}

/// <summary>
/// Represents the version information of an FFmpeg executable.
/// </summary>
/// <param name="Major">The major version number.</param>
/// <param name="Minor">The minor version number.</param>
/// <param name="Patch">The patch version number.</param>
/// <param name="Build">The build number, if available.</param>
public readonly record struct FFmpegVersion(
    int Major,
    int Minor,
    int Patch,
    int? Build = null)
{
    /// <summary>
    /// Gets a value indicating whether this version is at least the specified version.
    /// </summary>
    /// <param name="major">The major version to compare against.</param>
    /// <param name="minor">The minor version to compare against.</param>
    /// <param name="patch">The patch version to compare against.</param>
    /// <returns><see langword="true"/> if this version is at least the specified version; otherwise, <see langword="false"/>.</returns>
    public bool IsAtLeast(int major, int minor = 0, int patch = 0) =>
        Major > major ||
        (Major == major && Minor > minor) ||
        (Major == major && Minor == minor && Patch >= patch);

    /// <summary>
    /// Returns a string representation of the version.
    /// </summary>
    /// <returns>A string in the format "major.minor.patch[.build]".</returns>
    public override string ToString() =>
        Build.HasValue ? $"{Major}.{Minor}.{Patch}.{Build}" : $"{Major}.{Minor}.{Patch}";
}