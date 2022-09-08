using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="FFmpegCommand"/> objects.
    /// </summary>
    public static class FFmpegCommandJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes an <see cref="FFmpegCommand"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The FFmpeg command to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the FFmpeg command.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this FFmpegCommand value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an <see cref="FFmpegCommand"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>
        /// The deserialized <see cref="FFmpegCommand"/>, or null if deserialization fails.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        public static FFmpegCommand? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

            try
            {
                return JsonSerializer.Deserialize<FFmpegCommand>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an <see cref="FFmpegCommand"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized <see cref="FFmpegCommand"/> if successful.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out FFmpegCommand? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

            try
            {
                value = JsonSerializer.Deserialize<FFmpegCommand>(json, _jsonSerializerOptions);
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
