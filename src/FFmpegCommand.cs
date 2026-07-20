#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

/// <summary>
/// Represents a command to be executed with FFmpeg.
/// </summary>
public sealed class FFmpegCommand
{
    internal readonly string _ffmpegPath;
    internal readonly List<InputFile> _inputs = [];
    internal readonly List<OutputFile> _outputs = [];
    internal readonly FilterGraph _filterGraph = new();
    internal readonly List<string> _globalOptions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegCommand"/> class.
    /// </summary>
    /// <param name="ffmpegPath">The path to the FFmpeg executable.</param>
    private FFmpegCommand(string ffmpegPath)
    {
        _ffmpegPath = ffmpegPath;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="FFmpegCommand"/> class.
    /// </summary>
    /// <param name="ffmpegPath">The path to the FFmpeg executable. Defaults to "ffmpeg".</param>
    /// <returns>A new instance of the <see cref="FFmpegCommand"/> class.</returns>
    public static FFmpegCommand Create(string ffmpegPath = "ffmpeg") => new(ffmpegPath);

    /// <summary>
    /// Adds an input file to the command.
    /// </summary>
    /// <param name="path">The path to the input file.</param>
    /// <param name="cfg">An optional configuration action for the input file.</param>
    /// <returns>The current instance of the <see cref="FFmpegCommand"/> class.</returns>
    public FFmpegCommand AddInput(string path, Action<InputFile>? cfg = null)
    {
        var inputFile = new InputFile(path);
        cfg?.Invoke(inputFile);
        _inputs.Add(inputFile);
        return this;
    }

    /// <summary>
    /// Configures the filter graph for the command.
    /// </summary>
    /// <param name="cfg">An action to configure the filter graph.</param>
    /// <returns>The current instance of the <see cref="FFmpegCommand"/> class.</returns>
    public FFmpegCommand WithFilterGraph(Action<FilterGraph> cfg)
    {
        cfg(_filterGraph);
        return this;
    }

    /// <summary>
    /// Adds an output file to the command.
    /// </summary>
    /// <param name="path">The path to the output file.</param>
    /// <param name="cfg">An optional configuration action for the output file.</param>
    /// <returns>The current instance of the <see cref="FFmpegCommand"/> class.</returns>
    public FFmpegCommand AddOutput(string path, Action<OutputFile>? cfg = null)
    {
        var outputFile = new OutputFile(path);
        cfg?.Invoke(outputFile);
        _outputs.Add(outputFile);
        return this;
    }

    /// <summary>
    /// Adds a global option to the command.
    /// </summary>
    /// <param name="key">The key of the global option.</param>
    /// <param name="value">The value of the global option. Defaults to null.</param>
    /// <returns>The current instance of the <see cref="FFmpegCommand"/> class.</returns>
    public FFmpegCommand GlobalOption(string key, string? value = null)
    {
        _globalOptions.Add(value is null ? $"-{key}" : $"-{key} {value}");
        return this;
    }

    /// <summary>
    /// Builds the command line arguments for the command.
    /// </summary>
    /// <returns>The command line arguments as a string.</returns>
    public string BuildCommandLine()
    {
        var args = new List<string>(_globalOptions);

        foreach (var input in _inputs)
        {
            args.AddRange(input.BuildArgs());
        }

        if (!_filterGraph.IsEmpty)
        {
            args.Add($"-filter_complex {_filterGraph.Build()}");
        }

        foreach (var output in _outputs)
        {
            args.AddRange(output.BuildArgs());
        }

        return string.Join(" ", args);
    }

    /// <summary>
    /// Runs the command asynchronously.
    /// </summary>
    /// <param name="progress">An optional progress reporter.</param>
    /// <param name="progressAction">An optional progress callback action.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>The exit code of the command.</returns>
    public async Task<int> RunAsync(IProgress<FFmpegProgress>? progress = null, Action<FFmpegProgress>? progressAction = null, CancellationToken ct = default)
    {
        using var process = new Process
        {
            StartInfo =
            {
                FileName = _ffmpegPath,
                Arguments = BuildCommandLine(),
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        var stderrLines = new Queue<string>();
        var stderrReadTask = Task.Run(async () =>
        {
            while (!process.StandardError.EndOfStream)
            {
                var line = await process.StandardError.ReadLineAsync(ct);
                if (line is null)
                {
                    continue;
                }

                stderrLines.Enqueue(line);
                if (progress != null && FFmpegProgress.TryParse(line, out var ffmpegProgress))
                {
                    progress.Report(ffmpegProgress);
                }

                if (progressAction != null && FFmpegProgress.TryParse(line, out var ffmpegProgress2))
                {
                    progressAction(ffmpegProgress2);
                }
            }
        }, ct);

        await process.WaitForExitAsync(ct);
        await stderrReadTask.ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            var errorMessage = string.Join(Environment.NewLine, stderrLines);
            throw new FFmpegException(BuildCommandLine(), process.ExitCode, errorMessage);
        }

        return process.ExitCode;
    }
}
