#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent
{
    /// <summary>
    /// Strategy for concatenating media files.
    /// </summary>
    public enum ConcatStrategy
    {
        /// <summary>
        /// Automatically choose demuxer when inputs are compatible (same codec, resolution, etc.), otherwise use filter.
        /// </summary>
        Auto,
        /// <summary>
        /// Always use the concat demuxer (stream copy). Requires compatible inputs.
        /// </summary>
        DemuxerCopy,
        /// <summary>
        /// Always use the concat filter (re-encode). Works with incompatible inputs.
        /// </summary>
        FilterReencode
    }

    /// <summary>
    /// Preset for concatenating multiple media files using FFmpeg's concat demuxer or filter.
    /// </summary>
    /// <example>
    /// <code>
    /// var preset = new ConcatPreset("output.mp4")
    ///     .WithStrategy(ConcatStrategy.Auto)
    ///     .AddInput("input1.mp4")
    ///     .AddInput("input2.mp4")
    ///     .WithReencode("libx264", "aac");
    /// await preset.RunAsync();
    /// </code>
    /// </example>
    public sealed class ConcatPreset
    {
        private const string DefaultVideoCodec = "libx264";
        private const string DefaultAudioCodec = "aac";
        private const string FormatOption = "-f";
        private const string ConcatDemuxerFormat = "concat";
        private const string SafeOption = "-safe";
        private const string InputOption = "-i";
        private const string VideoCodecOption = "-c:v";
        private const string AudioCodecOption = "-c:a";
        private const string CodecOption = "-c";
        private const string CopyCodec = "copy";

        private string? _outputPath;
        private readonly List<string> _inputs = new();
        private bool _reencode;
        private string _videoCodec = DefaultVideoCodec;
        private string _audioCodec = DefaultAudioCodec;
        private ConcatStrategy strategy = ConcatStrategy.Auto;

        private ConcatPreset()
        {
        }

        /// <summary>
        /// Creates a fluent concat command builder.
        /// </summary>
        /// <returns>A new concat preset builder.</returns>
        public static ConcatPreset Create() => new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatPreset"/> class.
        /// </summary>
        /// <param name="outputPath">The path where the concatenated file will be written.</param>
        public ConcatPreset(string outputPath)
        {
            _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
        }

        /// <summary>
        /// Sets the concatenation strategy.
        /// </summary>
        /// <param name="strategy">The concatenation strategy to use.</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public ConcatPreset WithStrategy(ConcatStrategy strategy)
        {
            this.strategy = strategy;
            return this;
        }

        /// <summary>
        /// Adds an input file to be concatenated.
        /// </summary>
        /// <param name="path">Path to the input file.</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public ConcatPreset AddInput(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Input path cannot be null or whitespace.", nameof(path));

            _inputs.Add(path);
            return this;
        }

        /// <summary>
        /// Adds input files to be concatenated, in enumeration order.
        /// </summary>
        /// <param name="paths">Paths to the input files.</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public ConcatPreset AddInputs(IEnumerable<string> paths)
        {
            ArgumentNullException.ThrowIfNull(paths);

            foreach (var path in paths)
                AddInput(path);

            return this;
        }

        /// <summary>
        /// Sets the output path for a command created by <see cref="Create"/>.
        /// </summary>
        /// <param name="path">Path to the output file.</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public ConcatPreset Output(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Output path cannot be null or whitespace.", nameof(path));

            _outputPath = path;
            return this;
        }

        /// <summary>
        /// Selects whether the command should re-encode its streams.
        /// </summary>
        /// <param name="enabled"><see langword="true"/> to use the concat filter and default codecs; otherwise stream copy is used.</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public ConcatPreset ReEncode(bool enabled)
        {
            _reencode = enabled;
            return this;
        }

        /// <summary>
        /// Builds an <see cref="FFmpegCommand"/> for the configured concatenation.
        /// </summary>
        /// <returns>The configured command.</returns>
        /// <exception cref="InvalidOperationException">Fewer than two inputs were added, or no output was set.</exception>
        public FFmpegCommand Build()
        {
            if (_inputs.Count < 2)
                throw new InvalidOperationException("At least two input files are required for concatenation.");
            if (string.IsNullOrWhiteSpace(_outputPath))
                throw new InvalidOperationException("An output path is required for concatenation.");

            var command = FFmpegCommand.Create();
            foreach (var input in _inputs)
                command.AddInput(input);

            if (_reencode)
            {
                var streamInputs = string.Concat(
                    Enumerable.Range(0, _inputs.Count).Select(i => $"[{i}:v][{i}:a]"));
                command.WithFilterGraph(graph =>
                    graph.AddFilter($"{streamInputs}concat=n={_inputs.Count}:v=1:a=1[v][a]"));
                command.AddOutput(_outputPath, output => output
                    .Option("map", "[v]")
                    .Option("map", "[a]")
                    .WithVideo(video => video.Codec(_videoCodec))
                    .WithAudio(audio => audio.Codec(_audioCodec)));
            }
            else
            {
                command.AddOutput(_outputPath, output => output.Option("c", CopyCodec));
            }

            return command;
        }

        /// <summary>
        /// Configures the preset to re‑encode the output using the specified codecs.
        /// If not called, the preset will use <c>-c copy</c> in demuxer mode.
        /// </summary>
        /// <param name="videoCodec">Video codec to use (default: libx264).</param>
        /// <param name="audioCodec">Audio codec to use (default: aac).</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public ConcatPreset WithReencode(
            string videoCodec = DefaultVideoCodec,
            string audioCodec = DefaultAudioCodec)
        {
            _reencode = true;
            _videoCodec = videoCodec ?? throw new ArgumentNullException(nameof(videoCodec));
            _audioCodec = audioCodec ?? throw new ArgumentNullException(nameof(audioCodec));
            return this;
        }

        /// <summary>
        /// Builds the content for the temporary concat list file.
        /// </summary>
        /// <returns>A string suitable for FFmpeg's concat demuxer.</returns>
        public string BuildConcatListContent()
        {
            if (_inputs.Count == 0)
                throw new InvalidOperationException("No input files have been added.");

            var lines = new List<string>(_inputs.Count);
            foreach (var input in _inputs)
            {
                // Escape single quotes for FFmpeg list syntax.
                var escaped = input.Replace("'", @"'\''");
                lines.Add($"file '{escaped}'");
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Checks if all inputs are compatible for concat demuxer (same codec, resolution, etc.).
        /// </summary>
        private async Task<bool> AreInputsCompatibleForDemuxerAsync()
        {
            if (_inputs.Count == 0)
                return false;

            MediaInfo? firstInfo = null;

            foreach (var input in _inputs)
            {
                var info = await MediaInfo.ProbeAsync(input);
                if (firstInfo == null)
                {
                    firstInfo = info;
                }
                else
                {
                    if (!AreCompatible(firstInfo, info))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if two media infos are compatible for concat demuxer.
        /// </summary>
        private static bool AreCompatible(MediaInfo x, MediaInfo y)
        {
            // Video
            if (x.VideoCodec != null)
            {
                if (y.VideoCodec == null)
                    return false;
                if (!string.Equals(x.VideoCodec, y.VideoCodec, StringComparison.OrdinalIgnoreCase))
                    return false;
                if (x.Width != y.Width || x.Height != y.Height)
                    return false;
                if (x.FrameRate != y.FrameRate)
                    return false;
            }
            else
            {
                if (y.VideoCodec != null)
                    return false;
            }

            // Audio
            if (x.AudioCodec != null)
            {
                if (y.AudioCodec == null)
                    return false;
                if (!string.Equals(x.AudioCodec, y.AudioCodec, StringComparison.OrdinalIgnoreCase))
                    return false;
                if (x.SampleRate != y.SampleRate)
                    return false;
                if (x.AudioChannels != y.AudioChannels)
                    return false;
            }
            else
            {
                if (y.AudioCodec != null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Executes the concatenation using FFmpeg.
        /// </summary>
        /// <param name="ffmpegPath">Path to the ffmpeg executable (default: "ffmpeg").</param>
        /// <param name="ct">Cancellation token.</param>
        /// <example>
        /// <code>
        /// var preset = new ConcatPreset("output.mp4")
        ///     .AddInput("clip1.mp4")
        ///     .AddInput("clip2.mp4");
        /// await preset.RunAsync();
        /// </code>
        /// </example>
        public async Task RunAsync(string ffmpegPath = "ffmpeg", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(ffmpegPath))
                throw new ArgumentException("ffmpegPath cannot be null or whitespace.", nameof(ffmpegPath));

            bool useDemuxer = false;
            if (strategy == ConcatStrategy.DemuxerCopy)
            {
                useDemuxer = true;
            }
            else if (strategy == ConcatStrategy.Auto)
            {
                useDemuxer = await AreInputsCompatibleForDemuxerAsync();
            }
            // else: FilterReencode -> useDemuxer remains false

            if (useDemuxer)
            {
                // Demuxer mode
                var listContent = BuildConcatListContent();

                var tempFile = Path.GetTempFileName();
                try
                {
                    await File.WriteAllTextAsync(tempFile, listContent, ct).ConfigureAwait(false);

                    string effectiveVideoCodec = _videoCodec ?? DefaultVideoCodec;
                    string effectiveAudioCodec = _audioCodec ?? DefaultAudioCodec;

                    var args = $"{FormatOption} {ConcatDemuxerFormat} {SafeOption} 0 {InputOption} \"{tempFile}\" ";

                    if (_reencode)
                    {
                        args += $"{VideoCodecOption} {effectiveVideoCodec} {AudioCodecOption} {effectiveAudioCodec} ";
                    }
                    else
                    {
                        args += $"{CodecOption} {CopyCodec} ";
                    }

                    args += $"\"{_outputPath}\"";

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
                    var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

                    process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);
                    process.Start();

                    // Drain output streams to avoid deadlocks.
                    _ = process.StandardOutput.ReadToEndAsync(ct);
                    _ = process.StandardError.ReadToEndAsync(ct);

                    using (ct.Register(() =>
                    {
                        try { if (!process.HasExited) process.Kill(); } catch { }
                    }))
                    {
                        var exitCode = await tcs.Task.WaitAsync(ct).ConfigureAwait(false);
                        if (exitCode != 0)
                            throw new InvalidOperationException($"ffmpeg exited with code {exitCode}.");
                    }
                }
                finally
                {
                    try { File.Delete(tempFile); } catch { /* ignore cleanup failures */ }
                }
            }
            else
            {
                // Filter mode
                if (_inputs.Count == 0)
                    throw new InvalidOperationException("No input files have been added.");

                // Build input arguments: -i input1 -i input2 ...
                var inputArgs = string.Join(" ", _inputs.Select(input => $"{InputOption} \"{input}\""));

                // Build filter_complex for concat filter
                var videoInputs = string.Concat(Enumerable.Range(0, _inputs.Count).Select(i => $"[{i}:v]"));
                var audioInputs = string.Concat(Enumerable.Range(0, _inputs.Count).Select(i => $"[{i}:a]"));
                var filter = $"{videoInputs}{audioInputs}concat=n={_inputs.Count}:v=1:a=1[v][a]";

                string effectiveVideoCodec = _videoCodec ?? DefaultVideoCodec;
                string effectiveAudioCodec = _audioCodec ?? DefaultAudioCodec;

                var args = $"{inputArgs} -filter_complex \"{filter}\" -map \"[v]\" -map \"[a]\" {VideoCodecOption} {effectiveVideoCodec} {AudioCodecOption} {effectiveAudioCodec} \"{_outputPath}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
                var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

                process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);
                process.Start();

                // Drain output streams to avoid deadlocks.
                _ = process.StandardOutput.ReadToEndAsync(ct);
                _ = process.StandardError.ReadToEndAsync(ct);

                using (ct.Register(() =>
                {
                    try { if (!process.HasExited) process.Kill(); } catch { }
                }))
                {
                    var exitCode = await tcs.Task.WaitAsync(ct).ConfigureAwait(false);
                    if (exitCode != 0)
                        throw new InvalidOperationException($"ffmpeg exited with code {exitCode}.");
                }
            }
        }
    }
}
