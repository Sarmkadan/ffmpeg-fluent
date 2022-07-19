#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent
{
    /// <summary>
    /// Preset for extracting a single preview frame (thumbnail) from a video file.
    /// </summary>
    public sealed class ThumbnailPreset
    {
        private readonly string _inputPath;
        private readonly string _outputPath;

        private TimeSpan? _position;
        private (int width, int height)? _size;
        private bool _useSmartSelect;

        /// <summary>
        /// Initializes a new instance of <see cref="ThumbnailPreset"/>.
        /// </summary>
        /// <param name="inputPath">Path to the source video.</param>
        /// <param name="outputPath">Path where the thumbnail image will be saved.</param>
        public ThumbnailPreset(string inputPath, string outputPath)
        {
            _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
            _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
        }

        /// <summary>
        /// Sets the timestamp from which the thumbnail will be taken.
        /// </summary>
        public ThumbnailPreset AtTime(TimeSpan position)
        {
            _position = position;
            return this;
        }

        /// <summary>
        /// Sets the desired size of the thumbnail.
        /// </summary>
        public ThumbnailPreset WithSize(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            _size = (width, height);
            return this;
        }

        /// <summary>
        /// Enables the ffmpeg "thumbnail" filter (smart frame selection).
        /// </summary>
        public ThumbnailPreset UseSmartSelect()
        {
            _useSmartSelect = true;
            return this;
        }

        /// <summary>
        /// Builds the argument list that will be passed to ffmpeg.
        /// </summary>
        public string[] BuildArguments()
        {
            var args = new List<string>
            {
                "-i", _inputPath,
                "-y" // overwrite output without asking
            };

            // Seek position (if any) – placed before -i for fast seeking, but after -i works as well.
            if (_position.HasValue)
            {
                // Using -ss after -i for precise seeking.
                args.Add("-ss");
                args.Add(_position.Value.ToString(@"hh\:mm\:ss\.fff"));
            }

            // Video filter chain
            var filters = new List<string>();
            if (_useSmartSelect)
            {
                filters.Add("thumbnail");
            }

            if (_size.HasValue)
            {
                var (w, h) = _size.Value;
                filters.Add($"scale={w}:{h}");
            }

            if (filters.Count > 0)
            {
                args.Add("-vf");
                args.Add(string.Join(",", filters));
            }

            // Output a single frame
            args.Add("-frames:v");
            args.Add("1");
            args.Add(_outputPath);

            return args.ToArray();
        }

        /// <summary>
        /// Executes ffmpeg with the built arguments.
        /// </summary>
        /// <param name="ffmpegPath">Path to the ffmpeg executable. Defaults to "ffmpeg".</param>
        /// <param name="ct">Cancellation token.</param>
        public async Task RunAsync(string ffmpegPath = "ffmpeg", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(ffmpegPath))
                throw new ArgumentException("ffmpeg path must not be empty.", nameof(ffmpegPath));

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Populate ArgumentList (available from .NET Core 2.0+). Fallback to Arguments string if not supported.
#if NET5_0_OR_GREATER
            foreach (var a in BuildArguments())
                startInfo.ArgumentList.Add(a);
#else
            startInfo.Arguments = string.Join(' ', BuildArguments());
#endif

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // Capture output to avoid deadlocks (optional, but safe)
            _ = process.StandardOutput.ReadToEndAsync(ct);
            _ = process.StandardError.ReadToEndAsync(ct);

            await process.WaitForExitAsync(ct).ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"ffmpeg exited with code {process.ExitCode}.");
            }
        }
    }
}
