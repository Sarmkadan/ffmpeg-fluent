using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent
{
    public static class OutputFileJsonExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public static string ToJson(this OutputFile value, bool indented = false)
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

        public static OutputFile? FromJson(string json)
        {
            return JsonSerializer.Deserialize<OutputFile>(json, JsonOptions);
        }

        public static bool TryFromJson(string json, out OutputFile? value)
        {
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
    }
}
