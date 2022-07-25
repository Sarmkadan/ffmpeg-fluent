using System.Collections.Generic;

namespace FFmpegFluent
{
    /// <summary>
    /// Extension methods that provide convenient shortcuts for configuring <see cref="VideoOptions"/>.
    /// </summary>
    public static class VideoOptionsExtensions
    {
        /// <summary>
        /// Sets both the video codec and bitrate in a single call.
        /// </summary>
        /// <param name="options">The <see cref="VideoOptions"/> instance to configure.</param>
        /// <param name="codec">The codec name (e.g., "libx264").</param>
        /// <param name="bitrate">The bitrate string (e.g., "2000k").</param>
        /// <returns>The same <see cref="VideoOptions"/> instance for further chaining.</returns>
        public static VideoOptions WithCodecAndBitrate(this VideoOptions options, string codec, string bitrate)
        {
            return options.Codec(codec).Bitrate(bitrate);
        }

        /// <summary>
        /// Configures the video resolution and frame rate together.
        /// </summary>
        /// <param name="options">The <see cref="VideoOptions"/> instance to configure.</param>
        /// <param name="width">Target width in pixels.</param>
        /// <param name="height">Target height in pixels.</param>
        /// <param name="fps">Desired frame rate.</param>
        /// <returns>The same <see cref="VideoOptions"/> instance for further chaining.</returns>
        public static VideoOptions WithResolutionAndFrameRate(this VideoOptions options, int width, int height, double fps)
        {
            return options.Resolution(width, height).FrameRate(fps);
        }

        /// <summary>
        /// Applies a high‑quality preset by setting a CRF value and an encoder preset.
        /// </summary>
        /// <param name="options">The <see cref="VideoOptions"/> instance to configure.</param>
        /// <param name="crf">Constant Rate Factor (lower is higher quality). Default is 18.</param>
        /// <param name="preset">Encoder preset name (e.g., "slow", "medium"). Default is "slow".</param>
        /// <returns>The same <see cref="VideoOptions"/> instance for further chaining.</returns>
        public static VideoOptions WithHighQuality(this VideoOptions options, int crf = 18, string preset = "slow")
        {
            return options.Crf(crf).Preset(preset);
        }

        /// <summary>
        /// Disables video output entirely.
        /// </summary>
        /// <param name="options">The <see cref="VideoOptions"/> instance to configure.</param>
        /// <returns>The same <see cref="VideoOptions"/> instance for further chaining.</returns>
        public static VideoOptions DisableVideo(this VideoOptions options)
        {
            return options.NoVideo();
        }
    }
}
