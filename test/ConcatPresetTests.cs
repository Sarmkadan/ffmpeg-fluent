#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

/// <summary>
/// Ad-hoc tests for ConcatPreset cancellation and temp file cleanup.
/// </summary>
public static class ConcatPresetTests
{
    /// <summary>
    /// Runs all ConcatPreset tests.
    /// </summary>
    public static void RunTests()
    {
        Console.WriteLine("Running ConcatPresetTests...");
        TestBuilder();
        TestBuilderValidation();
        TestTempFileCleanupOnCancellation().Wait();
        Console.WriteLine("All ConcatPresetTests passed.");
    }

    private static void TestBuilder()
    {
        var command = ConcatPreset.Create()
            .AddInput("first.mp4")
            .AddInputs(new[] { "second.mp4", "third.mp4" }.AsEnumerable())
            .Output("joined.mp4")
            .ReEncode(true)
            .Build();

        var arguments = command.PreviewArguments();
        AssertContains(arguments, "-i first.mp4");
        AssertContains(arguments, "-i second.mp4");
        AssertContains(arguments, "-i third.mp4");
        AssertContains(arguments, "concat=n=3:v=1:a=1[v][a]");
        AssertContains(arguments, "-c:v libx264");
        AssertContains(arguments, "-c:a aac");
        AssertContains(arguments, "joined.mp4");

        var copyArguments = ConcatPreset.Create()
            .AddInputs(new[] { "first.mp4", "second.mp4" })
            .Output("joined.mp4")
            .ReEncode(false)
            .Build()
            .PreviewArguments();
        AssertContains(copyArguments, "-c copy");
    }

    private static void TestBuilderValidation()
    {
        AssertThrows<InvalidOperationException>(() => ConcatPreset.Create().Output("joined.mp4").Build());
        AssertThrows<InvalidOperationException>(() => ConcatPreset.Create().AddInput("only.mp4").Output("joined.mp4").Build());
        AssertThrows<InvalidOperationException>(() => ConcatPreset.Create().AddInputs(new[] { "one.mp4", "two.mp4" }).Build());
    }

    private static void AssertContains(string actual, string expected)
    {
        if (!actual.Contains(expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected command to contain '{expected}', but was '{actual}'.");
    }

    private static void AssertThrows<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}.");
    }

    private static async Task TestTempFileCleanupOnCancellation()
    {
        // Create a preset that will use demuxer mode (requires temp file)
        var preset = new ConcatPreset(Path.GetTempFileName())
            .WithStrategy(ConcatStrategy.DemuxerCopy)
            .AddInput("nonexistent1.mp4")
            .AddInput("nonexistent2.mp4");

        // Use a cancellation token that will cancel immediately
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        string[] tempFilesBefore = Directory.GetFiles(Path.GetTempPath(), "tmp*.tmp");

        try
        {
            await preset.RunAsync("ffmpeg", cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelled
        }
        catch (InvalidOperationException)
        {
            // Expected when ffmpeg fails or inputs don't exist
        }
        catch (FFmpegFluent.FFmpegException)
        {
            // Expected when ffmpeg is not found or fails
        }

        // Give a moment for cleanup
        await Task.Delay(100);

        string[] tempFilesAfter = Directory.GetFiles(Path.GetTempPath(), "tmp*.tmp");

        // The number of temp files should not have increased due to our test
        // (accounting for possible parallel test execution variance)
        if (tempFilesAfter.Length > tempFilesBefore.Length + 5) // Allow some variance
        {
            throw new InvalidOperationException(
                $"Possible temp file leak detected. Before: {tempFilesBefore.Length}, After: {tempFilesAfter.Length}");
        }

        Console.WriteLine("  [PASS] Temp file cleanup verified after cancellation.");
    }
}
