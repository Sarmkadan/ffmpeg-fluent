#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

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
        TestCapturingLoggerReceivesLocationDiagnostics();
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

    private static void TestCapturingLoggerReceivesLocationDiagnostics()
    {
        var originalPath = Environment.GetEnvironmentVariable("PATH");
        var originalFFmpegPath = Environment.GetEnvironmentVariable("FFMPEG_PATH");
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"ffmpeg-locator-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var ffmpegPath = Path.Combine(tempDirectory,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg");
            var ffprobePath = Path.Combine(tempDirectory,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe");
            File.WriteAllText(ffmpegPath, string.Empty);
            File.WriteAllText(ffprobePath, string.Empty);

            var logger = new CapturingLogger();
            var located = new FFmpegLocator(ffmpegPath, ffprobePath, logger).TryLocate(out _);

            Environment.SetEnvironmentVariable("FFMPEG_PATH", null);
            Environment.SetEnvironmentVariable("PATH", tempDirectory);
            _ = new FFmpegLocator(logger).TryLocate(out _);

            var debugCandidate = logger.Entries.Any(entry =>
                entry.Level == LogLevel.Debug && entry.Properties.ContainsKey("CandidatePath"));
            var chosenPath = logger.Entries.Any(entry =>
                entry.Level == LogLevel.Information && Equals(entry.Properties["Path"], Path.GetFullPath(ffmpegPath)));

            File.Delete(ffmpegPath);
            _ = new FFmpegLocator(logger).TryLocate(out _);
            var notFound = logger.Entries.Any(entry =>
                entry.Level == LogLevel.Warning &&
                entry.Properties.TryGetValue("SearchedLocations", out var value) &&
                value is IReadOnlyCollection<string> locations && locations.Count > 0);

            if (located && debugCandidate && chosenPath && notFound)
            {
                Console.WriteLine("[PASS] ILogger captured Debug, Information, and structured Warning diagnostics.");
            }
            else
            {
                Console.WriteLine("[FAIL] ILogger did not capture all expected location diagnostics.");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", originalPath);
            Environment.SetEnvironmentVariable("FFMPEG_PATH", originalFFmpegPath);
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<LogEntry> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var properties = state as IEnumerable<KeyValuePair<string, object?>>;
            Entries.Add(new LogEntry(logLevel,
                properties?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? new Dictionary<string, object?>()));
        }
    }

    private sealed record LogEntry(LogLevel Level, IReadOnlyDictionary<string, object?> Properties);
}
