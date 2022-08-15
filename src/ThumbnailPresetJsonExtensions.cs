using System;
using System.Text.Json;

namespace FFmpegFluent
{
    /// <summary>
    /// JSON serialization helpers for <see cref="ThumbnailPreset"/>.
    /// </summary>
    public static class ThumbnailPresetJsonExtensions
    {
        // Cached options with camel‑case naming. WriteIndented is set per call.
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Preserve defaults for other settings (e.g., ignore null values) as needed.
        };

        /// <summary>
        /// Serialises the <paramref name="value"/> to JSON.
        /// </summary>
        /// <param name="value">The <see cref="ThumbnailPreset"/> instance to serialise.</param>
        /// <param name="indented">If <c>true</c>, the output JSON will be indented.</param>
        /// <returns>A JSON string representing the object.</returns>
        public static string ToJson(this ThumbnailPreset value, bool indented = false)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            // Create a copy of the cached options with the requested indentation setting.
            var options = new JsonSerializerOptions(_options)
            {
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserialises a JSON string into a <see cref="ThumbnailPreset"/> instance.
        /// </summary>
        /// <param name="json">The JSON representation of a <see cref="ThumbnailPreset"/>.</param>
        /// <returns>The deserialised <see cref="ThumbnailPreset"/>, or <c>null</c> if the JSON is <c>null</c> or empty.</returns>
        public static ThumbnailPreset? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<ThumbnailPreset>(json, _options);
        }

        /// <summary>
        /// Tries to deserialise a JSON string into a <see cref="ThumbnailPreset"/> instance.
        /// </summary>
        /// <param name="json">The JSON representation of a <see cref="ThumbnailPreset"/>.</param>
        /// <param name="value">When this method returns, contains the deserialized value if the operation succeeded; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
        public static bool TryFromJson(string json, out ThumbnailPreset? value)
        {
            try
            {
                value = FromJson(json);
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
