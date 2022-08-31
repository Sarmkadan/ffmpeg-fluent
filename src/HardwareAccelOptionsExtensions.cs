#nullable enable

namespace FFmpegFluent
{
    /// <summary>
    /// Extension methods for <see cref="HardwareAccelOptions"/> providing convenient operations
    /// for hardware acceleration configuration and validation.
    /// </summary>
    public static class HardwareAccelOptionsExtensions
    {
        /// <summary>
        /// Determines whether the hardware acceleration is configured and enabled.
        /// </summary>
        /// <param name="options">The hardware acceleration options.</param>
        /// <returns>True if hardware acceleration is enabled; otherwise, false.</returns>
        public static bool IsEnabled(this HardwareAccelOptions? options)
        {
            return options?.Kind != HwAccelKind.None;
        }

        /// <summary>
        /// Creates a copy of the hardware acceleration options with the specified device.
        /// </summary>
        /// <param name="options">The original hardware acceleration options.</param>
        /// <param name="device">The device identifier to use.</param>
        /// <returns>A new <see cref="HardwareAccelOptions"/> instance with the updated device.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
        public static HardwareAccelOptions WithDevice(this HardwareAccelOptions options, string device)
        {
            ArgumentNullException.ThrowIfNull(options);
            return new HardwareAccelOptions
            {
                Kind = options.Kind,
                Device = device
            };
        }

        /// <summary>
        /// Determines whether the specified hardware acceleration kind is supported.
        /// </summary>
        /// <param name="options">The hardware acceleration options.</param>
        /// <param name="kind">The hardware acceleration kind to check.</param>
        /// <returns>True if the kind is supported; otherwise, false.</returns>
        public static bool Supports(this HardwareAccelOptions? options, HwAccelKind kind)
        {
            return options?.Kind == kind;
        }

        /// <summary>
        /// Gets the device path for the hardware acceleration, or a default value if not specified.
        /// </summary>
        /// <param name="options">The hardware acceleration options.</param>
        /// <param name="defaultDevice">The default device path to return if none is specified. Defaults to "/dev/dri/renderD128".</param>
        /// <returns>The device path, or the default if not specified.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="defaultDevice"/> is <see langword="null"/>.</exception>
        public static string GetDevicePath(this HardwareAccelOptions? options, string defaultDevice = "/dev/dri/renderD128")
        {
            ArgumentNullException.ThrowIfNull(defaultDevice);
            return options?.Device ?? defaultDevice;
        }
    }
}