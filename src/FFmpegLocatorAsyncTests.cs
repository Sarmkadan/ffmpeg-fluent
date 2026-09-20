#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

/// <summary>
/// Ad-hoc tests for <see cref="FFmpegLocator.LocateAsync"/>.
/// </summary>
public static class FFmpegLocatorAsyncTests
{
    /// <summary>
    /// Runs all async locator tests.
    /// </summary>
    public static async Task RunTests()
    {
        Console.WriteLine("Running FFmpegLocatorAsyncTests...");
        
        try
        {
            var locator = new FFmpegLocator();
            var (path, version) = await locator.LocateAsync(CancellationToken.None);
            Console.WriteLine($"[PASS] LocateAsync found: {path} (version: {version})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAIL] LocateAsync failed: {ex.Message}");
        }

        try
        {
            // Test with explicit path (if ffmpeg is in PATH, use it)
            var envPath = Environment.GetEnvironmentVariable("FFMPEG_PATH") ?? "ffmpeg";
            var locator2 = new FFmpegLocator(envPath, "ffprobe");
            var (path2, version2) = await locator2.LocateAsync(CancellationToken.None);
            Console.WriteLine($"[PASS] LocateAsync with explicit path found: {path2} (version: {version2})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAIL] LocateAsync with explicit path failed: {ex.Message}");
        }

        Console.WriteLine("FFmpegLocatorAsyncTests completed.");
    }
}
