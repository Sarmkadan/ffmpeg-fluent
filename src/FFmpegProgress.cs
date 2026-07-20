using System.Globalization;
using System.Text.RegularExpressions;

namespace FFmpegFluent;

/// <summary>
/// Represents FFmpeg processing progress information parsed from stderr output.
/// </summary>
public sealed class FFmpegProgress
{
    /// <summary>
    /// Gets the processed time.
    /// </summary>
    public TimeSpan ProcessedTime { get; set; }

    /// <summary>
    /// Gets the frames per second, or null if not available.
    /// </summary>
    public double? Fps { get; set; }

    /// <summary>
    /// Gets the bitrate, or null if not available.
    /// </summary>
    public string? Bitrate { get; set; }

    /// <summary>
    /// Gets the frame count, or null if not available.
    /// </summary>
    public long? FrameCount { get; set; }

    /// <summary>
    /// Gets the speed multiplier (e.g., 2.3x), or null if not available.
    /// </summary>
    public double? SpeedX { get; set; }

    /// <summary>
    /// Attempts to parse an FFmpeg progress line from stderr output.
    /// </summary>
    /// <param name="ffmpegStdErrLine">The stderr line to parse.</param>
    /// <param name="progress">The parsed progress, or null if parsing failed.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(string ffmpegStdErrLine, out FFmpegProgress? progress)
    {
        progress = null;

        if (string.IsNullOrWhiteSpace(ffmpegStdErrLine))
        {
            return false;
        }

        var frameMatch = Regex.Match(ffmpegStdErrLine, @"frame=\s*(\d+)");
        var timeMatch = Regex.Match(ffmpegStdErrLine, @"time=(\d{2}:\d{2}:\d{2}\.\d{2,})");
        var fpsMatch = Regex.Match(ffmpegStdErrLine, @"fps=\s*(\d+\.?\d*)");
        var bitrateMatch = Regex.Match(ffmpegStdErrLine, @"bitrate=\s*([\d.]+\s*[kKmMgG]?(?:bits/s|bps))");
        var speedMatch = Regex.Match(ffmpegStdErrLine, @"speed=\s*(\d+\.?\d*)x");
        var outTimeMsMatch = Regex.Match(ffmpegStdErrLine, @"out_time_ms=(\d+)");

        if (!timeMatch.Success && !outTimeMsMatch.Success)
        {
            return false; // time or out_time_ms is required for valid progress
        }

        var progressResult = new FFmpegProgress
        {
            ProcessedTime = timeMatch.Success ? ParseTime(timeMatch.Groups[1].Value) : ParseMilliseconds(outTimeMsMatch.Groups[1].Value),
            FrameCount = frameMatch.Success ? long.Parse(frameMatch.Groups[1].Value, CultureInfo.InvariantCulture) : null,
            Fps = fpsMatch.Success ? double.Parse(fpsMatch.Groups[1].Value, CultureInfo.InvariantCulture) : null,
            Bitrate = bitrateMatch.Success ? bitrateMatch.Groups[1].Value.Trim() : null,
            SpeedX = speedMatch.Success ? double.Parse(speedMatch.Groups[1].Value, CultureInfo.InvariantCulture) : null
        };

        progress = progressResult;
        return true;
    }

    /// <summary>
    /// Attempts to parse an FFmpeg progress line and update an existing progress instance incrementally.
    /// </summary>
    /// <param name="line">The stderr line to parse.</param>
    /// <param name="current">The progress instance to update with parsed values. If null, a new instance is created.</param>
    /// <returns>True if parsing succeeded and at least one field was updated; otherwise, false.</returns>
    public static bool TryParse(string line, FFmpegProgress current)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var frameMatch = Regex.Match(line, @"frame=\s*(\d+)");
        var timeMatch = Regex.Match(line, @"time=(\d{2}:\d{2}:\d{2}\.\d{2,})");
        var fpsMatch = Regex.Match(line, @"fps=\s*(\d+\.?\d*)");
        var bitrateMatch = Regex.Match(line, @"bitrate=\s*([\d.]+\s*[kKmMgG]?(?:bits/s|bps))");
        var speedMatch = Regex.Match(line, @"speed=\s*(\d+\.?\d*)x");
        var outTimeMsMatch = Regex.Match(line, @"out_time_ms=(\d+)");

        if (!timeMatch.Success && !outTimeMsMatch.Success && !frameMatch.Success && !fpsMatch.Success && !bitrateMatch.Success && !speedMatch.Success)
        {
            return false;
        }

        var target = new FFmpegProgress();

        // Copy existing values if current is not null
        if (current != null)
        {
            target.ProcessedTime = current.ProcessedTime;
            target.FrameCount = current.FrameCount;
            target.Fps = current.Fps;
            target.Bitrate = current.Bitrate;
            target.SpeedX = current.SpeedX;
        }

        var updated = false;

        if (timeMatch.Success)
        {
            target.ProcessedTime = ParseTime(timeMatch.Groups[1].Value);
            updated = true;
        }
        else if (outTimeMsMatch.Success)
        {
            target.ProcessedTime = ParseMilliseconds(outTimeMsMatch.Groups[1].Value);
            updated = true;
        }

        if (frameMatch.Success)
        {
            target.FrameCount = long.Parse(frameMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            updated = true;
        }

        if (fpsMatch.Success)
        {
            target.Fps = double.Parse(fpsMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            updated = true;
        }

        if (bitrateMatch.Success)
        {
            target.Bitrate = bitrateMatch.Groups[1].Value.Trim();
            updated = true;
        }

        if (speedMatch.Success)
        {
            target.SpeedX = double.Parse(speedMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            updated = true;
        }

        return updated;
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
        var seconds = int.Parse(parts[2], CultureInfo.InvariantCulture);

        // The fractional part is a decimal fraction of a second, not milliseconds:
        // ffmpeg prints centiseconds ("10.50" = 10 s 500 ms), so scale by digit count.
        var milliseconds = 0;
        if (parts.Length > 3)
        {
            var fraction = parts[3];
            var fractionValue = int.Parse(fraction, CultureInfo.InvariantCulture);
            milliseconds = (int)Math.Round(fractionValue * 1000 / Math.Pow(10, fraction.Length));
        }

        return new TimeSpan(0, hours, minutes, seconds, milliseconds);
    }

    private static TimeSpan ParseMilliseconds(string millisecondsString)
    {
        var milliseconds = long.Parse(millisecondsString, CultureInfo.InvariantCulture);
        return TimeSpan.FromMilliseconds(milliseconds);
    }
}
