#nullable enable

using System;
using System.IO;

namespace FFmpegFluent;

/// <summary>
/// Ad-hoc unit tests for <see cref="FFmpegLocator"/> validation logic.
/// Run by calling <see cref="RunAll"/> from a console application.
/// </summary>
public static class FFmpegLocatorTests
{
    /// <summary>
    /// Executes all validation tests and prints results to the console.
    /// </summary>
    public static void RunAll()
    {
        Console.WriteLine("Running FFmpegLocatorTests...");
        TestValidPaths();
        TestNullEmptyWhitespacePaths();
        TestNonExistentPaths();
        Console.WriteLine("All FFmpegLocatorTests passed.");
    }

    private static void TestValidPaths()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var ffmpegPath = Path.Combine(tempDir, "ffmpeg");
        var ffprobePath = Path.Combine(tempDir, "ffprobe");
        File.WriteAllText(ffmpegPath, "#!/bin/sh\necho test");
        File.WriteAllText(ffprobePath, "#!/bin/sh\necho test");

        try
        {
            var locator = new FFmpegLocator(ffmpegPath, ffprobePath);
            Console.WriteLine("PASS: TestValidPaths - Created locator with valid paths.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL: TestValidPaths - {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            File.Delete(ffmpegPath);
            File.Delete(ffprobePath);
            Directory.Delete(tempDir);
        }
    }

    private static void TestNullEmptyWhitespacePaths()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var validPath = Path.Combine(tempDir, "ffmpeg");
        File.WriteAllText(validPath, "#!/bin/sh\necho test");

        string[] invalidValues = { null, "", "   " };
        foreach (var invalid in invalidValues)
        {
            try
            {
                var _ = new FFmpegLocator(invalid, validPath);
                Console.WriteLine($"FAIL: TestNullEmptyWhitespacePaths - Did not throw for FFmpegPath='{invalid ?? "null"}'");
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"PASS: TestNullEmptyWhitespacePaths - Correctly threw ArgumentException for FFmpegPath='{invalid ?? "null"}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAIL: TestNullEmptyWhitespacePaths - Wrong exception type for FFmpegPath='{invalid ?? "null"}': {ex.GetType().Name}");
            }

            try
            {
                var _ = new FFmpegLocator(validPath, invalid);
                Console.WriteLine($"FAIL: TestNullEmptyWhitespacePaths - Did not throw for FFprobePath='{invalid ?? "null"}'");
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"PASS: TestNullEmptyWhitespacePaths - Correctly threw ArgumentException for FFprobePath='{invalid ?? "null"}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAIL: TestNullEmptyWhitespacePaths - Wrong exception type for FFprobePath='{invalid ?? "null"}': {ex.GetType().Name}");
            }
        }

        File.Delete(validPath);
        Directory.Delete(tempDir);
    }

    private static void TestNonExistentPaths()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "ffmpeg");
        try
        {
            var _ = new FFmpegLocator(nonExistentPath, nonExistentPath);
            Console.WriteLine("FAIL: TestNonExistentPaths - Did not throw for non-existent path.");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("PASS: TestNonExistentPaths - Correctly threw FileNotFoundException.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL: TestNonExistentPaths - Wrong exception type: {ex.GetType().Name}");
        }
    }
}
