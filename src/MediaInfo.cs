using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent
{
    /// <summary>
    /// Represents information about a media file.
    /// </summary>
    public sealed class MediaInfo
    {
        /// <summary>
        /// Gets the duration of the media file.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets the bit rate of the media file.
        /// </summary>
        public long BitRate { get; }

        /// <summary>
        /// Gets the video codec of the media file, or null if no video codec is present.
        /// </summary>
        public string? VideoCodec { get; }

        /// <summary>
        /// Gets the audio codec of the media file, or null if no audio codec is present.
        /// </summary>
        public string? AudioCodec { get; }

        /// <summary>
        /// Gets the width of the video in the media file.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the video in the media file.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the frame rate of the video in the media file.
        /// </summary>
        public double FrameRate { get; }

        /// <summary>
        /// Gets the number of audio channels in the media file.
        /// </summary>
        public int AudioChannels { get; }

        /// <summary>
        /// Gets the sample rate of the audio in the media file.
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Gets the format name of the media file.
        /// </summary>
        public string FormatName { get; }

        private MediaInfo(
            TimeSpan duration,
            long bitRate,
            string? videoCodec,
            string? audioCodec,
            int width,
            int height,
            double frameRate,
            int audioChannels,
            int sampleRate,
            string formatName)
        {
            Duration = duration;
            BitRate = bitRate;
            VideoCodec = videoCodec;
            AudioCodec = audioCodec;
            Width = width;
            Height = height;
            FrameRate = frameRate;
            AudioChannels = audioChannels;
            SampleRate = sampleRate;
            FormatName = formatName;
        }

        /// <summary>
        /// Probes the media file at the specified path and returns information about it.
        /// </summary>
        /// <param name="filePath">The path to the media file to probe.</param>
        /// <param name="ffprobePath">The path to the ffprobe executable. Defaults to "ffprobe".</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>A MediaInfo object containing information about the media file.</returns>
        public static async Task<MediaInfo> ProbeAsync(string filePath, string ffprobePath = "ffprobe", CancellationToken ct = default)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v error -print_format json -show_format -show_streams \"{filePath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var timeout = TimeSpan.FromSeconds(10);
            cts.CancelAfter(timeout);

            try
            {
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException("ffprobe failed with non-zero exit code.");
                }

                var json = JsonDocument.Parse(output);
                var root = json.RootElement;

                var format = root.GetProperty("format");
                var bitRate = long.Parse(format.GetProperty("bit_rate").GetString() ?? "0", CultureInfo.InvariantCulture);

                var duration = TimeSpan.FromSeconds(double.Parse(format.GetProperty("duration").GetString() ?? "0", CultureInfo.InvariantCulture));

                string? videoCodec = null;
                string? audioCodec = null;
                int width = 0;
                int height = 0;
                double frameRate = 0.0;
                int audioChannels = 0;
                int sampleRate = 0;
                string formatName = format.GetProperty("format_name").GetString() ?? "unknown";

                foreach (var stream in root.GetProperty("streams").EnumerateArray())
                {
                    var codecType = stream.GetProperty("codec_type").GetString();
                    if (codecType == "video")
                    {
                        videoCodec = stream.GetProperty("codec_name").GetString();
                        width = stream.GetProperty("width").GetInt32();
                        height = stream.GetProperty("height").GetInt32();
                        var rFrameRate = stream.GetProperty("r_frame_rate").GetString();
                        if (rFrameRate != null)
                        {
                            var parts = rFrameRate.Split('/');
                            if (parts.Length == 2 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var numerator) && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var denominator) && denominator != 0)
                            {
                                frameRate = (double)numerator / denominator;
                            }
                        }
                    }
                    else if (codecType == "audio")
                    {
                        audioCodec = stream.GetProperty("codec_name").GetString();
                        audioChannels = stream.GetProperty("channels").GetInt32();
                        sampleRate = stream.GetProperty("sample_rate").GetInt32();
                    }
                }

                return new MediaInfo(
                    duration,
                    bitRate,
                    videoCodec,
                    audioCodec,
                    width,
                    height,
                    frameRate,
                    audioChannels,
                    sampleRate,
                    formatName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to probe media file.", ex);
            }
        }
    }
}
