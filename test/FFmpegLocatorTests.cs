#nullable enable

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FFmpegFluent;

/// <summary>
/// Ad-hoc unit tests for <see cref="FFmpegLocator"/>.
/// </summary>
public static class FFmpegLocatorTests
{
    public static void RunAll()
    {
        Console.WriteLine("=== FFmpegLocator Tests ===");
        TestTryLocateDoesNotThrow();
        TestTryLocateReturnsPathWhenFound();
        TestExceptionContainsSearchedLocations();
        Console.WriteLine("=== FFmpegLocator Tests Complete ===");
    }

    private static void TestTryLocateDoesNotThrow()
    {
        var locator = new FFmpegLocator();
        try
        {
            var _ = locator.TryLocate(out var _);
            Console.WriteLine("[PASS] TryLocate executed without throwing.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAIL] TryLocate threw an exception: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void TestTryLocateReturnsPathWhenFound()
    {
        var locator = new FFmpegLocator();
        if (locator.TryLocate(out var path) && !string.IsNullOrEmpty(path))
        {
            Console.WriteLine($"[PASS] TryLocate returned path: {path}");
        }
        else
        {
            Console.WriteLine("[SKIP] TryLocate did not find ffmpeg (expected if not in PATH).");
        }
    }

    private static void TestExceptionContainsSearchedLocations()
    {
        try
        {
            var locator = new FFmpegLocator();
            var _ = locator.FFmpegPath;
            Console.WriteLine("[SKIP] FFmpegNotFoundException not triggered (ffmpeg found in PATH).");
        }
        catch (FFmpegNotFoundException ex)
        {
            if (ex.SearchedLocations.Length > 0 && ex.Message.Contains("Searched locations:"))
            {
                Console.WriteLine($"[PASS] FFmpegNotFoundException contains searched locations: {string.Join(", ", ex.SearchedLocations)}");
            }
            else
            {
                Console.WriteLine("[FAIL] FFmpegNotFoundException missing searched locations or message format.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[INFO] Other exception: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
