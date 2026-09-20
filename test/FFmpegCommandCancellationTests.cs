#nullable enable

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

/// <summary>
/// Ad-hoc tests for cancellation and process termination behavior.
/// </summary>
public static class FFmpegCommandCancellationTests
{
    /// <summary>
    /// Runs all cancellation tests.
    /// </summary>
    public static async Task RunTests()
    {
        Console.WriteLine("Running FFmpegCommandCancellationTests...");
        await TestCancellationKillsProcessTree();
        Console.WriteLine("All cancellation tests passed.");
    }

    private static async Task TestCancellationKillsProcessTree()
    {
        // Use a long-running dummy process to simulate ffmpeg
        var startInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "ping" : "sleep",
            Arguments = OperatingSystem.IsWindows() ? "-t 127.0.0.1" : "300",
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        
        try
        {
            await Task.Run(async () =>
            {
                try
                {
                    await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Expected
                }
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
            await process.WaitForExitAsync().ConfigureAwait(false);
        }

        if (process.HasExited)
        {
            Console.WriteLine("  [PASS] Process was successfully killed and exited after cancellation.");
        }
        else
        {
            Console.WriteLine("  [FAIL] Process did not exit after cancellation.");
            throw new Exception("Test failed: process still running.");
        }
    }
}
