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
