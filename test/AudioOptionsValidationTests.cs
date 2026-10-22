#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegFluent;

/// <summary>
/// Unit tests for <see cref="AudioOptionsValidation"/> and the pre-run check in <see cref="FFmpegCommand"/>.
/// </summary>
public static class AudioOptionsValidationTests
{
    public static void RunTests()
    {
        DefaultAndTypicalOptionsAreValid();
        SampleRateMustBeStandardForCodec();
        SampleRateUsesStandardListWithoutCodec();
        ChannelsMustBeOneToEight();
        VolumeMustBeFiniteNonNegative();
        BitrateMustMatchFfmpegFormat();
        BitrateMustBePositive();
        BitrateMustFallInCodecRange();
        CodecMustBeKnownEncoder();
        NoAudioSkipsAllChecks();
        AllProblemsAreReportedAtOnce();
        EnsureValidThrowsWithErrors();
        RunAsyncFailsBeforeLaunchingFfmpeg();

        Console.WriteLine("AudioOptionsValidationTests passed.");
    }

    private static void DefaultAndTypicalOptionsAreValid()
    {
        Expect(new AudioOptions().Validate().Count == 0, "Default audio options should be valid.");
        Expect(new AudioOptions().IsValid(), "IsValid should be true for default audio options.");
        Expect(new AudioOptions().Codec("aac").SampleRate(48000).Channels(2).Bitrate("192k").Volume(1.0).Validate().Count == 0,
            "A typical AAC configuration should be valid.");
        Expect(new AudioOptions().Codec("libmp3lame").SampleRate(44100).Channels(2).Bitrate("320k").Validate().Count == 0,
            "A typical MP3 configuration at the top of its bitrate range should be valid.");
        Expect(new AudioOptions().Codec("copy").Validate().Count == 0, "Stream copy should be valid.");
    }

    private static void SampleRateMustBeStandardForCodec()
    {
        Expect(new AudioOptions().Codec("aac").SampleRate(44100).Validate().Count == 0, "44.1 kHz should be valid for AAC.");
        Expect(new AudioOptions().Codec("aac").SampleRate(7350).Validate().Count == 0, "7350 Hz should be valid for AAC.");
        Expect(new AudioOptions().Codec("aac").SampleRate(96000).Validate().Count == 0, "96 kHz should be valid for AAC.");
        AssertSingleProblem(new AudioOptions().Codec("aac").SampleRate(192000).Validate(), "192000", "aac");
        AssertSingleProblem(new AudioOptions().Codec("libmp3lame").SampleRate(96000).Validate(), "96000", "libmp3lame");
        AssertSingleProblem(new AudioOptions().Codec("libmp3lame").SampleRate(7350).Validate(), "7350", "libmp3lame");
        Expect(new AudioOptions().Codec("libopus").SampleRate(48000).Validate().Count == 0, "48 kHz should be valid for Opus.");
        AssertSingleProblem(new AudioOptions().Codec("libopus").SampleRate(44100).Validate(), "44100", "libopus");
        Expect(new AudioOptions().Codec("flac").SampleRate(192000).Validate().Count == 0, "192 kHz should be valid for FLAC.");
    }

    private static void SampleRateUsesStandardListWithoutCodec()
    {
        Expect(new AudioOptions().SampleRate(22050).Validate().Count == 0, "22.05 kHz should be valid with no codec set.");
        Expect(new AudioOptions().SampleRate(8000).Validate().Count == 0, "8000 Hz (lower bound) should be valid with no codec set.");
        Expect(new AudioOptions().SampleRate(192000).Validate().Count == 0, "192000 Hz (upper bound) should be valid with no codec set.");
        AssertSingleProblem(new AudioOptions().SampleRate(12345).Validate(), "12345", null);
        AssertSingleProblem(new AudioOptions().SampleRate(4000).Validate(), "4000", null);
        AssertSingleProblem(new AudioOptions().SampleRate(200000).Validate(), "200000", null);
    }

