using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent
{
    public static class WatermarkHelperJsonExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string ToJson(this WatermarkHelper value, bool indented = false)
        {
            if (indented)
            {
                JsonOptions.WriteIndented = true;
            }
            else
            {
                JsonOptions.WriteIndented = false;
            }
            return JsonSerializer.Serialize(value, JsonOptions);
        }

        public static WatermarkHelper? FromJson(string json)
        {
            return JsonSerializer.Deserialize<WatermarkHelper>(json, JsonOptions);
        }

        public static bool TryFromJson(string json, out WatermarkHelper? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<WatermarkHelper>(json, JsonOptions);
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
