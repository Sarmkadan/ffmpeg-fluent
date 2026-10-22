#nullable enable

using System.Globalization;
using System.Text.RegularExpressions;

namespace FFmpegFluent;

/// <summary>
/// Parses bitrate strings in the format FFmpeg accepts for <c>-b:v</c> and <c>-b:a</c>.
/// </summary>
internal static class BitrateFormat
{
    private static readonly Regex _format = new(
        @"^(?<value>[0-9]+(\.[0-9]+)?)(?<suffix>[kKMG]?)\z",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// Parses a bitrate such as <c>192k</c>, <c>2M</c>, <c>1.5M</c> or a plain bits-per-second number.
    /// Ambiguous forms such as <c>2m</c> or <c>2MB</c> do not match.
    /// </summary>
    /// <param name="value">The bitrate string.</param>
    /// <param name="bitsPerSecond">The parsed value in bits per second, or 0 when the format is rejected.</param>
    /// <returns><see langword="true"/> if the string is in FFmpeg bitrate format; otherwise <see langword="false"/>.</returns>
    internal static bool TryParse(string value, out double bitsPerSecond)
    {
        bitsPerSecond = 0;

        var match = _format.Match(value);
        if (!match.Success)
        {
            return false;
        }

        var number = double.Parse(match.Groups["value"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
        var multiplier = match.Groups["suffix"].Value switch
        {
            "k" or "K" => 1_000d,
            "M" => 1_000_000d,
            "G" => 1_000_000_000d,
            _ => 1d
        };

        bitsPerSecond = number * multiplier;
        return true;
    }
}
