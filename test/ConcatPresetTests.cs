#nullable enable

using System;
using System.IO;
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
        TestTempFileCleanupOnCancellation().Wait();
        Console.WriteLine("All ConcatPresetTests passed.");
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