using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="WatermarkHelper"/> instances.
    /// </summary>
    public static class WatermarkHelperJsonExtensions
    {
        /// <summary>
        /// Gets the default JSON serializer options used for serializing and deserializing <see cref="WatermarkHelper"/> instances.
        /// Uses camelCase property naming policy and ignores null values.
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the specified <see cref="WatermarkHelper"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The watermark helper to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the watermark helper.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this WatermarkHelper value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="WatermarkHelper"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>
        /// The deserialized <see cref="WatermarkHelper"/> instance, or <see langword="null"/>
        /// if deserialization fails due to invalid JSON format.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
        public static WatermarkHelper? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            return JsonSerializer.Deserialize<WatermarkHelper>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="WatermarkHelper"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized <see cref="WatermarkHelper"/> instance if successful.</param>
        /// <returns>
        /// <see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
        public static bool TryFromJson(string json, out WatermarkHelper? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<WatermarkHelper>(json, _jsonOptions);
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
