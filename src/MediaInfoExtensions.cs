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
        public static string GetResolution(this MediaInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            return (info.Width > 0 && info.Height > 0)
                ? $"{info.Width}x{info.Height}"
                : string.Empty;
        }

        /// <summary>
        /// Determines whether the media contains only video (no audio stream).
        /// </summary>
        public static bool IsVideoOnly(this MediaInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            return !string.IsNullOrWhiteSpace(info.VideoCodec) &&
                   string.IsNullOrWhiteSpace(info.AudioCodec);
        }

        /// <summary>
        /// Calculates the average bitrate per second (bits per second) based on the total bitrate
        /// and the duration. Returns <c>0</c> if the duration is zero or negative.
        /// </summary>
        public static double AverageBitRatePerSecond(this MediaInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            var totalSeconds = info.Duration.TotalSeconds;
            return totalSeconds > 0 ? info.BitRate / totalSeconds : 0;
        }

        /// <summary>
        /// Produces a concise, human‑readable summary of the media information.
        /// </summary>
        public static string ToSummaryString(this MediaInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            return $"Format: {info.FormatName ?? "unknown"}, " +
                   $"Duration: {info.Duration}, " +
                   $"Resolution: {info.GetResolution()}, " +
                   $"Video: {info.VideoCodec ?? "none"}, " +
                   $"Audio: {info.AudioCodec ?? "none"}, " +
                   $"Bitrate: {info.BitRate} bps, " +
                   $"FPS: {info.FrameRate:F2}";
        }
    }
}
