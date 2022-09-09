using System.Globalization;

namespace FFmpegFluent;

/// <summary>
/// Represents FFmpeg processing progress information parsed from stderr output.
/// </summary>
public sealed class FFmpegProgress
{
    /// <summary>
    /// Gets the processed time.
    /// </summary>
    public TimeSpan ProcessedTime { get; init; }

    /// <summary>
    /// Gets the frames per second, or null if not available.
    /// </summary>
    public double? Fps { get; init; }

    /// <summary>
    /// Gets the bitrate, or null if not available.
    /// </summary>
    public string? Bitrate { get; init; }

    /// <summary>
    /// Gets the frame count, or null if not available.
    /// </summary>
    public long? FrameCount { get; init; }

    /// <summary>
    /// Gets the speed multiplier (e.g., 2.3x), or null if not available.
    /// </summary>
    public double? SpeedX { get; init; }

    /// <summary>
    /// Attempts to parse an FFmpeg progress line from stderr output.
    /// </summary>
    /// <param name="ffmpegStdErrLine">The stderr line to parse.</param>
    /// <param name="progress">The parsed progress, or null if parsing failed.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(string ffmpegStdErrLine, out FFmpegProgress progress)
    {
        progress = new FFmpegProgress();

        if (string.IsNullOrWhiteSpace(ffmpegStdErrLine))
        {
            return false;
        }

        var frameMatch = System.Text.RegularExpressions.Regex.Match(ffmpegStdErrLine, @"frame=\s*(\d+)");
        var timeMatch = System.Text.RegularExpressions.Regex.Match(ffmpegStdErrLine, @"time=(\d{2}:\d{2}:\d{2}\.\d{2,})");
        var fpsMatch = System.Text.RegularExpressions.Regex.Match(ffmpegStdErrLine, @"fps=\s*(\d+\.?\d*)");
        var bitrateMatch = System.Text.RegularExpressions.Regex.Match(ffmpegStdErrLine, @"bitrate=\s*([\d.]+\s*[kKmMgG]?bps)");
        var speedMatch = System.Text.RegularExpressions.Regex.Match(ffmpegStdErrLine, @"speed=\s*(\d+\.?\d*)x");

        if (!timeMatch.Success)
        {
            return false; // time is required for valid progress
        }

        var progressResult = new FFmpegProgress
        {
            ProcessedTime = ParseTime(timeMatch.Groups[1].Value),
            FrameCount = frameMatch.Success ? long.Parse(frameMatch.Groups[1].Value, CultureInfo.InvariantCulture) : null,
            Fps = fpsMatch.Success ? double.Parse(fpsMatch.Groups[1].Value, CultureInfo.InvariantCulture) : null,
            Bitrate = bitrateMatch.Success ? bitrateMatch.Groups[1].Value.Trim() : null,
            SpeedX = speedMatch.Success ? double.Parse(speedMatch.Groups[1].Value, CultureInfo.InvariantCulture) : null
        };

        progress = progressResult;
        return true;
    }

    private static TimeSpan ParseTime(string timeString)
    {
        var parts = timeString.Split(':', '.', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
        {
            throw new FormatException("Invalid time format");
        }

        var hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
        var minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
        var secondsParts = parts[2].Split('.', StringSplitOptions.RemoveEmptyEntries);
        var seconds = int.Parse(secondsParts[0], CultureInfo.InvariantCulture);
        var milliseconds = secondsParts.Length > 1 ? int.Parse(secondsParts[1], CultureInfo.InvariantCulture) : 0;

        return new TimeSpan(0, hours, minutes, seconds, milliseconds);
    }
}