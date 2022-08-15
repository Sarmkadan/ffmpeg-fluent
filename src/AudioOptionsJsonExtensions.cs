using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent
{
    public static class AudioOptionsJsonExtensions
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static string ToJson(this AudioOptions value, bool indented = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var options = indented ? jsonSerializerOptions : new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return JsonSerializer.Serialize(value, options);
        }

        public static AudioOptions? FromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<AudioOptions>(json, jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool TryFromJson(string json, out AudioOptions? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<AudioOptions>(json, jsonSerializerOptions);
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
