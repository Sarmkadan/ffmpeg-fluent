#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace FFmpegFluent;

/// <summary>
/// Fluent builder for video output options for FFmpeg.
/// </summary>
public sealed class VideoOptions
{
    private string? _codec;
    private string? _bitrate;
    private int? _crf;
    private string? _preset;
    private double? _frameRate;
    private int? _width;
    private int? _height;
    private int? _cropX;
    private int? _cropY;
    private int? _cropWidth;
    private int? _cropHeight;
    private bool _noVideo;

    /// <summary>
    /// Sets the video codec (e.g., "libx264").
    /// </summary>
    /// <param name="codec">The video codec name (e.g., "libx264", "libx265").</param>
    /// <returns>The same <see cref="VideoOptions"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="codec"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="codec"/> is empty or whitespace.</exception>
    public VideoOptions Codec(string codec)
    {
        ArgumentNullException.ThrowIfNull(codec);
        if (string.IsNullOrWhiteSpace(codec))
        {
            throw new ArgumentException("Codec name cannot be empty or whitespace.", nameof(codec));
        }

        _codec = codec;
        return this;
    }

    /// <summary>
    /// Sets the video bitrate (e.g., "2M").
    /// </summary>
    /// <param name="bitrate">The bitrate string (e.g., "2000k", "2M").</param>
    /// <returns>The same <see cref="VideoOptions"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bitrate"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="bitrate"/> is empty or whitespace.</exception>
    public VideoOptions Bitrate(string bitrate)
    {
        ArgumentNullException.ThrowIfNull(bitrate);
        if (string.IsNullOrWhiteSpace(bitrate))
        {
            throw new ArgumentException("Bitrate cannot be empty or whitespace.", nameof(bitrate));
        }

        _bitrate = bitrate;
        return this;
    }

    /// <summary>
    /// Sets the Constant Rate Factor (CRF) value.
    /// </summary>
    /// <param name="crf">The CRF value (typically 0-51, where lower is higher quality).</param>
    /// <returns>The same <see cref="VideoOptions"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="crf"/> is outside the valid range (0-51).</exception>
    public VideoOptions Crf(int crf)
    {
        if (crf < 0 || crf > 51)
        {
            throw new ArgumentOutOfRangeException(nameof(crf), "CRF must be between 0 and 51.");
        }

        _crf = crf;
        return this;
    }

    /// <summary>
    /// Sets the encoding preset (e.g., "fast", "slow").
    /// </summary>
    /// <param name="preset">The encoding preset name (e.g., "ultrafast", "superfast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow").</param>
    /// <returns>The same <see cref="VideoOptions"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="preset"/> is empty or whitespace.</exception>
    public VideoOptions Preset(string preset)
    {
        ArgumentNullException.ThrowIfNull(preset);
        if (string.IsNullOrWhiteSpace(preset))
        {
            throw new ArgumentException("Preset name cannot be empty or whitespace.", nameof(preset));
        }

        _preset = preset;
        return this;
    }

    /// <summary>
    /// Sets the output frame rate.
    /// </summary>
    /// <param name="fps">The frame rate in frames per second.</param>
    /// <returns>The same <see cref="VideoOptions"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fps"/> is not positive.</exception>
    public VideoOptions FrameRate(double fps)
    {
        if (fps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fps), "Frame rate must be a positive value.");
        }

        _frameRate = fps;
        return this;
    }

    /// <summary>
    /// Sets the output resolution. This will be emitted as a scale video filter.
    /// </summary>
    /// <param name="width">The target width in pixels (must be positive).</param>
    /// <param name="height">The target height in pixels (must be positive).</param>
    /// <returns>The same <see cref="VideoOptions"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    public VideoOptions Resolution(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be a positive value.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be a positive value.");
        }

        _width = width;
        _height = height;
        return this;
    }

    /// <summary>
    /// Crops the video to the specified rectangle. All parameters must be non-negative.
    /// This will be emitted as a crop video filter.
    /// </summary>
    /// <param name="width">Width of the crop rectangle (must be &gt; 0)</param>
    /// <param name="height">Height of the crop rectangle (must be &gt; 0)</param>
    /// <param name="x">X offset of the crop rectangle (must be &gt;= 0)</param>
    /// <param name="y">Y offset of the crop rectangle (must be &gt;= 0)</param>
    /// <returns>The VideoOptions instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if any parameter is negative.</exception>
    public VideoOptions Crop(int width, int height, int x, int y)
    {
        if (width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Crop width must be non-negative");
        }
        if (height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Crop height must be non-negative");
        }
        if (x < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Crop x offset must be non-negative");
        }
        if (y < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(y), "Crop y offset must be non-negative");
        }

        _cropWidth = width;
        _cropHeight = height;
        _cropX = x;
        _cropY = y;
        return this;
    }

    /// <summary>
    /// Disables video stream in the output.
    /// </summary>
    /// <returns>The same <see cref="VideoOptions"/> instance for fluent chaining.</returns>
    public VideoOptions NoVideo()
    {
        _noVideo = true;
        return this;
    }

    /// <summary>
    /// Builds the list of FFmpeg command‑line arguments representing the configured options.
    /// </summary>
    /// <returns>An enumerable of command-line arguments.</returns>
    public IEnumerable<string> BuildArgs()
    {
        var args = new List<string>();

        if (_noVideo)
        {
            args.Add("-vn");
            return args;
        }

        if (!string.IsNullOrEmpty(_codec))
        {
            args.Add("-c:v");
            args.Add(_codec);
        }

        if (!string.IsNullOrEmpty(_bitrate))
        {
            args.Add("-b:v");
            args.Add(_bitrate);
        }

        if (_crf.HasValue)
        {
            args.Add("-crf");
            args.Add(_crf.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrEmpty(_preset))
        {
            args.Add("-preset");
            args.Add(_preset);
        }

        if (_frameRate.HasValue)
        {
            args.Add("-r");
            args.Add(_frameRate.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (_width.HasValue && _height.HasValue)
        {
            // Use a scale filter to set resolution.
            args.Add("-vf");
            args.Add(ArgumentEscaper.EscapeArgument($"scale={_width.Value}:{_height.Value}"));
        }

        if (_cropWidth.HasValue && _cropHeight.HasValue && _cropX.HasValue && _cropY.HasValue)
        {
            // Use a crop filter to crop the video.
            args.Add("-vf");
            args.Add(ArgumentEscaper.EscapeArgument($"crop={_cropWidth.Value}:{_cropHeight.Value}:{_cropX.Value}:{_cropY.Value}"));
        }

        return args;
    }
}
