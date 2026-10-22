#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace FFmpegFluent;

/// <summary>
/// Non-throwing validation helpers for <see cref="AudioOptions"/>.
/// </summary>
public static class AudioOptionsValidation
{
    // Sample rates from the MPEG-4 AAC sampling-frequency table (includes 7350 Hz, which no other encoder here accepts).
    private static readonly int[] _aacSampleRates = [7350, 8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100, 48000, 64000, 88200, 96000];

    // MPEG-1/2/2.5 Layer III sample rates.
    private static readonly int[] _mp3SampleRates = [8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100, 48000];

    // Rates libopus accepts directly; other rates are rejected by the encoder.
    private static readonly int[] _opusSampleRates = [8000, 12000, 16000, 24000, 48000];

    // Used for encoders without a codec-specific subset (FLAC, PCM, ...) and when no codec is set.
    private static readonly int[] _standardSampleRates = [8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100, 48000, 64000, 88200, 96000, 176400, 192000];

    private const int MinChannels = 1;
    private const int MaxChannels = 8;

    private static readonly Dictionary<string, CodecLimits> _codecLimits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["aac"] = new(_aacSampleRates, 32_000, 512_000),
        ["libfdk_aac"] = new(_aacSampleRates, 32_000, 512_000),
        ["libmp3lame"] = new(_mp3SampleRates, 32_000, 320_000),
        ["libopus"] = new(_opusSampleRates),
        ["opus"] = new(_opusSampleRates),
    };

    // Encoders known to ffmpeg that are not in _codecLimits. Names with pcm_/adpcm_ prefixes are accepted by pattern.
    private static readonly HashSet<string> _knownEncoders = new(StringComparer.OrdinalIgnoreCase)
    {
        "aac", "libfdk_aac", "libmp3lame", "libopus", "opus", "libvorbis", "vorbis", "flac", "alac",
        "ac3", "eac3", "mp2", "mp2fixed", "wmav1", "wmav2", "libshine", "libtwolame", "wavpack",
        "copy",
    };

    /// <summary>
    /// Validates the <see cref="AudioOptions"/> instance and returns every problem found.
    /// Settings that are not emitted (when <see cref="AudioOptions.NoAudio"/> is set) are not checked.
    /// </summary>
    /// <param name="value">The audio options to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this AudioOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value._noAudio)
        {
            return problems;
        }

        if (value._codec is not null && !IsKnownEncoder(value._codec))
        {
            problems.Add($"Unknown audio encoder '{value._codec}'. Use a known encoder name such as 'aac', 'libmp3lame', 'libopus', 'flac' or 'copy'.");
        }

        var limits = value._codec is not null && _codecLimits.TryGetValue(value._codec, out var found) ? found : null;

        if (value._sampleRate is int sampleRate)
        {
            var allowedRates = limits?.SampleRates ?? _standardSampleRates;
            if (Array.IndexOf(allowedRates, sampleRate) < 0)
            {
                var codecNote = value._codec is null ? string.Empty : $" by {value._codec}";
                problems.Add(
                    $"Sample rate {sampleRate} Hz is not supported{codecNote}. Use one of: {string.Join(", ", allowedRates)}.");
            }
        }

        if (value._channels is int channels && (channels < MinChannels || channels > MaxChannels))
        {
            problems.Add($"Channel count must be between {MinChannels} and {MaxChannels}, got {channels}.");
        }

        if (value._volume is double volume && (!double.IsFinite(volume) || volume < 0))
        {
            problems.Add($"Volume multiplier must be a finite non-negative value, got {volume.ToString(CultureInfo.InvariantCulture)}.");
        }

        if (!string.IsNullOrEmpty(value._bitrate))
        {
            problems.AddRange(ValidateBitrate(value._bitrate, value._codec, limits));
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified <see cref="AudioOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The audio options to check.</param>
    /// <returns><see langword="true"/> if the instance has no validation problems; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this AudioOptions value) => Validate(value).Count == 0;

    /// <summary>
    /// Throws if the specified <see cref="AudioOptions"/> instance has any validation problems.
    /// </summary>
    /// <param name="value">The audio options to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="FFmpegOptionsValidationException">Thrown with every problem found when the instance is invalid.</exception>
    public static void EnsureValid(this AudioOptions value)
    {
        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new FFmpegOptionsValidationException(problems);
        }
    }

    private static bool IsKnownEncoder(string codec) =>
        _knownEncoders.Contains(codec)
        || _codecLimits.ContainsKey(codec)
        || codec.StartsWith("pcm_", StringComparison.OrdinalIgnoreCase)
        || codec.StartsWith("adpcm_", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> ValidateBitrate(string bitrate, string? codec, CodecLimits? limits)
    {
        if (!BitrateFormat.TryParse(bitrate, out var bitsPerSecond))
        {
            yield return $"Bitrate '{bitrate}' is not in FFmpeg format: a number with an optional k, M or G suffix (for example '128k' or '1M').";
            yield break;
        }

        if (bitsPerSecond <= 0)
        {
            yield return $"Bitrate '{bitrate}' must be greater than zero.";
            yield break;
        }

        if (limits?.MinBitrate is long min && limits.MaxBitrate is long max && (bitsPerSecond < min || bitsPerSecond > max))
        {
            yield return
                $"Bitrate '{bitrate}' ({bitsPerSecond.ToString("F0", CultureInfo.InvariantCulture)} bit/s) is outside the range " +
                $"{min}-{max} bit/s for {codec}.";
        }
    }

    private sealed record CodecLimits(int[] SampleRates, long? MinBitrate = null, long? MaxBitrate = null);
}
