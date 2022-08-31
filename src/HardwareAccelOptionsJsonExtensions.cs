#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent
{
    /// <summary>
    /// Provides extension methods for serializing and deserializing <see cref="HardwareAccelOptions"/> instances to and from JSON.
    /// </summary>
    public static class HardwareAccelOptionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the <see cref="HardwareAccelOptions"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The hardware acceleration options to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representing the hardware acceleration options.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this HardwareAccelOptions value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="HardwareAccelOptions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized hardware acceleration options, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static HardwareAccelOptions? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<HardwareAccelOptions>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="HardwareAccelOptions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized hardware acceleration options if successful.</param>
        /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
        public static bool TryFromJson(string json, out HardwareAccelOptions? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return true;
            }

            try
            {
                value = JsonSerializer.Deserialize<HardwareAccelOptions>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}