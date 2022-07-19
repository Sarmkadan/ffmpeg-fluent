using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

public enum WatermarkPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center
}

public sealed class WatermarkHelper
{
    private readonly string _inputPath;
    private readonly string _watermarkPath;
    private readonly string _outputPath;
    private WatermarkPosition _position = WatermarkPosition.BottomRight;
    private int _margin = 10;
    private double _opacity = 0.5;
    private double _scale = 0.2;

    public WatermarkHelper(string inputPath, string watermarkPath, string outputPath)
    {
        _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
        _watermarkPath = watermarkPath ?? throw new ArgumentNullException(nameof(watermarkPath));
        _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
    }

    public WatermarkHelper At(WatermarkPosition pos, int margin = 10)
    {
        _position = pos;
        _margin = margin;
        return this;
    }

    public WatermarkHelper WithOpacity(double opacity)
    {
        if (opacity < 0 || opacity > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(opacity), "Opacity must be between 0 and 1");
        }
        _opacity = opacity;
        return this;
    }

    public WatermarkHelper WithScale(double factor)
    {
        if (factor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), "Scale factor must be positive");
        }
        _scale = factor;
        return this;
    }

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