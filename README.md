# ffmpeg-fluent

## Architecture

FFmpegFluent is a fluent C# DSL that shells out to the `ffmpeg`/`ffprobe` CLI - no native
bindings, no dependencies beyond the BCL. It has two layers: a composable core
(`FFmpegCommand` + `InputFile`/`OutputFile`/`FilterGraph` with progress reporting via
`FFmpegProgress`) and self-contained presets for common jobs (concat, GIF, thumbnails,
audio extraction, watermarking, media probing). See
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full component breakdown, data flow,
and known limitations.

## HardwareAccelOptions
The `HardwareAccelOptions` type allows you to configure hardware acceleration options for FFmpeg. It provides a way to specify the hardware acceleration kind, device, and input arguments.

## FFmpegProgress
The `FFmpegProgress` type represents the progress information reported by FFmpeg during a conversion. It exposes the amount of processed time, frames per second, bitrate, frame count, and speed multiplier, and can be parsed from FFmpeg’s progress output.

Example usage:

## FFmpegException
The `FFmpegException` type represents a failure raised when an FFmpeg process exits with an error. It exposes the process exit code, the captured standard error output, and the full command line that was executed, making failed invocations easy to diagnose and log.

Example usage:
```csharp
var preset = new SubtitlePreset("input.mp4", "output-with-subtitles.mp4")
    .WithSubtitle("subtitles.srt");

try
{
    await preset.RunAsync();
}
catch (FFmpegException ex)
{
    Console.WriteLine($"FFmpeg failed with exit code {ex.ExitCode}.");
    Console.WriteLine($"Command line: {ex.CommandLine}");
    Console.WriteLine($"Standard error:{Environment.NewLine}{ex.StdErr}");
}
```

## ExtractAudioPresetExtensions
The `ExtractAudioPresetExtensions` type provides a set of convenience methods to create common audio extraction presets. These presets can be used to extract audio from a video file and save it in a specific format.

Example usage:

## ConcatPresetExtensions
The `ConcatPresetExtensions` type offers fluent extension methods to build a `ConcatPreset` for concatenating multiple inputs, optionally applying hardware‑accelerated re‑encoding, quality presets, and lead‑in delays. It also provides an asynchronous helper to run the preset and retrieve the generated output file path.

## OutputFileExtensions
The `OutputFileExtensions` type provides fluent extension methods to configure output file properties like container format, video/audio codecs, bitrates, frame rate, resolution, metadata, and overwrite behavior. These methods enable building an `OutputFile` configuration using a fluent API.

Example usage:

## SubtitlePreset
The `SubtitlePreset` type burns a subtitle file (e.g., `.srt` or `.ass`) into a video using FFmpeg's `subtitles` video filter. It takes the input/output paths plus the subtitle file, escapes the subtitle path for safe inclusion in the filter argument, and can build an `FFmpegCommand`, produce raw ffmpeg arguments, or run the conversion directly.

Example usage:
```csharp
var preset = new SubtitlePreset("input.mp4", "output-with-subtitles.mp4")
    .WithSubtitle("subtitles.srt");

// Burn the subtitles into the output video.
await preset.RunAsync();

// Or inspect the generated command/arguments instead of running immediately.
FFmpegCommand command = preset.Build();
string[] arguments = preset.BuildArguments();
```

## NormalizeAudioPreset
The `NormalizeAudioPreset` type normalizes the audio loudness of a media file using FFmpeg's `loudnorm` filter. It lets you configure the target integrated loudness (LUFS), true peak (dBTP), and loudness range (LRA), optionally enable normalization and metadata printing, and supports two-pass workflows by supplying measurement data from a first pass via `WithMeasurementInput`. Like the other presets, it can produce raw ffmpeg arguments via `BuildArguments()` or run the conversion directly with `RunAsync()`, and `ToString()` returns a human-readable summary of the configured options.

Example usage:
```csharp
var preset = new NormalizeAudioPreset("input.mp4", "output-normalized.mp4")
    .WithTargetIntegrated(-16.0)
    .WithTargetTruePeak(-1.5)
    .WithTargetLra(11.0)
    .WithNormalize(true)
    .WithPrintMetadata(true);

// Inspect the generated ffmpeg arguments instead of running immediately.
string[] arguments = preset.BuildArguments();

// Run the loudness normalization.
await preset.RunAsync();
```

## TrimPreset
The `TrimPreset` type extracts a time-coded segment from a media file by seeking to a start position (`From`) and stopping at an end position (`To`) or after a fixed length (`Duration`). When the trimmed output must match the source exactly it can be re-encoded via `WithReencode`, while `StreamCopy` performs a fast lossless cut without re-encoding. Like the other presets, it can build an `FFmpegCommand` via `Build()`, produce raw ffmpeg arguments via `BuildArguments()`, or run the trim directly with `Run()`.

Example usage:
```csharp
var preset = new TrimPreset("input.mp4", "output-trimmed.mp4")
    .From(TimeSpan.FromSeconds(10))
    .To(TimeSpan.FromSeconds(40))
    .StreamCopy();

// Inspect the generated ffmpeg arguments instead of running immediately.
string[] arguments = preset.BuildArguments();

// Perform the trim.
preset.Run();
```
