# ffmpeg-fluent

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
