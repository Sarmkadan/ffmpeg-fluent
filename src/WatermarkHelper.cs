using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

/// <summary>
/// Defines the position where the watermark will be placed on the video.
/// </summary>
public enum WatermarkPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center
}

/// <summary>
/// Helper class for adding watermarks to video files using FFmpeg.
/// Provides fluent API for configuring watermark position, opacity, scale, and executing the watermarking process.
/// </summary>
public sealed class WatermarkHelper
{
    private readonly string _inputPath;
    private readonly string _watermarkPath;
    private readonly string _outputPath;
    private WatermarkPosition _position = WatermarkPosition.BottomRight;
    private int _margin = 10;
    private double _opacity = 0.5;
    private double _scale = 0.2;

    /// <summary>
/// Initializes a new instance of the <see cref="WatermarkHelper"/> class.
/// </summary>
/// <param name="inputPath">The path to the input video file.</param>
/// <param name="watermarkPath">The path to the watermark image file.</param>
/// <param name="outputPath">The path where the output video with watermark will be saved.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the paths is null.</exception>
public WatermarkHelper(string inputPath, string watermarkPath, string outputPath)
    {
        _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
        _watermarkPath = watermarkPath ?? throw new ArgumentNullException(nameof(watermarkPath));
        _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
    }

    /// <summary>
/// Sets the position and margin for the watermark.
/// </summary>
/// <param name="pos">The position where the watermark will be placed on the video.</param>
/// <param name="margin">The margin in pixels from the edge of the video. Default is 10.</param>
/// <returns>The current <see cref="WatermarkHelper"/> instance for method chaining.</returns>
public WatermarkHelper At(WatermarkPosition pos, int margin = 10)
    {
        _position = pos;
        _margin = margin;
        return this;
    }

    /// <summary>
/// Sets the opacity of the watermark.
/// </summary>
/// <param name="opacity">The opacity value between 0 (fully transparent) and 1 (fully opaque).</param>
/// <returns>The current <see cref="WatermarkHelper"/> instance for method chaining.</returns>
/// <exception cref="ArgumentOutOfRangeException">Thrown when opacity is not between 0 and 1.</exception>
public WatermarkHelper WithOpacity(double opacity)
    {
        if (opacity < 0 || opacity > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(opacity), "Opacity must be between 0 and 1");
        }
        _opacity = opacity;
        return this;
    }

    /// <summary>
/// Sets the scale factor for the watermark.
/// </summary>
/// <param name="factor">The scaling factor for the watermark. Must be a positive value.</param>
/// <returns>The current <see cref="WatermarkHelper"/> instance for method chaining.</returns>
/// <exception cref="ArgumentOutOfRangeException">Thrown when factor is not positive.</exception>
public WatermarkHelper WithScale(double factor)
    {
        if (factor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), "Scale factor must be positive");
        }
        _scale = factor;
        return this;
    }

    /// <summary>
/// Builds the FFmpeg filter complex string for applying the watermark.
/// </summary>
/// <returns>A filter complex string that can be used with FFmpeg to overlay the watermark on the video.</returns>
public string BuildFilterComplex()
    {
        // Calculate position parameters based on watermark position
        string positionX, positionY;

        switch (_position)
        {
            case WatermarkPosition.TopLeft:
                positionX = _margin.ToString();
                positionY = _margin.ToString();
                break;
            case WatermarkPosition.TopRight:
                positionX = $"w - main_w - {_margin}";
                positionY = _margin.ToString();
                break;
            case WatermarkPosition.BottomLeft:
                positionX = _margin.ToString();
                positionY = $"h - main_h - {_margin}";
                break;
            case WatermarkPosition.BottomRight:
                positionX = $"w - main_w - {_margin}";
                positionY = $"h - main_h - {_margin}";
                break;
            case WatermarkPosition.Center:
                positionX = "(w - overlay_w)/2";
                positionY = "(h - overlay_h)/2";
                break;
            default:
                throw new InvalidOperationException("Unknown watermark position");
        }

        // Build the filter complex string
        return $"[0:v][1:v]overlay={positionX}:{positionY}:format=auto,format=yuv420p";
    }

    /// <summary>
/// Builds the complete FFmpeg command arguments array for executing the watermarking process.
/// </summary>
/// <returns>An array of strings containing all FFmpeg command arguments including input/output paths and filter configuration.</returns>
public string[] BuildArguments()
    {
        return [
            "-y",
            "-i", _inputPath,
            "-i", _watermarkPath,
            "-filter_complex", BuildFilterComplex(),
            "-map", "[outv]",
            "-map", "1:a?",
            "-c:v", "libx264",
            "-crf", "23",
            "-preset", "fast",
            "-pix_fmt", "yuv420p",
            _outputPath
        ];
    }

    /// <summary>
/// Asynchronously executes the FFmpeg process to apply the watermark to the video.
/// </summary>
/// <param name="ffmpegPath">The path to the FFmpeg executable. Defaults to "ffmpeg".</param>
/// <param name="ct">Cancellation token for cancelling the operation.</param>
/// <returns>A Task representing the asynchronous operation.</returns>
/// <exception cref="InvalidOperationException">Thrown when the FFmpeg process exits with a non-zero exit code.</exception>
public async Task RunAsync(string ffmpegPath = "ffmpeg", CancellationToken ct = default)
    {
        var arguments = BuildArguments();
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = string.Join(" ", arguments),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        process.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"FFmpeg process failed with exit code {process.ExitCode}");
        }
    }
}