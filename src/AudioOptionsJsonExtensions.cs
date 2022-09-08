using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegFluent
{
	/// <summary>
	/// Provides JSON serialization and deserialization extensions for <see cref="AudioOptions"/>.
	/// Supports formatting control and error handling for JSON operations.
	/// </summary>
	public static class AudioOptionsJsonExtensions
	{
		private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

		/// <summary>
		/// Serializes the specified <see cref="AudioOptions"/> instance to a JSON string.
		/// </summary>
		/// <param name="value">The audio options to serialize.</param>
		/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
		/// <returns>A JSON string representation of the audio options.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
		public static string ToJson(this AudioOptions value, bool indented = false)
		{
			ArgumentNullException.ThrowIfNull(value);

			var options = indented
				? jsonSerializerOptions
				: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
			return JsonSerializer.Serialize(value, options);
		}

		/// <summary>
		/// Deserializes a JSON string to an <see cref="AudioOptions"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize.</param>
		/// <returns>
		/// The deserialized <see cref="AudioOptions"/> instance, or <see langword="null"/>
		/// if deserialization fails due to invalid JSON format.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
		public static AudioOptions? FromJson(string json)
		{
			ArgumentNullException.ThrowIfNull(json);

			try
			{
				return JsonSerializer.Deserialize<AudioOptions>(json, jsonSerializerOptions);
			}
			catch (JsonException)
			{
				return null;
			}
		}

		/// <summary>
		/// Attempts to deserialize a JSON string to an <see cref="AudioOptions"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize.</param>
		/// <param name="value">Receives the deserialized <see cref="AudioOptions"/> instance if successful.</param>
		/// <returns>
		/// <see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
		public static bool TryFromJson(string json, out AudioOptions? value)
		{
			ArgumentNullException.ThrowIfNull(json);

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