    private static void ChannelsMustBeOneToEight()
    {
        Expect(new AudioOptions().Channels(1).Validate().Count == 0, "Mono should be valid.");
        Expect(new AudioOptions().Channels(8).Validate().Count == 0, "Eight channels should be valid.");
        AssertSingleProblem(new AudioOptions().Channels(9).Validate(), "between 1 and 8");
        AssertSingleProblem(new AudioOptions().Channels(64).Validate(), "between 1 and 8");
        AssertThrows<ArgumentOutOfRangeException>(() => new AudioOptions().Channels(0), "Channel count 0 must be rejected by the setter.");
    }

    private static void VolumeMustBeFiniteNonNegative()
    {
        Expect(new AudioOptions().Volume(0).Validate().Count == 0, "Zero volume (silence) should be valid.");
        Expect(new AudioOptions().Volume(2.5).Validate().Count == 0, "Volume above 1.0 should be valid (boost).");
        AssertSingleProblem(new AudioOptions().Volume(double.NaN).Validate(), "finite non-negative");
        AssertSingleProblem(new AudioOptions().Volume(double.PositiveInfinity).Validate(), "finite non-negative");
        AssertThrows<ArgumentOutOfRangeException>(() => new AudioOptions().Volume(-0.1), "Negative volume must be rejected by the setter.");
    }

    private static void BitrateMustMatchFfmpegFormat()
    {
        foreach (var valid in new[] { "192k", "192K", "2M", "1.5M", "1G", "128000" })
        {
            Expect(new AudioOptions().Bitrate(valid).Validate().Count == 0, $"Bitrate '{valid}' should be valid.");
        }

        foreach (var invalid in new[] { "fast", "192x", "2m", "2MB", "k", "1e5", "-128k" })
        {
            AssertSingleProblem(new AudioOptions().Bitrate(invalid).Validate(), "FFmpeg format", invalid);
        }
    }

    private static void BitrateMustBePositive()
    {
        AssertSingleProblem(new AudioOptions().Bitrate("0k").Validate(), "greater than zero", "0k");
        AssertSingleProblem(new AudioOptions().Bitrate("0").Validate(), "greater than zero", "0");
    }

    private static void BitrateMustFallInCodecRange()
    {
        // MP3: 32k-320k
        Expect(new AudioOptions().Codec("libmp3lame").Bitrate("32k").Validate().Count == 0, "32k should be valid for MP3.");
        Expect(new AudioOptions().Codec("libmp3lame").Bitrate("320k").Validate().Count == 0, "320k should be valid for MP3.");
        Expect(new AudioOptions().Codec("libmp3lame").Bitrate("32000").Validate().Count == 0, "32000 bit/s should be valid for MP3.");
        AssertSingleProblem(new AudioOptions().Codec("libmp3lame").Bitrate("31k").Validate(), "32000-320000", "31k");
        AssertSingleProblem(new AudioOptions().Codec("libmp3lame").Bitrate("321k").Validate(), "32000-320000", "321k");
        AssertSingleProblem(new AudioOptions().Codec("libmp3lame").Bitrate("512k").Validate(), "32000-320000", "512k");

        // AAC: 32k-512k
        Expect(new AudioOptions().Codec("aac").Bitrate("32k").Validate().Count == 0, "32k should be valid for AAC.");
        Expect(new AudioOptions().Codec("aac").Bitrate("512k").Validate().Count == 0, "512k should be valid for AAC.");
        Expect(new AudioOptions().Codec("aac").Bitrate("512000").Validate().Count == 0, "512000 bit/s should be valid for AAC.");
        AssertSingleProblem(new AudioOptions().Codec("aac").Bitrate("31999").Validate(), "32000-512000", "31999");
        AssertSingleProblem(new AudioOptions().Codec("aac").Bitrate("513k").Validate(), "32000-512000", "513k");
        AssertSingleProblem(new AudioOptions().Codec("aac").Bitrate("2M").Validate(), "32000-512000", "2M");

        // Codecs without a configured range, or no codec at all, only get the format and positivity checks.
        Expect(new AudioOptions().Codec("flac").Bitrate("2M").Validate().Count == 0, "No range is enforced for flac.");
        Expect(new AudioOptions().Bitrate("2M").Validate().Count == 0, "No range is enforced when no codec is set.");
    }

