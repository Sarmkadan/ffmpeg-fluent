#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

public sealed class FFmpegCommand
{
    private readonly string _ffmpegPath;
    private readonly List<InputFile> _inputs = [];
    private readonly List<OutputFile> _outputs = [];
    private readonly FilterGraph _filterGraph = new();
    private readonly List<string> _globalOptions = [];

    private FFmpegCommand(string ffmpegPath)
    {
        _ffmpegPath = ffmpegPath;
    }

    public static FFmpegCommand Create(string ffmpegPath = "ffmpeg") => new(ffmpegPath);

    public FFmpegCommand AddInput(string path, Action<InputFile>? cfg = null)
    {
        var inputFile = new InputFile(path);
        cfg?.Invoke(inputFile);
        _inputs.Add(inputFile);
        return this;
    }

    public FFmpegCommand WithFilterGraph(Action<FilterGraph> cfg)
    {
        cfg(_filterGraph);
        return this;
    }

    public FFmpegCommand AddOutput(string path, Action<OutputFile>? cfg = null)
    {
        var outputFile = new OutputFile(path);
        cfg?.Invoke(outputFile);
        _outputs.Add(outputFile);
        return this;
    }

    public FFmpegCommand GlobalOption(string key, string? value = null)
    {
        _globalOptions.Add(value is null ? $"-{key}" : $"-{key} {value}");
        return this;
    }

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

    public async Task<int> RunAsync(IProgress<FFmpegProgress>? progress = null, CancellationToken ct = default)
    {
        var process = new Process
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
        _ = Task.Run(async () =>
        {
            while (!process.StandardError.EndOfStream)
            {
                var line = await process.StandardError.ReadLineAsync(ct);
                stderrLines.Enqueue(line);
                if (progress != null && FFmpegProgress.TryParse(line, out var ffmpegProgress))
                {
                    progress.Report(ffmpegProgress);
                }
            }
        }, ct);

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            var errorMessage = string.Join(Environment.NewLine, stderrLines);
            throw new FFmpegException(BuildCommandLine(), process.ExitCode, errorMessage);
        }

        return process.ExitCode;
    }
}
