# ExtractAudioPresetExtensions
The `ExtractAudioPresetExtensions` class provides a set of predefined presets for extracting audio from multimedia files using FFmpeg. These presets offer a convenient way to specify common audio extraction settings, such as output format and bitrate, without requiring manual configuration of FFmpeg command-line options.

## API
The `ExtractAudioPresetExtensions` class includes the following public members:
* `AsMp3`: Returns an `ExtractAudioPreset` instance configured to extract audio as MP3.
* `AsAac`: Returns an `ExtractAudioPreset` instance configured to extract audio as AAC.
* `AsFlac`: Returns an `ExtractAudioPreset` instance configured to extract audio as FLAC.
* `AsWav`: Returns an `ExtractAudioPreset` instance configured to extract audio as WAV.
* `WithBitrate`: Returns an `ExtractAudioPreset` instance configured to extract audio with a specified bitrate.
* `CopyAudioStream`: Returns an `ExtractAudioPreset` instance configured to copy the audio stream without re-encoding.
* `FromStream`: Returns an `ExtractAudioPreset` instance configured to extract audio from a specific stream.

## Usage
Here are two examples of using the `ExtractAudioPresetExtensions` class:
```csharp
// Example 1: Extract audio as MP3
var preset = ExtractAudioPresetExtensions.AsMp3;
var ffmpegCommand = new FFMpegCommand(preset);
ffmpegCommand.Execute();

// Example 2: Extract audio with a specified bitrate
var preset = ExtractAudioPresetExtensions.WithBitrate(128000);
var ffmpegCommand = new FFMpegCommand(preset);
ffmpegCommand.Execute();
```

## Notes
When using the `ExtractAudioPresetExtensions` class, note that the presets are designed to be used as-is and should not be modified. Additionally, the `WithBitrate` preset requires a valid bitrate value to be specified. The `CopyAudioStream` preset will throw an exception if the input stream does not contain an audio stream. The `ExtractAudioPresetExtensions` class is thread-safe, but the `FFMpegCommand` class used in the examples may not be. It is recommended to use the `FFMpegCommand` class in a single-threaded context or to synchronize access to it when using multiple threads.
