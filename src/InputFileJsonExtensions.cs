using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent;

public static class InputFileJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string ToJson(this InputFile value, bool indented = false)
    {
        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    public static InputFile? FromJson(string json)
    {
        return JsonSerializer.Deserialize<InputFile>(json, _jsonOptions);
    }

    public static bool TryFromJson(string json, out InputFile? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<InputFile>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
