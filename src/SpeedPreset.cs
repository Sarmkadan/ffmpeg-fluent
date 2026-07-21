#nullable enable

using System;
using System.Collections.Generic;

namespace FFmpegFluent;

/// <summary>
/// Preset for changing the playback speed of a video file using FFmpeg.
/// Supports both video and audio speed adjustments with proper filtering.
/// Uses atempo filter for audio (0.5-2.0 range) and setpts filter for video.
/// For factors outside 0.5-2.0, atempo filters are chained together.
/// </summary>
public sealed class SpeedPreset
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private double _speedFactor = 1.0;
    private bool _preservePitch = false;
    private string? _videoCodec;
    private string? _audioCodec;

    /// <summary>
    /// Initializes a new instance of <see cref="SpeedPreset"/>.
    /// </summary>
    /// <param name="inputPath">Path to the source video.</param>
    /// <param name="outputPath">Path where the speed-adjusted video will be saved.</param>
    public SpeedPreset(string inputPath, string outputPath)
    {
        _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
        _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
    }

    /// <summary>
    /// Sets the playback speed factor.
    /// A value of 1.0 means normal speed, 2.0 means double speed, 0.5 means half speed.
    /// </summary>
    /// <param name="factor">The speed factor to apply (e.g., 2.0 for 2x speed).</param>
    /// <returns>The current instance of <see cref="SpeedPreset"/> for fluent chaining.</returns>
    public SpeedPreset WithSpeed(double factor)
    {
        if (factor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), "Speed factor must be greater than 0.");
        }

        _speedFactor = factor;
        return this;
    }

    /// <summary>
    /// Configures whether to preserve the original audio pitch when changing speed.
    /// When true, uses the "rubberband" filter for high-quality pitch preservation.
    /// When false, audio will be higher or lower in pitch with the speed change.
    /// </summary>
    /// <param name="preserve">Whether to preserve audio pitch (default: false).</param>
    /// <returns>The current instance of <see cref="SpeedPreset"/> for fluent chaining.</returns>
    public SpeedPreset PreservePitch(bool preserve = true)
    {
        _preservePitch = preserve;
        return this;
    }

    /// <summary>
    /// Configures the preset to re-encode the output using the specified codecs.
    /// </summary>
    /// <param name="videoCodec">Video codec to use (e.g., "libx264"). Defaults to "copy" if not specified.</param>
    /// <param name="audioCodec">Audio codec to use (e.g., "aac"). Defaults to "copy" if not specified.</param>
    /// <returns>The current instance of <see cref="SpeedPreset"/> for fluent chaining.</returns>
    public SpeedPreset WithReencode(string? videoCodec = null, string? audioCodec = null)
    {
        _videoCodec = videoCodec;
        _audioCodec = audioCodec;
        return this;
    }

    /// <summary>
    /// Builds the FFmpeg command for the speed adjustment operation.
    /// </summary>
    /// <returns>A configured <see cref="FFmpegCommand"/> instance.</returns>
    public FFmpegCommand Build()
    {
        var command = FFmpegCommand.Create();

        // Add input file
        command.AddInput(_inputPath);

        // Add output file
        command.AddOutput(_outputPath, output =>
        {
            output.Overwrite();

            // Handle video speed change
            if (Math.Abs(_speedFactor - 1.0) > 0.001)
            {
                HandleVideoSpeed(output);
            }

            // Handle audio speed change and pitch preservation
            if (Math.Abs(_speedFactor - 1.0) > 0.001)
            {
                HandleAudioSpeed(output);
            }

            // Configure codecs if re-encoding
            if (_videoCodec != null || _audioCodec != null)
            {
                output.Option("c:v", _videoCodec ?? "copy");
                output.Option("c:a", _audioCodec ?? "copy");
            }
        });

        return command;
    }

    /// <summary>
    /// Builds the argument list that will be passed to ffmpeg.
    /// </summary>
    /// <returns>A string array containing the FFmpeg arguments.</returns>
    public string[] BuildArguments()
    {
        var command = Build();
        return command.BuildCommandLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Executes the speed adjustment operation using FFmpeg.
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

    private void HandleVideoSpeed(OutputFile output)
    {
        // Calculate the setpts value (inverse of speed factor)
        // setpts=PTS/play_speed
        var setptsValue = 1.0 / _speedFactor;

        // Add video speed filter
        output.Option("filter:v", $"setpts={setptsValue}*PTS");

        // If re-encoding video, also set the video codec
        if (_videoCodec != null)
        {
            output.Option("c:v", _videoCodec);
        }
    }

    private void HandleAudioSpeed(OutputFile output)
    {
        // Calculate the atempo value(s)
        // atempo only supports 0.5-2.0 range, so we chain multiple filters for other values
        var tempo = _speedFactor;

        if (tempo < 0.5 || tempo > 2.0)
        {
            // Chain atempo filters for values outside the 0.5-2.0 range
            var atempoCount = 0;
            var remainingTempo = tempo;

            while (remainingTempo < 0.5 || remainingTempo > 2.0)
            {
                // For very small values, use 0.5 as the base
                var filterValue = Math.Max(0.5, Math.Min(2.0, remainingTempo));
                output.Option("filter:a", $"atempo={filterValue}");
                remainingTempo /= filterValue;
                atempoCount++;
            }

            // Apply the final atempo filter with the remaining tempo
            if (Math.Abs(remainingTempo - 1.0) > 0.001)
            {
                output.Option("filter:a", $"atempo={remainingTempo}");
            }
        }
        else if (Math.Abs(tempo - 1.0) > 0.001)
        {
            // Single atempo filter for values in the 0.5-2.0 range
            output.Option("filter:a", $"atempo={tempo}");
        }

        // Handle pitch preservation if requested
        if (_preservePitch && _speedFactor != 1.0)
        {
            // Use rubberband filter for high-quality pitch preservation
            // Note: rubberband filter may need to be installed separately
            output.Option("af", "rubberband=pitch=1");
        }

        // If re-encoding audio, also set the audio codec
        if (_audioCodec != null)
        {
            output.Option("c:a", _audioCodec);
        }
    }

    /// <summary>
    /// Returns a string representation of the preset configuration.
    /// </summary>
    public override string ToString() =>
        $"speed(speed_factor={_speedFactor},preserve_pitch={_preservePitch},video_codec={_videoCodec},audio_codec={_audioCodec})";
}
