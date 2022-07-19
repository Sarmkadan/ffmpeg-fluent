using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

public sealed class GifPreset
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private int _fps = 10;
    private int? _width;
    private TimeSpan? _start;
    private TimeSpan? _duration;

    public GifPreset(string inputPath, string outputPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(inputPath);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        _inputPath = inputPath;
        _outputPath = outputPath;
    }

    public GifPreset WithFps(int fps)
    {
        if (fps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fps), "FPS must be positive");
        }

        _fps = fps;
        return this;
    }

    public GifPreset WithWidth(int width)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
        }

        _width = width;
        return this;
    }

    public GifPreset WithRange(TimeSpan start, TimeSpan duration)
    {
        if (start < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(start), "Start time cannot be negative");
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive");
        }

        if (start + duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Start + duration cannot overflow");
        }

        _start = start;
        _duration = duration;
        return this;
    }

    public string[] BuildArguments()
    {
        var args = new System.Collections.Generic.List<string>();
        args.Add("-y");
        args.Add("-i");
        args.Add(_inputPath);

        if (_start.HasValue)
        {
            args.Add("-ss");
            args.Add(_start.Value.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        if (_duration.HasValue)
        {
            args.Add("-t");
            args.Add(_duration.Value.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        args.Add("-vf");

        var filters = new System.Collections.Generic.List<string>();

        if (_width.HasValue)
        {
            filters.Add($"scale={_width.Value}:-1:flags=lanczos");
        }

        filters.Add("fps");
        filters.Add(_fps.ToString(System.Globalization.CultureInfo.InvariantCulture));

        filters.Add("split[s0][s1]");
        filters.Add("[s0]palettegen=stats_mode=diff[p]");
        filters.Add("[s1][p]paletteuse");

        args.Add(string.Join(",", filters));

        args.Add("-gifflags");
        args.Add("+transdiff");

        args.Add("-y");
        args.Add(_outputPath);

        return args.ToArray();
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
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();

        await process.WaitForExitAsync(ct).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
            throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}. Error: {error}");
        }
    }
}