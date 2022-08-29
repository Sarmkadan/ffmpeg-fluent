namespace FFmpegFluent;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ConcatPreset"/> objects.
/// </summary>
public static class ConcatPresetJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes a <see cref="ConcatPreset"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The <see cref="ConcatPreset"/> instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the <see cref="ConcatPreset"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this ConcatPreset value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="ConcatPreset"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="ConcatPreset"/> instance, or <see langword="null"/> if the JSON represents a null value.</returns>
	/// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
	/// <exception cref="JsonException">The JSON is invalid or cannot be deserialized to a <see cref="ConcatPreset"/>.</exception>
	public static ConcatPreset? FromJson(string json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			throw new ArgumentException(
				"JSON string cannot be null, empty, or consist only of whitespace.",
				nameof(json));
		}

		try
		{
			return JsonSerializer.Deserialize<ConcatPreset>(json, _jsonOptions);
		}
		catch (JsonException ex)
		{
			throw new JsonException("Failed to deserialize ConcatPreset from JSON. Ensure the JSON format matches the expected schema.", ex);
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="ConcatPreset"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized <see cref="ConcatPreset"/> instance if successful; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
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