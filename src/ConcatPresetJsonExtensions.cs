namespace FFmpegFluent;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class ConcatPresetJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string ToJson(this ConcatPreset value, bool indented = false)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    public static ConcatPreset? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON string cannot be null or whitespace.", nameof(json));
        }

        try
        {
            return JsonSerializer.Deserialize<ConcatPreset>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to deserialize ConcatPreset from JSON.", ex);
        }
    }

    public static bool TryFromJson(string json, out ConcatPreset? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ConcatPreset>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}