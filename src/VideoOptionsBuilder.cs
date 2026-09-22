#nullable enable

using System;

namespace FFmpegFluent;

/// <summary>Builds an immutable snapshot of video encoding options.</summary>
public sealed class VideoOptionsBuilder
{
    private string? _codec;
    private string? _bitrate;
    private double? _frameRate;
    private int? _width;
    private int? _height;
    private int? _crf;
    private string? _preset;
    private string? _pixelFormat;

    /// <summary>Sets the video codec.</summary>
    public VideoOptionsBuilder Codec(string codec) { ArgumentNullException.ThrowIfNull(codec); _codec = codec; return this; }
    /// <summary>Sets the target video bitrate.</summary>
    public VideoOptionsBuilder Bitrate(string bitrate) { ArgumentNullException.ThrowIfNull(bitrate); _bitrate = bitrate; return this; }
    /// <summary>Sets the output frame rate.</summary>
    public VideoOptionsBuilder FrameRate(double fps) { _frameRate = fps; return this; }
    /// <summary>Sets the output width and height.</summary>
    public VideoOptionsBuilder Resolution(int width, int height) { _width = width; _height = height; return this; }
    /// <summary>Sets the constant rate factor.</summary>
    public VideoOptionsBuilder Crf(int crf) { _crf = crf; return this; }
    /// <summary>Sets the encoder preset.</summary>
    public VideoOptionsBuilder Preset(string preset) { ArgumentNullException.ThrowIfNull(preset); _preset = preset; return this; }
    /// <summary>Sets the output pixel format.</summary>
    public VideoOptionsBuilder PixelFormat(string pixelFormat) { ArgumentNullException.ThrowIfNull(pixelFormat); _pixelFormat = pixelFormat; return this; }

    /// <summary>Validates the configuration and creates an immutable options snapshot.</summary>
    public VideoOptions Build()
    {
        ValidateText(_codec, nameof(_codec), "Codec");
        ValidateText(_bitrate, nameof(_bitrate), "Bitrate");
        ValidateText(_preset, nameof(_preset), "Preset");
        ValidateText(_pixelFormat, nameof(_pixelFormat), "Pixel format");

        if (_frameRate is <= 0 || double.IsNaN(_frameRate.GetValueOrDefault()) || double.IsInfinity(_frameRate.GetValueOrDefault()))
            throw new ArgumentOutOfRangeException(nameof(_frameRate), "Frame rate must be a finite positive value.");
        if (_width is <= 0) throw new ArgumentOutOfRangeException(nameof(_width), "Width must be a positive value.");
        if (_height is <= 0) throw new ArgumentOutOfRangeException(nameof(_height), "Height must be a positive value.");
        if (_crf is < 0 or > 51) throw new ArgumentOutOfRangeException(nameof(_crf), "CRF must be between 0 and 51.");

        return new VideoOptions(_codec, _bitrate, _frameRate, _width, _height, _crf, _preset, _pixelFormat);
    }

    private static void ValidateText(string? value, string parameterName, string displayName)
    {
        if (value is not null && string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{displayName} cannot be empty or whitespace.", parameterName);
    }
}
