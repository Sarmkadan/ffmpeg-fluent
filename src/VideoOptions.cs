#nullable enable
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
    private bool _noVideo;

    /// <summary>
    /// Sets the video codec (e.g., "libx264").
    /// </summary>
    public VideoOptions Codec(string codec)
    {
        _codec = codec;
        return this;
    }

    /// <summary>
    /// Sets the video bitrate (e.g., "2M").
    /// </summary>
    public VideoOptions Bitrate(string bitrate)
    {
        _bitrate = bitrate;
        return this;
    }

    /// <summary>
    /// Sets the Constant Rate Factor (CRF) value.
    /// </summary>
    public VideoOptions Crf(int crf)
    {
        _crf = crf;
        return this;
    }

    /// <summary>
    /// Sets the encoding preset (e.g., "fast", "slow").
    /// </summary>
    public VideoOptions Preset(string preset)
    {
        _preset = preset;
        return this;
    }

    /// <summary>
    /// Sets the output frame rate.
    /// </summary>
    public VideoOptions FrameRate(double fps)
    {
        _frameRate = fps;
        return this;
    }

    /// <summary>
    /// Sets the output resolution. This will be emitted as a scale video filter.
    /// </summary>
    public VideoOptions Resolution(int width, int height)
    {
        _width = width;
        _height = height;
        return this;
    }

    /// <summary>
    /// Disables video stream in the output.
    /// </summary>
    public VideoOptions NoVideo()
    {
        _noVideo = true;
        return this;
    }

    /// <summary>
    /// Builds the list of FFmpeg command‑line arguments representing the configured options.
    /// </summary>
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
            args.Add($"scale={_width.Value}:{_height.Value}");
        }

        return args;
    }
}
