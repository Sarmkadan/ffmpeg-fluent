#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

/// <summary>
/// Preset for burning subtitles into a video using FFmpeg.
/// </summary>
public sealed class SubtitlePreset
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private string? _subtitlePath;

    /// <summary>
    /// Initializes a new instance of <see cref="SubtitlePreset"/>.
    /// </summary>
    /// <param name="inputPath">Path to the source video.</param>
    /// <param name="outputPath">Path where the output video with subtitles will be saved.</param>
    public SubtitlePreset(string inputPath, string outputPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(inputPath);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        _inputPath = inputPath;
        _outputPath = outputPath;
    }

    /// <summary>
    /// Specifies the subtitle file to burn into the video.
    /// </summary>
    /// <param name="subtitlePath">Path to the subtitle file (e.g., .srt, .ass).</param>
    /// <returns>The current instance of <see cref="SubtitlePreset"/>.</returns>
    public SubtitlePreset WithSubtitle(string subtitlePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(subtitlePath);
        _subtitlePath = subtitlePath;
        return this;
    }

    /// <summary>
    /// Builds a <see cref="FFmpegCommand"/> that performs the subtitle burn‑in.
    /// </summary>
    /// <returns>A configured <see cref="FFmpegCommand"/> instance.</returns>
    public FFmpegCommand Build()
    {
        if (_subtitlePath is null)
        {
            throw new InvalidOperationException("Subtitle path must be specified via WithSubtitle().");
        }

        var command = FFmpegCommand.Create();

        command.AddInput(_inputPath);

        command.AddOutput(_outputPath, output =>
        {
            output.Overwrite();
            var escaped = EscapePath(_subtitlePath);
            output.Option("vf", $"subtitles={escaped}");
        });

        return command;
    }

    /// <summary>
    /// Builds the raw argument list that will be passed to ffmpeg.
    /// Mirrors the approach used in <see cref="GifPreset"/>.
    /// </summary>
    /// <returns>An array of arguments for the ffmpeg command.</returns>
    public string[] BuildArguments()
    {
        if (_subtitlePath is null)
        {
            throw new InvalidOperationException("Subtitle path must be specified via WithSubtitle().");
        }

        var args = new List<string>();
        args.Add("-y");
        args.Add("-i");
        args.Add(_inputPath);
        args.Add("-vf");
        args.Add($"subtitles={EscapePath(_subtitlePath)}");
        args.Add(_outputPath);
        return args.ToArray();
    }

    /// <summary>
    /// Executes the ffmpeg command to burn subtitles into the video.
    /// </summary>
    /// <param name="ffmpegPath">Path to the ffmpeg executable. Defaults to "ffmpeg".</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync(string ffmpegPath = "ffmpeg", CancellationToken ct = default)
    {
        var arguments = BuildArguments();

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                // Quote arguments that contain spaces.
                Arguments = string.Join(" ", arguments.Select(a => a.Contains(' ') ? $"\"{a}\"" : a)),
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

    /// <summary>
    /// Escapes a file path for inclusion in the ffmpeg subtitles filter.
    /// Single quotes are escaped with a backslash.
    /// </summary>
    private static string EscapePath(string path)
    {
        // ffmpeg expects the path to be quoted; we escape any single quotes inside.
        return $"'{path.Replace("'", @"\'")}'";
    }
}
