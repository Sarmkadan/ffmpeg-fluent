using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent
{
    /// <summary>
    /// JSON serialization helpers for <see cref="FFmpegException"/>.
    /// </summary>
    public static class FFmpegExceptionJsonExtensions
    {
        // Cached serializer options with camelCase naming policy.
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Preserve case for enum values if any; can be adjusted as needed.
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Serializes the <see cref="FFmpegException"/> to a JSON string.
        /// </summary>
        /// <param name="value">The exception instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON representation of the exception.</returns>
        public static string ToJson(this FFmpegException value, bool indented = false)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var options = indented
                ? new JsonSerializerOptions(_options) { WriteIndented = true }
                : _options;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into an <see cref="FFmpegException"/> instance.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>The deserialized exception, or <c>null</c> if the JSON is <c>null</c> or empty.</returns>
        public static FFmpegException? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<FFmpegException>(json, _options);
        }

        /// <summary>
        /// Tries to deserialize a JSON string into an <see cref="FFmpegException"/> instance.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="value">When this method returns, contains the deserialized exception if the operation succeeded; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryFromJson(string json, out FFmpegException? value)
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
