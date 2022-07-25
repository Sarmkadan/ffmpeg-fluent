#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="VideoOptions"/>.
/// </summary>
public static class VideoOptionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="VideoOptions"/> to a JSON string.
    /// </summary>
    /// <param name="value">The video options to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the video options.</returns>
    public static string ToJson(this VideoOptions value, bool indented = false)
    {
        if (value is null)
        {
            return "{}";
        }

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="VideoOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="VideoOptions"/> instance, or null if deserialization fails.</returns>
    public static VideoOptions? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<VideoOptions>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="VideoOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized value, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out VideoOptions? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<VideoOptions>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}