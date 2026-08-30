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
    /// Gets the percentage complete (0-100), or null if total duration is unknown (e.g., live inputs, pipes, or streams without duration).
    /// </summary>
    public double? Percent { get; set; }

    /// <summary>
    /// Returns a string representation of the FFmpeg progress.
    /// </summary>
    /// <returns>A string representation of the FFmpeg progress.</returns>
    public override string ToString() => $"FFmpegProgress {{ ProcessedTime = {ProcessedTime}, Fps = {Fps}, Bitrate = {Bitrate}, FrameCount = {FrameCount}, SpeedX = {SpeedX} }}";

    /// <summary>
    /// Attempts to parse an FFmpeg progress line from stderr output.
    /// </summary>
    /// <remarks>
    /// This method is tolerant of unparsable or absent progress output. Unknown keys are ignored,
    /// 'N/A' values are mapped to null, and percentage is reported as nullable when total duration
    /// is unknown (e.g., live inputs, pipes, or streams without duration).
    /// Returns false if the input is null, empty, or whitespace, or if no valid progress fields were parsed.
    /// Negative or invalid numeric values (e.g., frame=-1) are mapped to null.
    /// </remarks>
    /// <param name="ffmpegStdErrLine">The stderr line to parse.</param>
    /// <param name="progress">The parsed progress, or null if the line is empty/whitespace or parsing failed.</param>
    /// <param name="rawLine">Optional. If provided, receives the raw input line for fallback observation.</param>
    /// <returns>True if parsing succeeded and at least one field was populated; otherwise, false.</returns>
    public static bool TryParse(string? ffmpegStdErrLine, out FFmpegProgress? progress, out string? rawLine)
    {
        if (string.IsNullOrWhiteSpace(ffmpegStdErrLine))
        {
            rawLine = null;
            progress = null;
            return false;
        }

        rawLine = ffmpegStdErrLine;
        var result = new FFmpegProgress();
        var updated = false;

        // Parse frame count
        var frameMatch = Regex.Match(ffmpegStdErrLine, @"frame=\s*(-?\d+)");
        if (frameMatch.Success)
        {
            if (long.TryParse(frameMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var frameValue))
            {
                // Guard against negative values
                if (frameValue < 0)
                {
                    result.FrameCount = null;
                }
                else
                {
                    result.FrameCount = frameValue;
                }
                updated = true;
            }
        }

        // Parse time (HH:MM:SS.ff format)
        var timeMatch = Regex.Match(ffmpegStdErrLine, @"time=(\d{2}:\d{2}:\d{2}\.\d{2,})");
        if (timeMatch.Success)
        {
            result.ProcessedTime = ParseTime(timeMatch.Groups[1].Value);
            updated = true;
        }
        else
        {
            // Parse out_time_ms (milliseconds since start)
            var outTimeMsMatch = Regex.Match(ffmpegStdErrLine, @"out_time_ms=(\d+)");
            if (outTimeMsMatch.Success)
            {
                result.ProcessedTime = ParseMilliseconds(outTimeMsMatch.Groups[1].Value);
                updated = true;
            }
        }

        // Parse fps
        var fpsMatch = Regex.Match(ffmpegStdErrLine, @"fps=\s*(-?[\d.]+)");
        if (fpsMatch.Success)
        {
            if (double.TryParse(fpsMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fpsValue))
            {
                // Guard against negative, infinity, and NaN values
                if (double.IsInfinity(fpsValue) || double.IsNaN(fpsValue) || fpsValue < 0)
                {
                    result.Fps = null;
                }
                else
                {
                    result.Fps = fpsValue;
                }
                updated = true;
            }
        }

        // Parse bitrate - handle N/A values
        var bitrateMatch = Regex.Match(ffmpegStdErrLine, @"bitrate=\s*([^\s]+)");
        if (bitrateMatch.Success)
        {
            var bitrateValue = bitrateMatch.Groups[1].Value.Trim();
            // Map "N/A" to null
            if (string.Equals(bitrateValue, "N/A", StringComparison.OrdinalIgnoreCase))
            {
                result.Bitrate = null;
            }
            else
            {
                result.Bitrate = bitrateValue;
            }
            updated = true;
        }

        // Parse speed
        var speedMatch = Regex.Match(ffmpegStdErrLine, @"speed=\s*(-?[\d.]+)x");
        if (speedMatch.Success)
        {
            if (double.TryParse(speedMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var speedValue))
            {
                // Guard against negative, infinity, and NaN values
                if (double.IsInfinity(speedValue) || double.IsNaN(speedValue) || speedValue < 0)
                {
                    result.SpeedX = null;
                }
                else
                {
                    result.SpeedX = speedValue;
                }
                updated = true;
            }
        }

        // Parse size - handle N/A values
        var sizeMatch = Regex.Match(ffmpegStdErrLine, @"size=\s*([^\s]+)");
        if (sizeMatch.Success)
        {
            var sizeValue = sizeMatch.Groups[1].Value.Trim();
            // Map "N/A" to null
            if (string.Equals(sizeValue, "N/A", StringComparison.OrdinalIgnoreCase))
            {
                // size=N/A is ignored as it's not a standard progress field
            }
            else
            {
                // size is available but we don't currently track it
                updated = true;
            }
        }

        // Parse percentage (when total duration is known)
        var percentMatch = Regex.Match(ffmpegStdErrLine, @"L?percentage=\s*(-?[\d.]+)");
        if (percentMatch.Success)
        {
            if (double.TryParse(percentMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var percentValue))
            {
                // Guard against negative, infinity, and NaN values, then clamp to 0-100
                if (double.IsInfinity(percentValue) || double.IsNaN(percentValue) || percentValue < 0)
                {
                    result.Percent = null;
                }
                else
                {
                    // Clamp percentage to valid range
                    if (percentValue > 100)
                    {
                        percentValue = 100;
                    }
                    result.Percent = percentValue;
                }
                updated = true;
            }
        }

        // Only return a progress object if we successfully parsed at least one field
        if (updated)
        {
            progress = result;
            return true;
        }

        progress = null;
        return false;
    }

    /// <summary>
    /// Attempts to parse an FFmpeg progress line from stderr output.
    /// </summary>
    /// <param name="ffmpegStdErrLine">The stderr line to parse.</param>
    /// <param name="progress">The parsed progress, or null if parsing failed.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryParse(string ffmpegStdErrLine, out FFmpegProgress? progress)
    {
        return TryParse(ffmpegStdErrLine, out progress, out _);
    }

    /// <summary>
    /// Attempts to parse an FFmpeg progress line and update an existing progress instance incrementally.
    /// </summary>
    /// <remarks>
    /// This method is tolerant of unparsable or absent progress output. Unknown keys are ignored,
    /// 'N/A' values are mapped to null.
    /// Negative or invalid numeric values (e.g., frame=-1) are mapped to null.
    /// </remarks>
    /// <param name="line">The stderr line to parse.</param>
    /// <param name="current">The progress instance to update with parsed values. If null, a new instance is created.</param>
    /// <returns>True if parsing succeeded and at least one field was updated; otherwise, false.</returns>
    public static bool TryParse(string line, FFmpegProgress current)
    {
        if (string.IsNullOrWhiteSpace(line))
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
            target.Percent = current.Percent;
        }

        var updated = false;

        // Parse frame count
        var frameMatch = Regex.Match(line, @"frame=\s*(-?\d+)");
        if (frameMatch.Success)
        {
            if (long.TryParse(frameMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var frameValue))
            {
                // Guard against negative values
                if (frameValue < 0)
                {
                    target.FrameCount = null;
                }
                else
                {
                    target.FrameCount = frameValue;
                }
                updated = true;
            }
        }

        // Parse time (HH:MM:SS.ff format)
        var timeMatch = Regex.Match(line, @"time=(\d{2}:\d{2}:\d{2}\.\d{2,})");
        if (timeMatch.Success)
        {
            target.ProcessedTime = ParseTime(timeMatch.Groups[1].Value);
            updated = true;
        }
        else
        {
            // Parse out_time_ms (milliseconds since start)
            var outTimeMsMatch = Regex.Match(line, @"out_time_ms=(\d+)");
            if (outTimeMsMatch.Success)
            {
                target.ProcessedTime = ParseMilliseconds(outTimeMsMatch.Groups[1].Value);
                updated = true;
            }
        }

        // Parse fps
        var fpsMatch = Regex.Match(line, @"fps=\s*(-?[\d.]+)");
        if (fpsMatch.Success)
        {
            if (double.TryParse(fpsMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var fpsValue))
            {
                // Guard against negative, infinity, and NaN values
                if (double.IsInfinity(fpsValue) || double.IsNaN(fpsValue) || fpsValue < 0)
                {
                    target.Fps = null;
                }
                else
                {
                    target.Fps = fpsValue;
                }
                updated = true;
            }
        }

        // Parse bitrate - handle N/A values
        var bitrateMatch = Regex.Match(line, @"bitrate=\s*([^\s]+)");
        if (bitrateMatch.Success)
        {
            var bitrateValue = bitrateMatch.Groups[1].Value.Trim();
            // Map "N/A" to null
            if (string.Equals(bitrateValue, "N/A", StringComparison.OrdinalIgnoreCase))
            {
                target.Bitrate = null;
            }
            else
            {
                target.Bitrate = bitrateValue;
            }
            updated = true;
        }

        // Parse speed
        var speedMatch = Regex.Match(line, @"speed=\s*(-?[\d.]+)x");
        if (speedMatch.Success)
        {
            if (double.TryParse(speedMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var speedValue))
            {
                // Guard against negative, infinity, and NaN values
                if (double.IsInfinity(speedValue) || double.IsNaN(speedValue) || speedValue < 0)
                {
                    target.SpeedX = null;
                }
                else
                {
                    target.SpeedX = speedValue;
                }
                updated = true;
            }
        }

        // Parse percentage (when total duration is known)
        var percentMatch = Regex.Match(line, @"L?percentage=\s*(-?[\d.]+)");
        if (percentMatch.Success)
        {
            if (double.TryParse(percentMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var percentValue))
            {
                // Guard against negative, infinity, and NaN values, then clamp to 0-100
                if (double.IsInfinity(percentValue) || double.IsNaN(percentValue) || percentValue < 0)
                {
                    target.Percent = null;
                }
                else
                {
                    // Clamp percentage to valid range
                    if (percentValue > 100)
                    {
                        percentValue = 100;
                    }
                    target.Percent = percentValue;
                }
                updated = true;
            }
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