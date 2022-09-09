using System;
using System.Collections.Generic;

namespace FFmpegFluent
{
    /// <summary>
    /// Provides validation methods for <see cref="ThumbnailPreset"/> instances.
    /// </summary>
    public static class ThumbnailPresetValidation
    {
        /// <summary>
        /// Validates the <see cref="ThumbnailPreset"/> by checking if <see cref="ThumbnailPreset.BuildArguments"/> returns valid command-line arguments.
        /// </summary>
        /// <param name="value">The thumbnail preset to validate.</param>
        /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ThumbnailPreset value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate that BuildArguments returns valid arguments
            var buildArgs = value.BuildArguments();

            if (buildArgs == null)
            {
                throw new InvalidOperationException("BuildArguments returned null, which should never happen.");
            }

            if (buildArgs.Length == 0)
            {
                errors.Add("BuildArguments must contain at least one argument");
            }
            else
            {
                foreach (var arg in buildArgs)
                {
                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        errors.Add("BuildArguments cannot contain null, empty, or whitespace strings");
                        break;
                    }
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="ThumbnailPreset"/> is valid by verifying that its <see cref="ThumbnailPreset.BuildArguments"/> contain no errors.
        /// </summary>
        /// <param name="value">The thumbnail preset to check.</param>
        /// <returns><see langword="true"/> if the preset is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this ThumbnailPreset value) => Validate(value).Count == 0;

        /// <summary>
        /// Validates the specified <see cref="ThumbnailPreset"/> and throws an <see cref="ArgumentException"/> if the preset's arguments are invalid.
        /// </summary>
        /// <param name="value">The thumbnail preset to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the preset is invalid.</exception>
        public static void EnsureValid(this ThumbnailPreset value)
        {
            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"ThumbnailPreset is invalid. Problems:\n{string.Join("\n- ", errors)}");
            }
        }
    }
}
