using System;

namespace FFmpegFluent;

/// <summary>
/// Provides extension methods for <see cref="WatermarkHelper"/> to position watermarks at common locations with customizable margins.
/// </summary>
public static class WatermarkHelperExtensions
{
    /// <summary>
    /// Positions the watermark in the top-left corner of the video.
    /// </summary>
    /// <param name="helper">The watermark helper instance.</param>
    /// <param name="margin">The margin in pixels from the edges of the video. Default is 10.</param>
    /// <returns>The watermark helper instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="helper"/> is <see langword="null"/>.</exception>
    public static WatermarkHelper TopLeft(this WatermarkHelper helper, int margin = 10)
    {
        ArgumentNullException.ThrowIfNull(helper);
        return helper.At(WatermarkPosition.TopLeft, margin);
    }

    /// <summary>
    /// Positions the watermark in the top-right corner of the video.
    /// </summary>
    /// <param name="helper">The watermark helper instance.</param>
    /// <param name="margin">The margin in pixels from the edges of the video. Default is 10.</param>
    /// <returns>The watermark helper instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="helper"/> is <see langword="null"/>.</exception>
    public static WatermarkHelper TopRight(this WatermarkHelper helper, int margin = 10)
    {
        ArgumentNullException.ThrowIfNull(helper);
        return helper.At(WatermarkPosition.TopRight, margin);
    }

    /// <summary>
    /// Positions the watermark in the bottom-left corner of the video.
    /// </summary>
    /// <param name="helper">The watermark helper instance.</param>
    /// <param name="margin">The margin in pixels from the edges of the video. Default is 10.</param>
    /// <returns>The watermark helper instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="helper"/> is <see langword="null"/>.</exception>
    public static WatermarkHelper BottomLeft(this WatermarkHelper helper, int margin = 10)
    {
        ArgumentNullException.ThrowIfNull(helper);
        return helper.At(WatermarkPosition.BottomLeft, margin);
    }

    /// <summary>
    /// Positions the watermark in the bottom-right corner of the video.
    /// </summary>
    /// <param name="helper">The watermark helper instance.</param>
    /// <param name="margin">The margin in pixels from the edges of the video. Default is 10.</param>
    /// <returns>The watermark helper instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="helper"/> is <see langword="null"/>.</exception>
    public static WatermarkHelper BottomRight(this WatermarkHelper helper, int margin = 10)
    {
        ArgumentNullException.ThrowIfNull(helper);
        return helper.At(WatermarkPosition.BottomRight, margin);
    }
}
