#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FFmpegFluent;

/// <summary>
/// Integration tests for VideoOptions covering realistic scenarios.
/// These tests require ffmpeg and ffprobe to be available.
/// </summary>
public static class VideoOptionsIntegrationTests
{
    private static readonly string? s_ffmpegPath = Environment.GetEnvironmentVariable("FFMPEG_PATH");
    private static readonly string? s_ffprobePath = Environment.GetEnvironmentVariable("FFPROBE_PATH");
    private static readonly bool s_ffmpegAvailable = s_ffmpegPath != null && s_ffprobePath != null;

    /// <summary>
    /// Runs all integration tests.
    /// </summary>
    public static async Task RunAllAsync()
    {
        Console.WriteLine("=== VideoOptions Integration Tests ===");

        if (!s_ffmpegAvailable)
        {
            Console.WriteLine("[SKIP] FFmpeg/FFprobe paths not configured via FFMPEG_PATH/FFPROBE_PATH env vars. Skipping all integration tests.");
            Console.WriteLine("=== VideoOptions Integration Tests Complete (skipped) ===");
            return;
        }

        Console.WriteLine($"[INFO] Using ffmpeg: {s_ffmpegPath}");
        Console.WriteLine($"[INFO] Using ffprobe: {s_ffprobePath}");

        var tempDir = Path.Combine(Path.GetTempPath(), $"ffmpeg-fluent-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            await TestScalePreserveAspectRatioAsync(tempDir).ConfigureAwait(false);
            await TestChangeFrameRateAsync(tempDir).ConfigureAwait(false);
            await TestTrimSegmentAsync(tempDir).ConfigureAwait(false);
            await TestThumbnailPngAsync(tempDir).ConfigureAwait(false);
            await TestMissingInputFailsCleanlyAsync(tempDir).ConfigureAwait(false);
        }
        finally
        {
            // Cleanup
            try { Directory.Delete(tempDir, recursive: true); } catch { }
        }

        Console.WriteLine("=== VideoOptions Integration Tests Complete ===");
    }

    /// <summary>
    /// Generates a 5-second test video using lavfi testsrc.
    /// </summary>
    private static async Task<string> GenerateTestVideoAsync(string outputPath, string ffmpegPath)
    {
        var args = $"-f lavfi -i testsrc=duration=5:size=1280x720:rate=30 -c:v libx264 -pix_fmt yuv420p -y \"{outputPath}\"";
        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync().ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
            throw new InvalidOperationException($"Failed to generate test video: {error}");
        }

