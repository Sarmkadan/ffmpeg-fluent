#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FFmpegFluent;

/// <summary>
/// Non-throwing validation helpers for <see cref="VideoOptions"/>.
/// </summary>
public static class VideoOptionsValidation
{
    private static readonly Regex _bitrateFormat = new(
        @"^(?<value>[0-9]+(\.[0-9]+)?)[kKMG]?\z",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="value"/> is a usable scale dimension:
    /// a positive pixel count, or -1/-2 to let FFmpeg derive that side from the aspect ratio.
    /// </summary>
    internal static bool IsValidDimension(int value) => value > 0 || value is -1 or -2;

    /// <summary>
    /// Validates the <see cref="VideoOptions"/> instance and returns every problem found.
    /// Settings that are not emitted (for example when <see cref="VideoOptions.NoVideo"/> is set) are not checked.
    /// </summary>
    /// <param name="value">The video options to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this VideoOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value._noVideo)
        {
            return problems;
        }

        if (value._width is int width && !IsValidDimension(width))
        {
            problems.Add($"Width must be a positive value or -1/-2 for automatic scaling, got {width}.");
        }

        if (value._height is int height && !IsValidDimension(height))
        {
            problems.Add($"Height must be a positive value or -1/-2 for automatic scaling, got {height}.");
        }

        if (value._width is < 0 && value._height is < 0)
        {
            problems.Add("Width and height cannot both be automatic (-1/-2); at least one must be a fixed pixel count.");
        }

        if (value._frameRate is double fps && (!double.IsFinite(fps) || fps <= 0))
        {
            problems.Add($"Frame rate must be a finite positive value, got {fps.ToString(CultureInfo.InvariantCulture)}.");
        }

        if (!string.IsNullOrEmpty(value._bitrate))
        {
            problems.AddRange(ValidateBitrate(value._bitrate));
        }

        if (value._crf is int crf)
        {
            var (min, max) = CrfRangeFor(value._codec);
            if (crf < min || crf > max)
            {
                var codecNote = value._codec is null ? string.Empty : $" for {value._codec}";
                problems.Add($"CRF {crf} is outside the valid range {min}-{max}{codecNote}.");
            }

            if (!string.IsNullOrEmpty(value._bitrate) && !SupportsCrfWithBitrate(value._codec))
            {
                var codecName = value._codec ?? "the default encoder";
                problems.Add(
                    $"CRF and a fixed bitrate are mutually exclusive for {codecName}: " +
                    "use Crf() for constant-quality encoding or Bitrate() for a fixed bitrate, not both.");
            }
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified <see cref="VideoOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The video options to check.</param>
    /// <returns><see langword="true"/> if the instance has no validation problems; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this VideoOptions value) => Validate(value).Count == 0;

    /// <summary>
    /// Throws if the specified <see cref="VideoOptions"/> instance has any validation problems.
    /// </summary>
    /// <param name="value">The video options to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="FFmpegOptionsValidationException">Thrown with every problem found when the instance is invalid.</exception>
    public static void EnsureValid(this VideoOptions value)
    {
        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new FFmpegOptionsValidationException(problems);
        }
    }

    private static IEnumerable<string> ValidateBitrate(string bitrate)
    {
        var match = _bitrateFormat.Match(bitrate);
        if (!match.Success)
        {
            yield return $"Bitrate '{bitrate}' is not in FFmpeg format: a number with an optional k, M or G suffix (for example '500k' or '2M').";
            yield break;
        }

        if (double.Parse(match.Groups["value"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) <= 0)
        {
            yield return $"Bitrate '{bitrate}' must be greater than zero.";
        }
    }

    // Encoder-specific CRF limits. Unknown or unset codecs fall back to the widest range any supported encoder uses.
    private static (int Min, int Max) CrfRangeFor(string? codec)
    {
        if (codec is not null && (codec.Equals("libx264", StringComparison.OrdinalIgnoreCase) || codec.Equals("libx265", StringComparison.OrdinalIgnoreCase)))
        {
            return (0, 51);
        }

        return (0, 63);
    }

    // Constant-quality VP9/AV1 accept -crf together with a target bitrate. For x264/x265 (and an unset codec, which
    // usually resolves to libx264) the two rate-control modes conflict.
    private static bool SupportsCrfWithBitrate(string? codec) =>
        codec is not null && (codec.Equals("libvpx-vp9", StringComparison.OrdinalIgnoreCase) || codec.Equals("libaom-av1", StringComparison.OrdinalIgnoreCase));
}
