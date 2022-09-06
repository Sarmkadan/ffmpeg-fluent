using System;
using System.Text.Json;

namespace FFmpegFluent
{
	/// <summary>
	/// JSON serialization helpers for <see cref="FilterGraph"/>.
	/// </summary>
	public static class FilterGraphJsonExtensions
	{
		// Cached options with camelCase naming policy.
		private static readonly JsonSerializerOptions _options = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true
		};

		/// <summary>
		/// Serializes the <see cref="FilterGraph"/> instance to JSON.
		/// </summary>
		/// <param name="value">The <see cref="FilterGraph"/> to serialize.</param>
		/// <param name="indented">If true, the output JSON will be indented.</param>
		/// <returns>A JSON string representing the <see cref="FilterGraph"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
		public static string ToJson(this FilterGraph value, bool indented = false)
		{
			ArgumentNullException.ThrowIfNull(value);

			// Use a copy of the cached options if indentation is requested.
			var options = indented
				? new JsonSerializerOptions(_options) { WriteIndented = true }
				: _options;

			return JsonSerializer.Serialize(value, options);
		}

		/// <summary>
		/// Deserializes a JSON string into a <see cref="FilterGraph"/> instance.
		/// </summary>
		/// <param name="json">The JSON string.</param>
		/// <returns>The deserialized <see cref="FilterGraph"/>, or null if the JSON represents a null value.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
		public static FilterGraph? FromJson(string json)
		{
			ArgumentNullException.ThrowIfNull(json);

			return JsonSerializer.Deserialize<FilterGraph>(json, _options);
		}

		/// <summary>
		/// Attempts to deserialize a JSON string into a <see cref="FilterGraph"/> instance.
		/// </summary>
		/// <param name="json">The JSON string.</param>
		/// <param name="value">When this method returns, contains the deserialized <see cref="FilterGraph"/> if the operation succeeded; otherwise, null.</param>
		/// <returns>True if deserialization succeeded; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
		public static bool TryFromJson(string json, out FilterGraph? value)
		{
			ArgumentNullException.ThrowIfNull(json);

			try
			{
				value = FromJson(json);
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