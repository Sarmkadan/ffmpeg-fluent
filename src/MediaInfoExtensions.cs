using System;

namespace FFmpegFluent
{
    /// <summary>
    /// Extension methods that provide convenient helpers for <see cref="MediaInfo"/>.
    /// </summary>
    public static class MediaInfoExtensions
    {
        /// <summary>
        /// Returns a string representation of the video resolution in the form "WIDTHxHEIGHT".
        /// If either dimension is zero or negative, an empty string is returned.
        /// </summary>
        /// <param name="info">The media information to process.</param>
        /// <returns>A string in the format "WIDTHxHEIGHT", or empty string if dimensions are invalid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
        public static string GetResolution(this MediaInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            return (info.Width > 0 && info.Height > 0)
                ? $"{info.Width}x{info.Height}"
                : string.Empty;
        }

        /// <summary>
        /// Determines whether the media contains only video (no audio stream).
        /// </summary>
        /// <param name="info">The media information to process.</param>
        /// <returns><see langword="true"/> if the media contains video but no audio; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
        public static bool IsVideoOnly(this MediaInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            return !string.IsNullOrWhiteSpace(info.VideoCodec) &&
                string.IsNullOrWhiteSpace(info.AudioCodec);
        }

        /// <summary>
        /// Calculates the average bitrate per second (bits per second) based on the total bitrate
        /// and the duration. Returns <c>0</c> if the duration is zero or negative.
        /// </summary>
        /// <param name="info">The media information to process.</param>
        /// <returns>The average bitrate in bits per second, or 0 if duration is zero or negative.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
        public static double AverageBitRatePerSecond(this MediaInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            var totalSeconds = info.Duration.TotalSeconds;
            return totalSeconds > 0 ? info.BitRate / totalSeconds : 0;
        }

        /// <summary>
        /// Produces a concise, human‑readable summary of the media information.
        /// </summary>
        /// <param name="info">The media information to process.</param>
        /// <returns>A formatted string containing key media properties.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
        public static string ToSummaryString(this MediaInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            return $"Format: {info.FormatName ?? "unknown"}, " +
                $"Duration: {info.Duration}, " +
                $"Resolution: {info.GetResolution()}, " +
                $"Video: {info.VideoCodec ?? "none"}, " +
                $"Audio: {info.AudioCodec ?? "none"}, " +
                $"Bitrate: {info.BitRate} bps, " +
                $"FPS: {info.FrameRate:F2}";
        }

        /// <summary>
        /// Determines whether the media contains an audio stream.
        /// </summary>
        /// <param name="info">The media information to process.</param>
        /// <returns><see langword="true"/> if the media contains an audio stream; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
        public static bool HasAudio(this MediaInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            return !string.IsNullOrWhiteSpace(info.AudioCodec);
        }

        /// <summary>
        /// Determines whether the media contains a video stream.
        /// </summary>
        /// <param name="info">The media information to process.</param>
        /// <returns><see langword="true"/> if the media contains a video stream; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
        public static bool HasVideo(this MediaInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            return !string.IsNullOrWhiteSpace(info.VideoCodec);
        }

        /// <summary>
        /// Returns the resolution of the media as a <see cref="System.Drawing.Size"/>-style tuple.
        /// </summary>
        /// <param name="info">The media information to process.</param>
        /// <returns>A tuple containing the width and height of the video.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
        public static (int Width, int Height) Resolution(this MediaInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            return (info.Width, info.Height);
        }

        /// <summary>
        /// Calculates the total bitrate of the media in megabits per second.
        /// </summary>
        /// <param name="info">The media information to process.</param>
        /// <returns>The bitrate in megabits per second.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
        public static double BitrateMbps(this MediaInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            return info.BitRate / 1_000_000.0;
        }
    }
}
