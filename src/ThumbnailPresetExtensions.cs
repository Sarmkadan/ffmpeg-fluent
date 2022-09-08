using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent
{
    /// <summary>
    /// Extension methods that add convenient shortcuts for working with <see cref="ThumbnailPreset"/>.
    /// </summary>
    public static class ThumbnailPresetExtensions
    {
        /// <summary>
        /// Positions the thumbnail capture at the midpoint of the supplied video duration.
        /// </summary>
        /// <param name="preset">The preset to configure.</param>
        /// <param name="totalDuration">The total duration of the source video.</param>
        /// <returns>The same <see cref="ThumbnailPreset"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        public static ThumbnailPreset AtMidpoint(this ThumbnailPreset preset, TimeSpan totalDuration)
        {
            ArgumentNullException.ThrowIfNull(preset);

            // Calculate half of the total duration.
            var midpoint = TimeSpan.FromTicks(totalDuration.Ticks / 2);
            return preset.AtTime(midpoint);
        }

        /// <summary>
        /// Sets a square size for the thumbnail (width == height).
        /// </summary>
        /// <param name="preset">The preset to configure.</param>
        /// <param name="size">The desired width and height in pixels.</param>
        /// <returns>The same <see cref="ThumbnailPreset"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is zero or negative.</exception>
        public static ThumbnailPreset WithSquareSize(this ThumbnailPreset preset, int size)
        {
            ArgumentNullException.ThrowIfNull(preset);
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            return preset.WithSize(size, size);
        }

        /// <summary>
        /// Conditionally enables smart‑select mode.
        /// </summary>
        /// <param name="preset">The preset to configure.</param>
        /// <param name="condition">If <c>true</c>, <see cref="ThumbnailPreset.UseSmartSelect"/> is applied.</param>
        /// <returns>The same <see cref="ThumbnailPreset"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        public static ThumbnailPreset UseSmartSelectIf(this ThumbnailPreset preset, bool condition)
        {
            ArgumentNullException.ThrowIfNull(preset);
            return condition ? preset.UseSmartSelect() : preset;
        }

        /// <summary>
        /// Executes the preset and returns a task that completes when the ffmpeg process finishes.
        /// This is a thin wrapper around <see cref="ThumbnailPreset.RunAsync"/> that makes the call
        /// slightly more expressive when used in fluent pipelines.
        /// </summary>
        /// <param name="preset">The preset to run.</param>
        /// <param name="ffmpegPath">Path to the ffmpeg executable (defaults to "ffmpeg").</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="preset"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="ffmpegPath"/> is null or whitespace.</exception>
        public static async Task RunAndWaitAsync(this ThumbnailPreset preset, string ffmpegPath = "ffmpeg", CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(preset);
            if (string.IsNullOrWhiteSpace(ffmpegPath))
            {
                throw new ArgumentException("ffmpeg path must not be empty.", nameof(ffmpegPath));
            }

            await preset.RunAsync(ffmpegPath, cancellationToken).ConfigureAwait(false);
        }
    }
}
