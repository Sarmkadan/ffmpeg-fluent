using System;
using System.Text.Json;

namespace FFmpegFluent
{
    /// <summary>
    /// JSON serialization helpers for <see cref="ThumbnailPreset"/>.
    /// </summary>
    public static class ThumbnailPresetJsonExtensions
    {
        // Cached options with camel-case naming. WriteIndented is set per call.
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Preserve defaults for other settings (e.g., ignore null values) as needed.
        };

        /// <summary>
        /// Serializes the <paramref name="value"/> to JSON.
        /// </summary>
        /// <param name="value">The <see cref="ThumbnailPreset"/> instance to serialize.</param>
        /// <param name="indented">If <c>true</c>, the output JSON will be indented.</param>
        /// <returns>A JSON string representing the object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this ThumbnailPreset value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            // Create a copy of the cached options with the requested indentation setting.
            var options = new JsonSerializerOptions(_options) { WriteIndented = indented };

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="ThumbnailPreset"/> instance.
        /// </summary>
        /// <param name="json">The JSON representation of a <see cref="ThumbnailPreset"/>.</param>
        /// <returns>The deserialized <see cref="ThumbnailPreset"/>, or <c>null</c> if the JSON is <c>null</c> or empty.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
        public static ThumbnailPreset? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<ThumbnailPreset>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="ThumbnailPreset"/> instance.
        /// </summary>
        /// <param name="json">The JSON representation of a <see cref="ThumbnailPreset"/>.</param>
        /// <param name="value">When this method returns, contains the deserialized value if the operation succeeded; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if deserialization succeeds; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
        public static bool TryFromJson(string json, out ThumbnailPreset? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<ThumbnailPreset>(json, _options);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
