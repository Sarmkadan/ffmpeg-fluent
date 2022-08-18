using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent
{
    public static class FFmpegCommandJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static string ToJson(this FFmpegCommand value, bool indented = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var options = indented ? _jsonSerializerOptions : _jsonSerializerOptions;
            return JsonSerializer.Serialize(value, options);
        }

        public static FFmpegCommand? FromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<FFmpegCommand>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool TryFromJson(string json, out FFmpegCommand? value)
        {
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
