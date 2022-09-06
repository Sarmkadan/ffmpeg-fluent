#nullable enable

namespace FFmpegFluent
{
    /// <summary>
    /// Provides validation helpers for <see cref="HardwareAccelOptions"/> instances.
    /// </summary>
    public static class HardwareAccelOptionsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="HardwareAccelOptions"/> instance.
        /// </summary>
        /// <param name="value">The options to validate.</param>
        /// <returns>A list of validation problems (empty if valid).</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this HardwareAccelOptions? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate Kind
            if (value.Kind == HwAccelKind.None)
            {
                problems.Add("HardwareAccelOptions.Kind must be set to a valid hardware acceleration kind (cannot be None).");
            }

            // Validate Device for VAAPI
            if (value.Kind == HwAccelKind.Vaapi && string.IsNullOrWhiteSpace(value.Device))
            {
                problems.Add("HardwareAccelOptions.Device cannot be null or whitespace when Kind is Vaapi.");
            }
            else if (value.Kind == HwAccelKind.Vaapi && !string.IsNullOrWhiteSpace(value.Device))
            {
                // Validate Device path format for VAAPI (basic validation)
                if (!value.Device.StartsWith("/dev/", StringComparison.Ordinal))
                {
                    problems.Add("HardwareAccelOptions.Device should be a valid device path starting with '/dev/' when Kind is Vaapi.");
                }
            }

            return problems;
        }

        /// <summary>
        /// Determines whether the specified <see cref="HardwareAccelOptions"/> instance is valid.
        /// </summary>
        /// <param name="value">The options to check.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        public static bool IsValid(this HardwareAccelOptions? value) => value is not null && Validate(value).Count == 0;

        /// <summary>
        /// Ensures that the specified <see cref="HardwareAccelOptions"/> instance is valid.
        /// </summary>
        /// <param name="value">The options to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
        public static void EnsureValid(this HardwareAccelOptions? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);

            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"HardwareAccelOptions is not valid. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}