    private static void CodecMustBeKnownEncoder()
    {
        foreach (var known in new[] { "aac", "AAC", "libmp3lame", "libopus", "flac", "pcm_s16le", "adpcm_ima_wav", "copy" })
        {
            Expect(new AudioOptions().Codec(known).Validate().Count == 0, $"Encoder '{known}' should be valid.");
        }

        AssertSingleProblem(new AudioOptions().Codec("notacodec").Validate(), "Unknown audio encoder", "notacodec");
        AssertSingleProblem(new AudioOptions().Codec("libx264").Validate(), "Unknown audio encoder", "libx264");
    }

    private static void NoAudioSkipsAllChecks()
    {
        var options = new AudioOptions().Codec("notacodec").SampleRate(12345).Channels(99).Volume(double.NaN).Bitrate("fast").NoAudio();
        Expect(options.Validate().Count == 0, "NoAudio should skip every audio rule.");
    }

    private static void AllProblemsAreReportedAtOnce()
    {
        var options = new AudioOptions().Codec("notacodec").SampleRate(12345).Channels(9).Volume(double.NaN);

        // Unknown codec, sample rate, channel count, volume.
        var problems = options.Validate();
        Expect(problems.Count == 4, $"Expected 4 problems, got {problems.Count}: {string.Join(" | ", problems)}");

        var mp3 = new AudioOptions().Codec("libmp3lame").Bitrate("999k").SampleRate(7350);

        // Sample rate and bitrate range for MP3.
        problems = mp3.Validate();
        Expect(problems.Count == 2, $"Expected 2 MP3 problems, got {problems.Count}: {string.Join(" | ", problems)}");
    }

    private static void EnsureValidThrowsWithErrors()
    {
        var options = new AudioOptions().Codec("libmp3lame").Bitrate("999k");

        try
        {
            options.EnsureValid();
        }
        catch (FFmpegOptionsValidationException ex)
        {
            Expect(ex.Errors.Count == 1, "Exception should carry exactly the one problem found.");
            Expect(ex.Errors.SequenceEqual(options.Validate()), "Exception Errors should match Validate().");
            return;
        }

        throw new InvalidOperationException("EnsureValid should have thrown FFmpegOptionsValidationException.");
    }

    private static void RunAsyncFailsBeforeLaunchingFfmpeg()
    {
        // The locator points at a binary that does not exist. If validation ran after process start,
        // the failure would be a Win32/IO exception instead of FFmpegOptionsValidationException.
        var command = FFmpegCommand.Create(new DummyLocator())
            .AddInput("input.mp4")
            .AddOutput("good.m4a", o => o.WithAudio(a => a.Codec("aac").Bitrate("192k").SampleRate(48000)))
            .AddOutput("bad.mp3", o => o.WithAudio(a => a.Codec("libmp3lame").Bitrate("999k").Channels(2)))
            .AddOutput("worse.wav", o => o.WithAudio(a => a.Codec("notacodec").Channels(16)));

        try
        {
            command.RunAsync().GetAwaiter().GetResult();
        }
        catch (FFmpegOptionsValidationException ex)
        {
            Expect(ex.Errors.Count == 3, $"Expected 3 problems across the two bad outputs, got {ex.Errors.Count}: {string.Join(" | ", ex.Errors)}");
            Expect(ex.Errors.All(e => !e.StartsWith("Output 'good.m4a':", StringComparison.Ordinal)),
                "The valid output should not be reported.");
            Expect(ex.Errors.Count(e => e.StartsWith("Output 'bad.mp3':", StringComparison.Ordinal)) == 1,
                "The MP3 bitrate problem should name its output.");
            Expect(ex.Errors.Count(e => e.StartsWith("Output 'worse.wav':", StringComparison.Ordinal)) == 2,
                "Both the unknown encoder and channel count should be reported for the third output.");
            return;
        }

        throw new InvalidOperationException("RunAsync should have thrown FFmpegOptionsValidationException.");
    }

    private static void AssertSingleProblem(IReadOnlyList<string> problems, string mustContain, string? context = null)
    {
        var label = context is null ? string.Empty : $" for '{context}'";
        Expect(problems.Count == 1, $"Expected exactly one problem{label}, got {problems.Count}.");
        Expect(problems[0].Contains(mustContain, StringComparison.OrdinalIgnoreCase),
            $"Problem{label} should mention '{mustContain}', got: {problems[0]}");
    }

    private static void AssertThrows<T>(Action action, string message) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException($"{message} Expected {typeof(T).Name}.");
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
