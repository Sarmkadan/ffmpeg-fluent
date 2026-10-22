#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegFluent;

/// <summary>
/// Unit tests for <see cref="VideoOptionsValidation"/> and the pre-run check in <see cref="FFmpegCommand"/>.
/// </summary>
public static class VideoOptionsValidationTests
{
    public static void RunTests()
    {
        DefaultOptionsAreValid();
        DimensionsAcceptPositiveOrAutoValues();
        DimensionsRejectBothAutomatic();
        FrameRateMustBeFinitePositive();
        BitrateMustMatchFfmpegFormat();
        BitrateMustBePositive();
        CrfMustFitCodecRange();
        CrfAndBitrateAreMutuallyExclusiveForX26x();
        CrfAndBitrateAllowedForConstantQualityVp9();
        AllProblemsAreReportedAtOnce();
        EnsureValidThrowsWithErrors();
        RunAsyncFailsBeforeLaunchingFfmpeg();

        Console.WriteLine("VideoOptionsValidationTests passed.");
    }

    private static void DefaultOptionsAreValid()
    {
        Expect(new VideoOptions().Validate().Count == 0, "Default options should be valid.");
        Expect(new VideoOptions().NoVideo().Validate().Count == 0, "NoVideo options should be valid.");
        Expect(new VideoOptions().Codec("libx264").Bitrate("2M").FrameRate(30).Resolution(1920, 1080).Preset("fast").Validate().Count == 0,
            "A typical fixed-bitrate configuration should be valid.");
    }

    private static void DimensionsAcceptPositiveOrAutoValues()
    {
        Expect(new VideoOptions().Resolution(-1, 720).Validate().Count == 0, "Width -1 (auto) should be valid.");
        Expect(new VideoOptions().Resolution(1280, -2).Validate().Count == 0, "Height -2 (auto) should be valid.");

        AssertThrows<ArgumentOutOfRangeException>(() => new VideoOptions().Resolution(0, 720), "Width 0 must be rejected.");
        AssertThrows<ArgumentOutOfRangeException>(() => new VideoOptions().Resolution(1280, -3), "Height -3 must be rejected.");
    }

    private static void DimensionsRejectBothAutomatic()
    {
        AssertSingleProblem(new VideoOptions().Resolution(-1, -1).Validate(), "both be automatic");
        AssertSingleProblem(new VideoOptions().Resolution(-2, -1).Validate(), "both be automatic");
    }

    private static void FrameRateMustBeFinitePositive()
    {
        Expect(new VideoOptions().FrameRate(29.97).Validate().Count == 0, "Fractional frame rate should be valid.");
        AssertSingleProblem(new VideoOptions().FrameRate(double.NaN).Validate(), "finite positive");
        AssertSingleProblem(new VideoOptions().FrameRate(double.PositiveInfinity).Validate(), "finite positive");
        AssertThrows<ArgumentOutOfRangeException>(() => new VideoOptions().FrameRate(0), "Frame rate 0 must be rejected.");
    }

    private static void BitrateMustMatchFfmpegFormat()
    {
        foreach (var valid in new[] { "2M", "500k", "750K", "1.5M", "1000000", "4G" })
        {
            Expect(new VideoOptions().Bitrate(valid).Validate().Count == 0, $"Bitrate '{valid}' should be valid.");
        }

        foreach (var invalid in new[] { "2m", "2MB", "2Mbps", "fast", "1.2.3M", " 2M", "2M ", "M", "-500k", "2 M" })
        {
            AssertSingleProblem(new VideoOptions().Bitrate(invalid).Validate(), "FFmpeg format", invalid);
        }
    }

    private static void BitrateMustBePositive()
    {
        AssertSingleProblem(new VideoOptions().Bitrate("0k").Validate(), "greater than zero");
        AssertSingleProblem(new VideoOptions().Bitrate("0.0M").Validate(), "greater than zero");
    }

    private static void CrfMustFitCodecRange()
    {
        Expect(new VideoOptions().Codec("libx264").Crf(0).Validate().Count == 0, "CRF 0 should be valid for libx264.");
        Expect(new VideoOptions().Codec("libx264").Crf(51).Validate().Count == 0, "CRF 51 should be valid for libx264.");
        AssertSingleProblem(new VideoOptions().Codec("libx264").Crf(52).Validate(), "0-51");

        Expect(new VideoOptions().Codec("libx265").Crf(51).Validate().Count == 0, "CRF 51 should be valid for libx265.");
        AssertSingleProblem(new VideoOptions().Codec("libx265").Crf(52).Validate(), "0-51");

        // Codec may be set after CRF; validation must still use the final codec.
        AssertSingleProblem(new VideoOptions().Crf(60).Codec("libx264").Validate(), "0-51");
        Expect(new VideoOptions().Codec("libvpx-vp9").Crf(60).Validate().Count == 0, "CRF 60 should be valid for libvpx-vp9.");

        AssertThrows<ArgumentOutOfRangeException>(() => new VideoOptions().Crf(-1), "Negative CRF must be rejected.");
        AssertThrows<ArgumentOutOfRangeException>(() => new VideoOptions().Crf(64), "CRF above 63 must be rejected.");
    }

    private static void CrfAndBitrateAreMutuallyExclusiveForX26x()
    {
        AssertSingleProblem(new VideoOptions().Codec("libx264").Crf(23).Bitrate("2M").Validate(), "mutually exclusive");
        AssertSingleProblem(new VideoOptions().Codec("libx265").Crf(28).Bitrate("1500k").Validate(), "mutually exclusive");

        // With no codec set, ffmpeg usually picks libx264, so the conflict is still reported.
        AssertSingleProblem(new VideoOptions().Crf(23).Bitrate("2M").Validate(), "mutually exclusive");

        Expect(new VideoOptions().Codec("libx264").Crf(23).Validate().Count == 0, "CRF alone should be valid.");
        Expect(new VideoOptions().Codec("libx264").Bitrate("2M").Validate().Count == 0, "Bitrate alone should be valid.");
    }

    private static void CrfAndBitrateAllowedForConstantQualityVp9()
    {
        Expect(new VideoOptions().Codec("libvpx-vp9").Crf(31).Bitrate("2M").Validate().Count == 0,
            "libvpx-vp9 constrained-quality (CRF plus bitrate) should be valid.");
    }

    private static void AllProblemsAreReportedAtOnce()
    {
        var options = new VideoOptions()
            .Codec("libx264")
            .Crf(60)
            .Bitrate("2MB")
            .FrameRate(double.NaN)
            .Resolution(-1, -1);

        var problems = options.Validate();

        // CRF out of range, CRF with bitrate, bitrate format, frame rate, both dimensions automatic.
        Expect(problems.Count == 5, $"Expected 5 problems, got {problems.Count}: {string.Join(" | ", problems)}");
    }

    private static void EnsureValidThrowsWithErrors()
    {
        var options = new VideoOptions().Codec("libx264").Crf(23).Bitrate("2M");

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
            .AddOutput("good.mp4", o => o.WithVideo(v => v.Codec("libx264").Resolution(1280, 720)))
            .AddOutput("bad.mp4", o => o.WithVideo(v => v.Codec("libx264").Crf(23).Bitrate("2M")));

        try
        {
            command.RunAsync().GetAwaiter().GetResult();
        }
        catch (FFmpegOptionsValidationException ex)
        {
            Expect(ex.Errors.Count == 1, "Only the invalid output should be reported.");
            Expect(ex.Errors[0].StartsWith("Output 'bad.mp4':", StringComparison.Ordinal), "Error should name the offending output.");
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
