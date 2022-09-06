namespace FFmpegFluent;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides JSON serialization and deserialization extension methods for <see cref="ExtractAudioPreset"/> objects.
/// </summary>
public static class ExtractAudioPresetJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an <see cref="ExtractAudioPreset"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ExtractAudioPreset"/> instance to serialize.</param>
    /// <param name="indented">If true, the JSON output will be formatted with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="ExtractAudioPreset"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ExtractAudioPreset value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="ExtractAudioPreset"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="ExtractAudioPreset"/> instance, or null if <paramref name="json"/> is null or whitespace.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when deserialization fails, wrapped with a message.</exception>
    public static ExtractAudioPreset? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

        try
        {
            return JsonSerializer.Deserialize<ExtractAudioPreset>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to deserialize ExtractAudioPreset from JSON.", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an <see cref="ExtractAudioPreset"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="ExtractAudioPreset"/> instance if deserialization succeeded, or null if deserialization failed.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ExtractAudioPreset? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ExtractAudioPreset>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
