#nullable enable

using System;
using System.Collections.Generic;

namespace FFmpegFluent;

/// <summary>
/// Preset for trimming a video file using FFmpeg.
/// Supports both stream copy mode (fast, no re-encoding) and re-encoding mode.
/// </summary>
public sealed class TrimPreset
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private TimeSpan? _from;
    private TimeSpan? _to;
    private TimeSpan? _duration;
    private bool _streamCopy;
    private string? _videoCodec;
    private string? _audioCodec;

    /// <summary>
    /// Initializes a new instance of <see cref="TrimPreset"/>.
    /// </summary>
    /// <param name="inputPath">Path to the source video.</param>
    /// <param name="outputPath">Path where the trimmed video will be saved.</param>
    public TrimPreset(string inputPath, string outputPath)
    {
        _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
        _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
    }

    /// <summary>
    /// Sets the start time for the trim operation.
    /// </summary>
    public TrimPreset From(TimeSpan from)
    {
        _from = from;
        return this;
    }

    /// <summary>
    /// Sets the end time for the trim operation.
    /// </summary>
    public TrimPreset To(TimeSpan to)
    {
        _to = to;
        return this;
    }

    /// <summary>
    /// Sets the duration for the trim operation.
    /// </summary>
    public TrimPreset Duration(TimeSpan duration)
    {
        _duration = duration;
        return this;
    }

    /// <summary>
    /// Configures the preset to use stream copy mode (no re-encoding) with -c copy.
    /// This is fast but requires exact keyframe alignment.
    /// </summary>
    public TrimPreset StreamCopy()
    {
        _streamCopy = true;
        return this;
    }

    /// <summary>
    /// Configures the preset to re-encode the output using the specified codecs.
    /// </summary>
    /// <param name="videoCodec">Video codec to use (e.g., "libx264"). Defaults to "copy" if not specified.</param>
    /// <param name="audioCodec">Audio codec to use (e.g., "aac"). Defaults to "copy" if not specified.</param>
    public TrimPreset WithReencode(string? videoCodec = null, string? audioCodec = null)
    {
        _streamCopy = false;
        _videoCodec = videoCodec;
        _audioCodec = audioCodec;
        return this;
    }

    /// <summary>
    /// Builds the FFmpeg command for the trim operation.
    /// </summary>
    /// <returns>A configured <see cref="FFmpegCommand"/> instance.</returns>
    public FFmpegCommand Build()
    {
        var command = FFmpegCommand.Create();

        // Add input file
        command.AddInput(_inputPath, input =>
        {
            if (_from.HasValue)
            {
                input.Seek(_from.Value);
            }

            if (_duration.HasValue)
            {
                input.Duration(_duration.Value);
            }
        });

        // Add output file
        command.AddOutput(_outputPath, output =>
        {
            output.Overwrite();

            if (_streamCopy)
            {
                output.Option("c", "copy");
            }
            else if (_videoCodec != null || _audioCodec != null)
            {
                output.Option("c:v", _videoCodec ?? "copy");
                output.Option("c:a", _audioCodec ?? "copy");
            }

            // If using To() without Duration(), calculate duration from start to end
            if (_to.HasValue && !_duration.HasValue && _from.HasValue)
            {
                var calculatedDuration = _to.Value - _from.Value;
                if (calculatedDuration > TimeSpan.Zero)
                {
                    output.Option("t", calculatedDuration.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        });

        return command;
    }

    /// <summary>
    /// Builds the argument list that will be passed to ffmpeg.
    /// </summary>
    public string[] BuildArguments()
    {
        var command = Build();
        return command.BuildCommandLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Executes the trim operation using FFmpeg.
    /// </summary>
    /// <param name="ffmpegPath">Path to the ffmpeg executable. Defaults to "ffmpeg".</param>
    public void Run(string ffmpegPath = "ffmpeg")
    {
        var command = Build();
        var exitCode = command.RunAsync().Result;
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"ffmpeg exited with code {exitCode}.");
        }
    }
}
