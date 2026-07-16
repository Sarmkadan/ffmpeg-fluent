using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

/// <summary>
/// Represents a preset for creating GIFs using FFmpeg.
/// </summary>
public sealed class GifPreset
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private int _fps = 10;
    private int? _width;
    private TimeSpan? _start;
    private TimeSpan? _duration;

    /// <summary>
    /// Initializes a new instance of the <see cref="GifPreset"/> class.
    /// </summary>
    /// <param name="inputPath">The path to the input file.</param>
    /// <param name="outputPath">The path to the output file.</param>
    public GifPreset(string inputPath, string outputPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(inputPath);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        _inputPath = inputPath;
        _outputPath = outputPath;
    }

    /// <summary>
    /// Sets the frames per second (FPS) for the GIF.
    /// </summary>
    /// <param name="fps">The FPS value.</param>
    /// <returns>The current instance of the <see cref="GifPreset"/> class.</returns>
    public GifPreset WithFps(int fps)
    {
        if (fps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fps), "FPS must be positive");
        }

        _fps = fps;
        return this;
    }

    /// <summary>
    /// Sets the width of the GIF.
    /// </summary>
    /// <param name="width">The width value.</param>
    /// <returns>The current instance of the <see cref="GifPreset"/> class.</returns>
    public GifPreset WithWidth(int width)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive");
        }

        _width = width;
        return this;
    }

    /// <summary>
    /// Sets the time range for the GIF.
    /// </summary>
    /// <param name="start">The start time.</param>
    /// <param name="duration">The duration.</param>
    /// <returns>The current instance of the <see cref="GifPreset"/> class.</returns>
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

    /// <summary>
    /// Builds the arguments for the FFmpeg command.
    /// </summary>
    /// <returns>An array of arguments for the FFmpeg command.</returns>
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

        args.Add("-filter_complex");

        var preFilters = new System.Collections.Generic.List<string>();

        if (_width.HasValue)
        {
            preFilters.Add($"scale={_width.Value}:-1:flags=lanczos");
        }

        preFilters.Add($"fps={_fps.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        preFilters.Add("split[s0][s1]");

        // Linear filters are comma-chained; labeled palette segments are separate
        // chains and must be joined with ';' (which also requires -filter_complex,
        // since -vf only accepts a single linear chain).
        var filterComplex = string.Join(",", preFilters)
            + ";[s0]palettegen=stats_mode=diff[p]"
            + ";[s1][p]paletteuse";

        args.Add(filterComplex);

        args.Add("-gifflags");
        args.Add("+transdiff");

        args.Add(_outputPath);

        return args.ToArray();
    }

    /// <summary>
    /// Runs the FFmpeg command to create the GIF.
    /// </summary>
    /// <param name="ffmpegPath">The path to the FFmpeg executable.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync(string ffmpegPath = "ffmpeg", CancellationToken ct = default)
    {
        var arguments = BuildArguments();
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = string.Join(" ", System.Linq.Enumerable.Select(arguments, a => a.Contains(' ') ? $"\"{a}\"" : a)),
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
