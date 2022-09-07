using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="OutputFile"/> instances.
    /// </summary>
    public static class OutputFileJsonExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes an <see cref="OutputFile"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The output file instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the output file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this OutputFile value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            return JsonSerializer.Serialize(value, GetJsonOptions(indented));
        }

        /// <summary>
        /// Deserializes an <see cref="OutputFile"/> instance from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="OutputFile"/> instance, or null if the JSON is invalid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static OutputFile? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            return JsonSerializer.Deserialize<OutputFile>(json, JsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize an <see cref="OutputFile"/> instance from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized value if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out OutputFile? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<OutputFile>(json, JsonOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }

        private static JsonSerializerOptions GetJsonOptions(bool indented)
        {
            var options = new JsonSerializerOptions(JsonOptions);
            options.WriteIndented = indented;
            return options;
        }
    }
}