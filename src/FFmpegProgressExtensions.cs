namespace FFmpegFluent;

/// <summary>
/// Provides extension methods for <see cref="FFmpegProgress"/> to enhance functionality with common operations.
/// </summary>
public static class FFmpegProgressExtensions
{
    /// <summary>
    /// Calculates the estimated remaining time based on processed time and speed multiplier.
    /// </summary>
    /// <param name="progress">The FFmpeg progress instance.</param>
    /// <returns>The estimated remaining time, or null if speed is not available.</returns>
    public static TimeSpan? GetRemainingTime(this FFmpegProgress progress)
    {
        if (progress?.SpeedX == null || progress.SpeedX <= 0)
        {
            return null;
        }

        var processedSeconds = progress.ProcessedTime.TotalSeconds;
        var speed = progress.SpeedX.Value;
        var estimatedTotalSeconds = processedSeconds / speed;
        var remainingSeconds = estimatedTotalSeconds - processedSeconds;

        return remainingSeconds > 0 ? TimeSpan.FromSeconds(remainingSeconds) : TimeSpan.Zero;
    }

    /// <summary>
    /// Determines if the processing is complete based on whether speed has dropped to zero or below.
    /// </summary>
    /// <param name="progress">The FFmpeg progress instance.</param>
    /// <returns>True if processing appears to be complete; otherwise, false.</returns>
    public static bool IsComplete(this FFmpegProgress progress)
    {
        return progress?.SpeedX == 0;
    }

    /// <summary>
    /// Gets a formatted progress string showing processed time, speed, and fps.
    /// </summary>
    /// <param name="progress">The FFmpeg progress instance.</param>
    /// <param name="includeBitrate">Whether to include bitrate in the output.</param>
    /// <returns>A formatted progress string.</returns>
    public static string ToProgressString(this FFmpegProgress progress, bool includeBitrate = true)
    {
        if (progress == null)
        {
            return "No progress available";
        }

        var timeStr = progress.ProcessedTime.ToString("hh\\:mm\\:ss");
        var speedStr = progress.SpeedX.HasValue ? $"{progress.SpeedX.Value:F2}x" : "?x";
        var fpsStr = progress.Fps.HasValue ? $"{progress.Fps.Value:F1}fps" : "?fps";

        var parts = new List<string> { $"Time: {timeStr}", $"Speed: {speedStr}", fpsStr };

        if (includeBitrate && progress.Bitrate != null)
        {
            parts.Add($"Bitrate: {progress.Bitrate}");
        }

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Calculates the estimated total duration based on processed time and speed multiplier.
    /// </summary>
    /// <param name="progress">The FFmpeg progress instance.</param>
    /// <returns>The estimated total duration, or null if speed is not available.</returns>
    public static TimeSpan? GetEstimatedTotalDuration(this FFmpegProgress progress)
    {
        if (progress?.SpeedX == null || progress.SpeedX <= 0)
        {
            return null;
        }

        var processedSeconds = progress.ProcessedTime.TotalSeconds;
        var speed = progress.SpeedX.Value;
        var estimatedTotalSeconds = processedSeconds / speed;

        return TimeSpan.FromSeconds(estimatedTotalSeconds);
    }
}