        return outputPath;
    }

    /// <summary>
    /// Probes a media file and returns MediaInfo.
    /// </summary>
    private static async Task<MediaInfo> ProbeAsync(string filePath, string ffprobePath)
    {
        return await MediaInfo.ProbeAsync(filePath, ffprobePath).ConfigureAwait(false);
    }

    /// <summary>
    /// Test: Scale to 640x360 while preserving aspect ratio.
    /// </summary>
    private static async Task TestScalePreserveAspectRatioAsync(string tempDir)
    {
        Console.WriteLine("\n[TEST] Scale to 640x360 preserving aspect ratio...");

        var inputPath = Path.Combine(tempDir, "input_scale.mp4");
        var outputPath = Path.Combine(tempDir, "output_scale.mp4");

        await GenerateTestVideoAsync(inputPath, s_ffmpegPath!).ConfigureAwait(false);

        // Use FFmpegCommand with VideoOptions to scale
        var command = FFmpegCommand.Create()
            .AddInput(inputPath)
            .AddOutput(outputPath, output =>
            {
                output.WithVideo(video =>
                {
                    video.Codec("libx264")
                        .Preset("fast")
                        .Resolution(640, 360)
                        .PixelFormat("yuv420p");
                });
            });

        await command.RunAsync().ConfigureAwait(false);

        // Verify with ffprobe
        var info = await ProbeAsync(outputPath, s_ffprobePath!).ConfigureAwait(false);

        if (info.Width != 640 || info.Height != 360)
        {
            throw new InvalidOperationException($"Scale test failed: expected 640x360, got {info.Width}x{info.Height}");
        }

        // Verify duration is approximately 5 seconds (allowing small tolerance)
        var expectedDuration = TimeSpan.FromSeconds(5);
        var tolerance = TimeSpan.FromSeconds(0.5);
        if (info.Duration < expectedDuration - tolerance || info.Duration > expectedDuration + tolerance)
        {
            throw new InvalidOperationException($"Scale test failed: expected duration ~5s, got {info.Duration.TotalSeconds}s");
        }

        Console.WriteLine($"[PASS] Scale test: {info.Width}x{info.Height}, duration={info.Duration.TotalSeconds:F1}s");
    }

    /// <summary>
    /// Test: Change frame rate to 15 fps.
    /// </summary>
    private static async Task TestChangeFrameRateAsync(string tempDir)
    {
        Console.WriteLine("\n[TEST] Change frame rate to 15 fps...");

        var inputPath = Path.Combine(tempDir, "input_fps.mp4");
        var outputPath = Path.Combine(tempDir, "output_fps.mp4");

        await GenerateTestVideoAsync(inputPath, s_ffmpegPath!).ConfigureAwait(false);

        var command = FFmpegCommand.Create()
            .AddInput(inputPath)
            .AddOutput(outputPath, output =>
            {
                output.WithVideo(video =>
                {
                    video.Codec("libx264")
                        .Preset("fast")
                        .FrameRate(15)
                        .PixelFormat("yuv420p");
                });
            });

        await command.RunAsync().ConfigureAwait(false);

        var info = await ProbeAsync(outputPath, s_ffprobePath!).ConfigureAwait(false);

        // Frame rate should be 15 fps (allow small tolerance)
        if (Math.Abs(info.FrameRate - 15.0) > 0.5)
        {
            throw new InvalidOperationException($"Frame rate test failed: expected ~15 fps, got {info.FrameRate} fps");
        }

        // Duration should still be ~5 seconds
        var expectedDuration = TimeSpan.FromSeconds(5);
        var tolerance = TimeSpan.FromSeconds(0.5);
        if (info.Duration < expectedDuration - tolerance || info.Duration > expectedDuration + tolerance)
        {
            throw new InvalidOperationException($"Frame rate test failed: expected duration ~5s, got {info.Duration.TotalSeconds}s");
        }

        Console.WriteLine($"[PASS] Frame rate test: {info.FrameRate:F1} fps, duration={info.Duration.TotalSeconds:F1}s");
    }

    /// <summary>
    /// Test: Trim a segment (seek + duration).
    /// </summary>
    private static async Task TestTrimSegmentAsync(string tempDir)
    {
        Console.WriteLine("\n[TEST] Trim segment (seek 1s, duration 2s)...");

        var inputPath = Path.Combine(tempDir, "input_trim.mp4");
        var outputPath = Path.Combine(tempDir, "output_trim.mp4");

        await GenerateTestVideoAsync(inputPath, s_ffmpegPath!).ConfigureAwait(false);

        // Use -ss before -i for fast seek, and -t for duration
        var command = FFmpegCommand.Create()
            .AddInput(inputPath)
            .AddOutput(outputPath, output =>
            {
                output.Option("ss", "1");  // Seek to 1 second
                output.Option("t", "2");  // Duration 2 seconds
                output.WithVideo(video =>
                {
                    video.Codec("libx264")
                        .Preset("fast")
                        .PixelFormat("yuv420p");
                });
            });

        await command.RunAsync().ConfigureAwait(false);

        var info = await ProbeAsync(outputPath, s_ffprobePath!).ConfigureAwait(false);

        // Duration should be ~2 seconds (allowing tolerance)
        var expectedDuration = TimeSpan.FromSeconds(2);
        var tolerance = TimeSpan.FromSeconds(0.5);
        if (info.Duration < expectedDuration - tolerance || info.Duration > expectedDuration + tolerance)
        {
            throw new InvalidOperationException($"Trim test failed: expected duration ~2s, got {info.Duration.TotalSeconds}s");
        }

        Console.WriteLine($"[PASS] Trim test: duration={info.Duration.TotalSeconds:F1}s");
    }

    /// <summary>
    /// Test: Produce a thumbnail PNG at a given timestamp.
    /// </summary>
    private static async Task TestThumbnailPngAsync(string tempDir)
    {
        Console.WriteLine("\n[TEST] Produce thumbnail PNG at timestamp 2s...");

        var inputPath = Path.Combine(tempDir, "input_thumb.mp4");
        var outputPath = Path.Combine(tempDir, "thumbnail.png");

        await GenerateTestVideoAsync(inputPath, s_ffmpegPath!).ConfigureAwait(false);

        // Use -ss for timestamp, -frames:v 1 for single frame, output as PNG
        // Use OutputFile.Option to add -ss and -frames:v
        var command = FFmpegCommand.Create()
            .AddInput(inputPath)
            .AddOutput(outputPath, output =>
            {
                output.Option("ss", "2");  // Seek to 2 seconds
                output.Option("frames:v", "1");  // Single frame
                output.WithVideo(video =>
                {
                    video.Codec("png")
                        .PixelFormat("yuv420p");
                });
            });

        await command.RunAsync().ConfigureAwait(false);

        // Verify PNG header (first 8 bytes: 89 50 4E 47 0D 0A 1A 0A)
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var fileBytes = await File.ReadAllBytesAsync(outputPath).ConfigureAwait(false);

        if (fileBytes.Length < 8)
        {
            throw new InvalidOperationException("Thumbnail test failed: output file too small to be a valid PNG");
        }

        for (int i = 0; i < 8; i++)
        {
            if (fileBytes[i] != pngHeader[i])
            {
                throw new InvalidOperationException($"Thumbnail test failed: invalid PNG header at byte {i}. Expected 0x{pngHeader[i]:X2}, got 0x{fileBytes[i]:X2}");
            }
        }

        Console.WriteLine($"[PASS] Thumbnail test: valid PNG header, size={fileBytes.Length} bytes");
    }

    /// <summary>
    /// Test: FFmpegCommand fails cleanly when input path does not exist.
    /// </summary>
    private static async Task TestMissingInputFailsCleanlyAsync(string tempDir)
    {
        Console.WriteLine("\n[TEST] Missing input fails cleanly...");

        var nonExistentPath = Path.Combine(tempDir, "this_file_does_not_exist.mp4");
        var outputPath = Path.Combine(tempDir, "output_missing.mp4");

        var command = FFmpegCommand.Create()
            .AddInput(nonExistentPath)
            .AddOutput(outputPath);

        try
        {
            await command.RunAsync().ConfigureAwait(false);
            throw new InvalidOperationException("Missing input test failed: expected FFmpegException but command succeeded");
        }
        catch (FFmpegException ex)
        {
            // Verify the exception contains useful information
            if (ex.ExitCode == 0)
            {
                throw new InvalidOperationException("Missing input test failed: FFmpegException has exit code 0");
            }

            if (string.IsNullOrWhiteSpace(ex.StdErr))
            {
                throw new InvalidOperationException("Missing input test failed: FFmpegException has empty StdErr");
            }

            if (string.IsNullOrWhiteSpace(ex.CommandLine))
            {
                throw new InvalidOperationException("Missing input test failed: FFmpegException has empty CommandLine");
            }

            // Verify stderr contains error about missing file
            var stderrLower = ex.StdErr.ToLowerInvariant();
            if (!stderrLower.Contains("no such file") &&
                !stderrLower.Contains("no such file or directory") &&
                !stderrLower.Contains("file not found") &&
                !stderrLower.Contains("cannot open"))
            {
                throw new InvalidOperationException($"Missing input test failed: StdErr doesn't indicate missing file. StdErr: {ex.StdErr}");
            }

            Console.WriteLine($"[PASS] Missing input test: exit code={ex.ExitCode}, stderr captured ({ex.StdErr.Length} chars)");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Missing input test failed: unexpected exception type {ex.GetType().Name}: {ex.Message}");
        }
    }
}