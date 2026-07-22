#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
    internal string? _passLogFilePath;

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
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or whitespace.</exception>
    public FFmpegCommand GlobalOption(string key, string? value = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _globalOptions.Add(value is null ? $"-{key}" : $"-{key} {ArgumentEscaper.EscapeArgument(value)}");
        return this;
    }

    /// <summary>
    /// Configures two-pass encoding for the command.
    /// </summary>
    /// <param name="logFilePath">Path to the file where FFmpeg will store pass statistics. If null, a temporary file will be created.</param>
    /// <returns>The current instance of the <see cref="FFmpegCommand"/> class.</returns>
    /// <remarks>
    /// Two-pass encoding improves quality for variable bitrate codecs like libx264.
    /// Call <see cref="BuildPassArguments"/> to get the arguments for each pass.
    /// </remarks>
    public FFmpegCommand WithTwoPass(string? logFilePath = null)
    {
        _passLogFilePath = logFilePath ?? Path.GetTempFileName();
        return this;
    }

    /// <summary>
    /// Builds the command line arguments for the specified encoding pass.
    /// </summary>
    /// <param name="pass">The pass number (1 or 2).</param>
    /// <returns>An <see cref="IEnumerable"/> of <see cref="string"/> containing the arguments for the specified pass.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pass"/> is not 1 or 2.</exception>
    public IEnumerable<string> BuildPassArguments(int pass)
    {
        if (pass != 1 && pass != 2)
        {
            throw new ArgumentOutOfRangeException(nameof(pass), "Pass must be 1 or 2.");
        }

        if (_passLogFilePath == null)
        {
            throw new InvalidOperationException("Two-pass encoding not configured. Call WithTwoPass() first.");
        }

        var args = new List<string>(_globalOptions);

        foreach (var input in _inputs)
        {
            args.AddRange(input.BuildArgs());
        }

        if (!_filterGraph.IsEmpty)
        {
            args.Add($"-filter_complex {ArgumentEscaper.EscapeFilterGraph(_filterGraph.Build())}");
        }

        foreach (var output in _outputs)
        {
            args.AddRange(output.BuildArgs());
        }

        // Add pass-specific options
        if (pass == 1)
        {
            // First pass: analyze and write stats, no audio, output to null
            args.Add("-pass");
            args.Add("1");
            args.Add("-an");
            args.Add("-f");
            args.Add("null");
            args.Add(OperatingSystem.IsWindows() ? "NUL" : "nul");
        }
        else if (pass == 2)
        {
            // Second pass: use stats from first pass
            args.Add("-pass");
            args.Add("2");
        }

        // Add pass log file path
        args.Add($"-passlogfile {_passLogFilePath}");

        return args;
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
            args.Add($"-filter_complex {ArgumentEscaper.EscapeFilterGraph(_filterGraph.Build())}");
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
        await RunAsync(progress, progressAction, timeout: null, ct).ConfigureAwait(false);
        return 0;
    }

    /// <summary>
    /// Runs the command asynchronously with a timeout.
    /// </summary>
    /// <param name="timeout">The maximum duration to allow for the command to complete. If null, no timeout is enforced.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>The exit code of the command.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeout"/> is negative.</exception>
    public Task<int> RunAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        if (timeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be non-negative.");
        }
        return RunAsync(null, null, timeout, ct);
    }

    /// <summary>
    /// Runs the command asynchronously with a timeout.
    /// </summary>
    /// <param name="timeout">The maximum duration to allow for the command to complete. If null, no timeout is enforced.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>The exit code of the command.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeout"/> is negative.</exception>
    public Task<int> RunAsync(TimeSpan? timeout, CancellationToken ct = default)
    {
        if (timeout.HasValue && timeout.Value < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be non-negative.");
        }
        return RunAsync(null, null, timeout, ct);
    }

    /// <summary>
    /// Runs the command asynchronously with progress reporting and optional timeout.
    /// </summary>
    /// <param name="progress">An optional progress reporter.</param>
    /// <param name="progressAction">An optional progress callback action.</param>
    /// <param name="timeout">The maximum duration to allow for the command to complete. If null, no timeout is enforced.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>The exit code of the command.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeout"/> is negative.</exception>
    public async Task<int> RunAsync(IProgress<FFmpegProgress>? progress, Action<FFmpegProgress>? progressAction, TimeSpan? timeout, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(_ffmpegPath);

        if (timeout.HasValue && timeout.Value < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be non-negative.");
        }

        using var process = new Process
        {
            StartInfo =
            {
                FileName = _ffmpegPath,
                Arguments = BuildCommandLine(),
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        // Start stderr reader
        var stderrLines = new Queue<string>();
        var stderrReadTask = Task.Run(async () =>
        {
            try
            {
                while (!process.StandardError.EndOfStream)
                {
                    var line = await process.StandardError.ReadLineAsync(ct).ConfigureAwait(false);
                    if (line is null)
                    {
                        continue;
                    }

                    stderrLines.Enqueue(line);

                    // Try to parse structured progress data
                    if (progress != null && FFmpegProgress.TryParse(line, out var ffmpegProgress) && ffmpegProgress != null)
                    {
                        progress.Report(ffmpegProgress);
                    }

                    if (progressAction != null && FFmpegProgress.TryParse(line, out var ffmpegProgress2) && ffmpegProgress2 != null)
                    {
                        progressAction(ffmpegProgress2);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }, ct);

        // Handle timeout and cancellation
        Task<int> executionTask;
        if (timeout.HasValue)
        {
            executionTask = ExecuteWithTimeoutAsync(process, timeout.Value, ct, stderrLines, stderrReadTask);
        }
        else
        {
            executionTask = ExecuteWithoutTimeoutAsync(process, ct, stderrLines, stderrReadTask);
        }

        return await executionTask.ConfigureAwait(false);
    }

    private async Task<int> ExecuteWithoutTimeoutAsync(Process process, CancellationToken ct, Queue<string> stderrLines, Task stderrReadTask)
    {
        await Task.WhenAll(
            process.WaitForExitAsync(ct),
            stderrReadTask
        ).ConfigureAwait(false);

        return await GetResultAsync(process, stderrLines).ConfigureAwait(false);
    }

    private async Task<int> ExecuteWithTimeoutAsync(Process process, TimeSpan timeout, CancellationToken ct, Queue<string> stderrLines, Task stderrReadTask)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var timeoutTask = Task.Delay(timeout, timeoutCts.Token);
        var processExitTask = process.WaitForExitAsync(ct);

        var completedTask = await Task.WhenAny(
            processExitTask,
            timeoutTask,
            stderrReadTask
        ).ConfigureAwait(false);

        if (completedTask == timeoutTask)
        {
            // Timeout occurred - need to cancel the process
            await HandleTimeoutAsync(process, timeoutCts).ConfigureAwait(false);
            throw new FFmpegException(BuildCommandLine(), -1, $"FFmpeg command timed out after {timeout.TotalSeconds} seconds.");
        }
        else if (completedTask == processExitTask)
        {
            // Process completed normally
            timeoutCts.Cancel(); // Cancel the timeout task
            await Task.WhenAll(stderrReadTask).ConfigureAwait(false);
            return await GetResultAsync(process, stderrLines).ConfigureAwait(false);
        }
        else
        {
            // stderrReadTask completed - wait for process to exit
            await Task.WhenAll(processExitTask, stderrReadTask).ConfigureAwait(false);
            return await GetResultAsync(process, stderrLines).ConfigureAwait(false);
        }
    }

    private async Task HandleTimeoutAsync(Process process, CancellationTokenSource timeoutCts)
    {
        try
        {
            // First, try graceful shutdown by sending 'q' to stdin (FFmpeg's quit command)
            if (!process.HasExited && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Unix-like systems, try sending SIGTERM first
                process.Kill(entireProcessTree: true);
            }
            else if (!process.HasExited)
            {
                // On Windows, try writing 'q' to stdin if process has stdin
                try
                {
                    await process.StandardInput.WriteAsync("q");
                    await process.StandardInput.FlushAsync();
                    await Task.Delay(200).ConfigureAwait(false); // Give it a moment to respond

                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch
                {
                    // If stdin write fails, kill the process
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
            }
        }
        finally
        {
            timeoutCts.Cancel();
        }
    }

    private async Task<int> GetResultAsync(Process process, Queue<string> stderrLines)
    {
        if (process.ExitCode != 0)
        {
            var errorMessage = string.Join(Environment.NewLine, stderrLines);
            throw new FFmpegException(BuildCommandLine(), process.ExitCode, errorMessage);
        }

        return process.ExitCode;
    }
}