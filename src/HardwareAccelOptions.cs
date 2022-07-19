#nullable enable

namespace FFmpegFluent
{
    /// <summary>
    /// Types of hardware acceleration supported by the library.
    /// </summary>
    public enum HwAccelKind
    {
        None,
        Nvenc,
        Qsv,
        Vaapi,
        VideoToolbox
    }

    /// <summary>
    /// Options for hardware acceleration.
    /// </summary>
    public sealed class HardwareAccelOptions
    {
        /// <summary>
        /// The kind of hardware acceleration to use.
        /// </summary>
        public HwAccelKind Kind { get; set; } = HwAccelKind.None;

        /// <summary>
        /// Optional device identifier (used by some accelerators such as VAAPI).
        /// </summary>
        public string? Device { get; set; }

        /// <summary>
        /// Returns the ffmpeg command‑line arguments required to enable the selected hardware acceleration
        /// for input processing.
        /// </summary>
        /// <returns>An array of arguments (e.g. ["-hwaccel", "vaapi", "-hwaccel_device", "/dev/dri/renderD128"]).</returns>
        public string[] GetInputArguments()
        {
            return Kind switch
            {
                HwAccelKind.None => [],
                HwAccelKind.Nvenc => ["-hwaccel", "cuda"],
                HwAccelKind.Qsv => ["-hwaccel", "qsv"],
                HwAccelKind.Vaapi => Device is not null
                    ? ["-hwaccel", "vaapi", "-hwaccel_device", Device]
                    : ["-hwaccel", "vaapi"],
                HwAccelKind.VideoToolbox => ["-hwaccel", "videotoolbox"],
                _ => []
            };
        }

        /// <summary>
        /// Returns the encoder name that corresponds to the selected hardware acceleration.
        /// For example, baseCodec "h264" with Nvenc becomes "h264_nvenc".
        /// </summary>
        /// <param name="baseCodec">The base codec name (e.g., "h264", "hevc").</param>
        /// <returns>The encoder name to be used with ffmpeg.</returns>
        public string GetEncoderName(string baseCodec)
        {
            if (string.IsNullOrWhiteSpace(baseCodec))
                return baseCodec;

            return Kind switch
            {
                HwAccelKind.Nvenc => $"{baseCodec}_nvenc",
                HwAccelKind.Qsv => $"{baseCodec}_qsv",
                HwAccelKind.Vaapi => $"{baseCodec}_vaapi",
                HwAccelKind.VideoToolbox => $"{baseCodec}_videotoolbox",
                _ => baseCodec
            };
        }

        /// <summary>
        /// Creates a <see cref="HardwareAccelOptions"/> configured for Nvidia NVENC.
        /// </summary>
        public static HardwareAccelOptions Nvenc()
        {
            return new HardwareAccelOptions
            {
                Kind = HwAccelKind.Nvenc,
                Device = null
            };
        }

        /// <summary>
        /// Creates a <see cref="HardwareAccelOptions"/> configured for VAAPI.
        /// </summary>
        /// <param name="device">
        /// The VAAPI device path. Defaults to "/dev/dri/renderD128".
        /// </param>
        public static HardwareAccelOptions Vaapi(string device = "/dev/dri/renderD128")
        {
            return new HardwareAccelOptions
            {
                Kind = HwAccelKind.Vaapi,
                Device = device
            };
        }
    }
}
