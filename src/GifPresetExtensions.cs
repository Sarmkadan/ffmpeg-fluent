using System;
using System.Threading.Tasks;

namespace FFmpegFluent
{
    /// <summary>
    /// Provides extension methods for configuring <see cref="GifPreset"/> instances.
    /// </summary>
    public static class GifPresetExtensions
    {
        /// <summary>
        /// Configures the preset with default settings suitable for most use cases.
        /// </summary>
        /// <param name="preset">The preset to configure.</param>
        /// <returns>The configured preset.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="preset"/> is <see langword="null"/>.</exception>
        public static GifPreset WithDefaultSettings(this GifPreset preset)
        {
            ArgumentNullException.ThrowIfNull(preset);

            return preset
                .WithFps(30)
                .WithWidth(640);
        }

        /// <summary>
        /// Runs the FFmpeg command and outputs the generated arguments to the console.
        /// </summary>
        /// <param name="preset">The preset to configure.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="preset"/> is <see langword="null"/>.</exception>
        public static async Task RunAndBuildArgumentsAsync(this GifPreset preset)
        {
            ArgumentNullException.ThrowIfNull(preset);

            await preset.RunAsync().ConfigureAwait(false);
            var arguments = preset.BuildArguments();
            Console.WriteLine("Arguments: " + string.Join(" ", arguments));
        }

        /// <summary>
        /// Applies the specified time range multiple times to create a looping effect.
        /// </summary>
        /// <param name="preset">The preset to configure.</param>
        /// <param name="start">The start time of each range.</param>
        /// <param name="duration">The duration of each range.</param>
        /// <param name="loopCount">The number of times to apply the range.</param>
        /// <returns>The configured preset with looping ranges applied.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="preset"/> is <see langword="null"/>.</exception>
        public static GifPreset WithLoopingRange(this GifPreset preset, TimeSpan start, TimeSpan duration, int loopCount)
        {
            ArgumentNullException.ThrowIfNull(preset);

            if (loopCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(loopCount), "Loop count must be positive");
            }

            for (int i = 0; i < loopCount; i++)
            {
                preset = preset.WithRange(start, duration);
            }

            return preset;
        }

        /// <summary>
        /// Configures the preset with standard definition settings (720p, 25 FPS).
        /// </summary>
        /// <param name="preset">The preset to configure.</param>
        /// <returns>The configured preset.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="preset"/> is <see langword="null"/>.</exception>
        public static GifPreset WithStandardDefinition(this GifPreset preset)
        {
            ArgumentNullException.ThrowIfNull(preset);

            return preset
                .WithFps(25)
                .WithWidth(720);
        }
    }
}