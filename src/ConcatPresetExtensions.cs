#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FFmpegFluent
{
    /// <summary>
    /// Extension methods for <see cref="ConcatPreset"/> to provide fluent and convenient operations.
    /// </summary>
    public static class ConcatPresetExtensions
    {
        /// <summary>
        /// Adds multiple input files to the preset at once.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <param name="paths">Paths to the input files.</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public static ConcatPreset AddInputs(this ConcatPreset preset, params string[] paths)
        {
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));

            if (paths == null)
                throw new ArgumentNullException(nameof(paths));

            foreach (var path in paths)
            {
                preset.AddInput(path);
            }

            return preset;
        }

        /// <summary>
        /// Adds multiple input files from a directory matching a search pattern.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <param name="directoryPath">Path to the directory containing input files.</param>
        /// <param name="searchPattern">Search pattern (e.g., "*.mp4", "*.mkv").</param>
        /// <param name="searchOption">Whether to include subdirectories.</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public static ConcatPreset AddInputsFromDirectory(
            this ConcatPreset preset,
            string directoryPath,
            string searchPattern = "*.mp4",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));

            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Directory path cannot be null or whitespace.", nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            var files = Directory.GetFiles(directoryPath, searchPattern, searchOption)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (files.Length == 0)
                throw new InvalidOperationException($"No files found matching pattern '{searchPattern}' in directory '{directoryPath}'");

            foreach (var file in files)
            {
                preset.AddInput(file);
            }

            return preset;
        }

        /// <summary>
        /// Configures the preset to re-encode with hardware acceleration using NVENC (NVIDIA).
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <param name="presetName">NVENC preset (e.g., "fast", "medium", "slow").</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public static ConcatPreset WithNvencReencode(
            this ConcatPreset preset,
            string presetName = "medium")
        {
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));

            if (string.IsNullOrWhiteSpace(presetName))
                throw new ArgumentException("Preset name cannot be null or whitespace.", nameof(presetName));

            return preset.WithReencode("h264_nvenc", "aac")
                .WithVideoQualityPreset(presetName);
        }

        /// <summary>
        /// Configures the preset to re-encode with hardware acceleration using QuickSync (Intel).
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <param name="presetName">QSV preset (e.g., "fast", "medium", "slow").</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public static ConcatPreset WithQsvReencode(
            this ConcatPreset preset,
            string presetName = "medium")
        {
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));

            if (string.IsNullOrWhiteSpace(presetName))
                throw new ArgumentException("Preset name cannot be null or whitespace.", nameof(presetName));

            return preset.WithReencode("h264_qsv", "aac")
                .WithVideoQualityPreset(presetName);
        }

        /// <summary>
        /// Sets the video quality preset for re-encoding operations.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <param name="presetName">Quality preset (e.g., "ultrafast", "superfast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow").</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public static ConcatPreset WithVideoQualityPreset(this ConcatPreset preset, string presetName)
        {
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));

            if (string.IsNullOrWhiteSpace(presetName))
                throw new ArgumentException("Preset name cannot be null or whitespace.", nameof(presetName));

            // The actual preset application happens in RunAsync via FFmpeg's -preset flag
            // We store it in a way that RunAsync can access it
            var field = typeof(ConcatPreset).GetField("_videoCodec", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                // Append preset to the codec specification
                var currentCodec = field.GetValue(preset) as string ?? "libx264";
                field.SetValue(preset, $"{currentCodec}:{presetName}");
            }

            return preset;
        }

        /// <summary>
        /// Adds a delay before the first input file, creating a "lead-in" effect.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <param name="delaySeconds">Delay duration in seconds.</param>
        /// <returns>The same <see cref="ConcatPreset"/> instance for fluent chaining.</returns>
        public static ConcatPreset WithLeadInDelay(this ConcatPreset preset, int delaySeconds)
        {
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));

            if (delaySeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(delaySeconds), "Delay must be non-negative.");

            if (delaySeconds == 0)
                return preset;

            // Create a silent audio file with the specified delay
            var silenceFile = Path.GetTempFileName();
            try
            {
                // Generate a silent audio file with the delay
                var ffmpegArgs = $"-f lavfi -i anullsrc=r=44100:cl=stereo -t {delaySeconds} -c:a pcm_s16le \"{silenceFile}\"";
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = System.Diagnostics.Process.Start(startInfo);
                process?.WaitForExit();

                // Insert the silence file at the beginning
                preset.AddInput(silenceFile);
            }
            finally
            {
                try { File.Delete(silenceFile); } catch { }
            }

            return preset;
        }

        /// <summary>
        /// Executes the concatenation and returns the output file path.
        /// </summary>
        /// <param name="preset">The preset instance.</param>
        /// <param name="ffmpegPath">Path to the ffmpeg executable.</param>
        /// <returns>The path to the concatenated output file.</returns>
        public static async Task<string> RunAndGetOutputPathAsync(this ConcatPreset preset, string ffmpegPath = "ffmpeg")
        {
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));

            var outputPathField = typeof(ConcatPreset).GetField("_outputPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (outputPathField == null)
                throw new InvalidOperationException("Could not access output path field.");

            var outputPath = outputPathField.GetValue(preset) as string;
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new InvalidOperationException("Output path is not set.");

            await preset.RunAsync(ffmpegPath).ConfigureAwait(false);
            return outputPath;
        }
    }
}
