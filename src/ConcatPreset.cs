#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent
{
    /// <summary>
    /// Preset for concatenating multiple media files using FFmpeg's concat demuxer.
    /// </summary>
    public sealed class ConcatPreset
    {
        private readonly string _outputPath;
        private readonly List<string> _inputs = new();
        private bool _reencode;
        private string _videoCodec = "libx264";
        private string _audioCodec = "aac";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatPreset"/> class.
        /// </summary>
        /// <param name="outputPath">The path where the concatenated file will be written.</param>
        public ConcatPreset(string outputPath)
        {
            _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
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
        /// Configures the preset to re‑encode the output using the specified codecs.
        /// If not called, the preset will use <c>-c copy</c>.
        /// </summary>
        /// <param name="videoCodec">Video codec to use (default: libx264).</param>
        /// <param name="audioCodec">Audio codec to use (default: aac).</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public ConcatPreset WithReencode(string videoCodec = "libx264", string audioCodec = "aac")
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
                var escaped = input.Replace("'", "'\\''");
                lines.Add($"file '{escaped}'");
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Executes the concatenation using FFmpeg.
        /// </summary>
        /// <param name="ffmpegPath">Path to the ffmpeg executable (default: "ffmpeg").</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task RunAsync(string ffmpegPath = "ffmpeg", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(ffmpegPath))
                throw new ArgumentException("ffmpegPath cannot be null or whitespace.", nameof(ffmpegPath));

            var listContent = BuildConcatListContent();

            // Create a temporary file for the concat list.
            var tempFile = Path.GetTempFileName();
            try
            {
                await File.WriteAllTextAsync(tempFile, listContent, ct).ConfigureAwait(false);

                var args = $"-f concat -safe 0 -i \"{tempFile}\" ";

                if (_reencode)
                {
                    args += $"-c:v {_videoCodec} -c:a {_audioCodec} ";
                }
                else
                {
                    args += "-c copy ";
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
                    var exitCode = await tcs.Task.ConfigureAwait(false);
                    if (exitCode != 0)
                        throw new InvalidOperationException($"ffmpeg exited with code {exitCode}.");
                }
            }
            finally
            {
                try { File.Delete(tempFile); } catch { /* ignore cleanup failures */ }
            }
        }
    }
}
