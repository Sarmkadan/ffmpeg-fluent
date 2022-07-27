namespace FFmpegFluent;

/// <summary>
/// Provides validation helpers for <see cref="FFmpegProgress"/> instances.
/// </summary>
public static class FFmpegProgressValidation
{
    /// <summary>
    /// Validates the specified FFmpeg progress value.
    /// </summary>
    /// <param name="value">The FFmpeg progress to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    public static IReadOnlyList<string> Validate(this FFmpegProgress value)
    {
        if (value is null)
        {
            return new[] { "FFmpegProgress cannot be null." };
        }

        var problems = new List<string>();

        // Validate ProcessedTime (required field)
        if (value.ProcessedTime == default)
        {
            problems.Add("ProcessedTime must be set (cannot be default TimeSpan).");
        }
        else if (value.ProcessedTime < TimeSpan.Zero)
        {
            problems.Add("ProcessedTime cannot be negative.");
        }

        // Validate Fps (must be positive if set)
        if (value.Fps.HasValue)
        {
            if (value.Fps <= 0)
            {
                problems.Add("Fps must be positive if set.");
            }
        }

        // Validate Bitrate (must be non-empty if set)
        if (!string.IsNullOrWhiteSpace(value.Bitrate))
        {
            if (string.IsNullOrWhiteSpace(value.Bitrate))
            {
                problems.Add("Bitrate must be non-empty if set.");
            }
        }

        // Validate FrameCount (must be non-negative if set)
        if (value.FrameCount.HasValue)
        {
            if (value.FrameCount < 0)
            {
                problems.Add("FrameCount must be non-negative if set.");
            }
        }

        // Validate SpeedX (must be positive if set)
        if (value.SpeedX.HasValue)
        {
            if (value.SpeedX <= 0)
            {
                problems.Add("SpeedX must be positive if set.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified FFmpeg progress value is valid.
    /// </summary>
    /// <param name="value">The FFmpeg progress to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValid(this FFmpegProgress value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified FFmpeg progress value is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The FFmpeg progress to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the value is invalid.</exception>
    public static void EnsureValid(this FFmpegProgress value)
    {
        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"FFmpegProgress is invalid. Problems: {string.Join("; ", problems)}");
        }
    }
}