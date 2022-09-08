using System;

namespace FFmpegFluent
{
    /// <summary>
    /// Provides extension methods for building FFmpeg filter graphs with common video/audio processing operations.
    /// Each method returns the modified <see cref="FilterGraph"/> instance to allow fluent chaining.
    /// </summary>
    public static class FilterGraphExtensions
    {
        /// <summary>
        /// Adds an overlay filter to overlay one video stream onto another at the top-left corner (x=0, y=0).
        /// </summary>
        /// <param name="graph">The filter graph to extend.</param>
        /// <param name="overlayFilter">The overlay filter specification (e.g., "[in1][in2]overlay=0:0").</param>
        /// <returns>The filter graph with the overlay filter added.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="overlayFilter"/> is <see langword="null"/>.</exception>
        public static FilterGraph Overlay(this FilterGraph graph, string overlayFilter)
        {
            ArgumentNullException.ThrowIfNull(graph);
            ArgumentNullException.ThrowIfNull(overlayFilter);

            return graph.AddFilter("overlay", ("x", "0"), ("y", "0"), ("overlay", overlayFilter));
        }

        /// <summary>
        /// Adds a scale filter to resize video frames to the specified dimensions.
        /// </summary>
        /// <param name="graph">The filter graph to extend.</param>
        /// <param name="scaleFilter">The scale specification (e.g., "1280:720" or "-1:480").</param>
        /// <returns>The filter graph with the scale filter added.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="scaleFilter"/> is <see langword="null"/>.</exception>
        public static FilterGraph Scale(this FilterGraph graph, string scaleFilter)
        {
            ArgumentNullException.ThrowIfNull(graph);
            ArgumentNullException.ThrowIfNull(scaleFilter);

            return graph.AddFilter("scale", ("size", scaleFilter));
        }

        /// <summary>
        /// Adds a fade filter to apply a fade-in effect to video frames over the specified duration.
        /// </summary>
        /// <param name="graph">The filter graph to extend.</param>
        /// <param name="fadeFilter">The fade duration specification (e.g., "t=3" for 3 seconds).</param>
        /// <returns>The filter graph with the fade filter added.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="fadeFilter"/> is <see langword="null"/>.</exception>
        public static FilterGraph Fade(this FilterGraph graph, string fadeFilter)
        {
            ArgumentNullException.ThrowIfNull(graph);
            ArgumentNullException.ThrowIfNull(fadeFilter);

            return graph.AddFilter("fade", ("t", fadeFilter), ("type", "in"));
        }

        /// <summary>
        /// Adds a crop filter to crop video frames to the specified width and height.
        /// The same value is used for both width and height.
        /// </summary>
        /// <param name="graph">The filter graph to extend.</param>
        /// <param name="cropFilter">The crop specification (e.g., "w=640:h=480" or "iw-10:ih-20").</param>
        /// <returns>The filter graph with the crop filter added.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="cropFilter"/> is <see langword="null"/>.</exception>
        public static FilterGraph Crop(this FilterGraph graph, string cropFilter)
        {
            ArgumentNullException.ThrowIfNull(graph);
            ArgumentNullException.ThrowIfNull(cropFilter);

            return graph.AddFilter("crop", ("w", cropFilter), ("h", cropFilter));
        }
    }
}
