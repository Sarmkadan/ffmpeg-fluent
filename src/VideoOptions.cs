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
    internal string? _codec;
    internal string? _bitrate;
    internal int? _crf;
    internal string? _preset;
    internal double? _frameRate;
    internal int? _width;
    internal int? _height;
    internal int? _cropX;
    internal int? _cropY;
    internal int? _cropWidth;
    internal int? _cropHeight;
    internal bool _noVideo;

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
        int count = 0;
        if (_noVideo)
        {
            count = 1;
        }
        else
        {
            if (!string.IsNullOrEmpty(_codec)) count += 2;
            if (!string.IsNullOrEmpty(_bitrate)) count += 2;
            if (_crf.HasValue) count += 2;
            if (!string.IsNullOrEmpty(_preset)) count += 2;
            if (_frameRate.HasValue) count += 2;
            if (_width.HasValue && _height.HasValue) count += 2;
            if (_cropWidth.HasValue && _cropHeight.HasValue && _cropX.HasValue && _cropY.HasValue) count += 2;
        }

        if (count == 0)
        {
            return Array.Empty<string>();
        }

        var args = new string[count];
        int i = 0;

        if (_noVideo)
        {
            args[i++] = "-vn";
        }
        else
        {
            if (!string.IsNullOrEmpty(_codec))
            {
                args[i++] = "-c:v";
                args[i++] = _codec;
            }

            if (!string.IsNullOrEmpty(_bitrate))
            {
                args[i++] = "-b:v";
                args[i++] = _bitrate;
            }

            if (_crf.HasValue)
            {
                args[i++] = "-crf";
                Span<char> crfBuffer = stackalloc char[32];
                if (_crf.Value.TryFormat(crfBuffer, out int crfWritten, provider: CultureInfo.InvariantCulture))
                {
                    args[i++] = crfBuffer.Slice(0, crfWritten).ToString();
                }
            }

            if (!string.IsNullOrEmpty(_preset))
            {
                args[i++] = "-preset";
                args[i++] = _preset;
            }

            if (_frameRate.HasValue)
            {
                args[i++] = "-r";
                Span<char> fpsBuffer = stackalloc char[32];
                if (_frameRate.Value.TryFormat(fpsBuffer, out int fpsWritten, provider: CultureInfo.InvariantCulture))
                {
                    args[i++] = fpsBuffer.Slice(0, fpsWritten).ToString();
                }
            }

            if (_width.HasValue && _height.HasValue)
            {
                args[i++] = "-vf";
                args[i++] = ArgumentEscaper.EscapeArgument($"scale={_width.Value}:{_height.Value}");
            }

            if (_cropWidth.HasValue && _cropHeight.HasValue && _cropX.HasValue && _cropY.HasValue)
            {
                args[i++] = "-vf";
                args[i++] = ArgumentEscaper.EscapeArgument($"crop={_cropWidth.Value}:{_cropHeight.Value}:{_cropX.Value}:{_cropY.Value}");
            }
        }

        return args;
    }

    /// <summary>
    /// Runs ad-hoc tests to verify that argument output remains unchanged after optimization.
    /// </summary>
    public static void RunTests()
    {
        var testsPassed = true;

        // Test 1: Unset options should return empty array with zero allocation
        var emptyOpts = new VideoOptions();
        var emptyArgs = emptyOpts.BuildArgs().ToArray();
        if (emptyArgs.Length != 0)
        {
            Console.WriteLine("FAIL: Unset options did not return empty array.");
            testsPassed = false;
        }

        // Test 2: NoVideo
        var noVideoOpts = new VideoOptions().NoVideo();
        var noVideoArgs = noVideoOpts.BuildArgs().ToArray();
        if (noVideoArgs.Length != 1 || noVideoArgs[0] != "-vn")
        {
            Console.WriteLine("FAIL: NoVideo output mismatch.");
            testsPassed = false;
        }

        // Test 3: Full configuration
        var fullOpts = new VideoOptions()
            .Codec("libx264")
            .Bitrate("2M")
            .Crf(23)
            .Preset("fast")
            .FrameRate(30.0)
            .Resolution(1920, 1080)
            .Crop(1920, 1080, 0, 0);
        
        var fullArgs = fullOpts.BuildArgs().ToArray();
        var expectedFull = new[]
        {
            "-c:v", "libx264",
            "-b:v", "2M",
            "-crf", "23",
            "-preset", "fast",
            "-r", "30",
            "-vf", "scale=1920:1080",
            "-vf", "crop=1920:1080:0:0"
        };

        if (fullArgs.Length != expectedFull.Length)
        {
            Console.WriteLine($"FAIL: Full config length mismatch. Expected {expectedFull.Length}, got {fullArgs.Length}.");
            testsPassed = false;
        }
        else
        {
            for (int k = 0; k < fullArgs.Length; k++)
            {
                if (fullArgs[k] != expectedFull[k])
                {
                    Console.WriteLine($"FAIL: Full config mismatch at index {k}. Expected '{expectedFull[k]}', got '{fullArgs[k]}'");
                    testsPassed = false;
                    break;
                }
            }
        }

        Console.WriteLine(testsPassed ? "VideoOptions tests passed." : "VideoOptions tests FAILED.");
    }
}
