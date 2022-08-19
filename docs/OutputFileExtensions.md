# OutputFileExtensions

Provides a set of static factory members that produce pre-configured `OutputFile` instances representing common output formats, codec combinations, and metadata options. These members simplify the construction of output specifications when building FFmpeg command pipelines, allowing callers to select a format preset and optionally chain additional configuration without manually instantiating and configuring `OutputFile` objects.

## API

All members are `public static` properties or methods that return a new `OutputFile` instance. Unless otherwise noted, none of these members accept parameters or throw exceptions during invocation—they return a valid `OutputFile` object ready for further configuration or direct use in a pipeline.

### Format Presets

- **`AsMp4`**  
  Returns an `OutputFile` configured for the MP4 container format. The returned object applies the appropriate format flag (`-f mp4`) and sensible defaults for MP4 output.

- **`AsMkv`**  
  Returns an `OutputFile` configured for the Matroska (MKV) container format. Applies `-f matroska` and associated defaults.

- **`AsWebM`**  
  Returns an `OutputFile` configured for the WebM container format. Applies `-f webm` and defaults suitable for web streaming.

- **`AsMov`**  
  Returns an `OutputFile` configured for the QuickTime (MOV) container format. Applies `-f mov` and associated defaults.

### Codec Presets

- **`WithH264Codec`**  
  Returns an `OutputFile` with the video codec set to H.264 (`-c:v libx264` or equivalent). The returned object inherits any previously set format or container configuration.

- **`WithH265Codec`**  
  Returns an `OutputFile` with the video codec set to H.265/HEVC (`-c:v libx265` or equivalent). The returned object inherits any previously set format or container configuration.

- **`WithVp9Codec`**  
  Returns an `OutputFile` with the video codec set to VP9 (`-c:v libvpx-vp9` or equivalent). The returned object inherits any previously set format or container configuration.

- **`WithAacAudio`**  
  Returns an `OutputFile` with the audio codec set to AAC (`-c:a aac` or equivalent). The returned object inherits any previously set format or container configuration.

- **`WithMp3Audio`**  
  Returns an `OutputFile` with the audio codec set to MP3 (`-c:a libmp3lame` or equivalent). The returned object inherits any previously set format or container configuration.

- **`WithOpusAudio`**  
  Returns an `OutputFile` with the audio codec set to Opus (`-c:a libopus` or equivalent). The returned object inherits any previously set format or container configuration.

### Encoding Parameters

- **`WithVideoBitrate`**  
  Accepts a bitrate value (typically in kilobits per second as a string or integer) and returns an `OutputFile` with the video bitrate flag applied (`-b:v`). Throws `ArgumentNullException` or `ArgumentException` if the supplied value is null or invalid.

- **`WithAudioBitrate`**  
  Accepts a bitrate value and returns an `OutputFile` with the audio bitrate flag applied (`-b:a`). Throws `ArgumentNullException` or `ArgumentException` if the supplied value is null or invalid.

- **`WithFrameRate`**  
  Accepts a frame rate value (integer or fractional representation) and returns an `OutputFile` with the frame rate flag applied (`-r`). Throws `ArgumentNullException` or `ArgumentException` if the supplied value is null or invalid.

- **`WithResolution`**  
  Accepts a resolution string (e.g., `"1920x1080"`) or dimensions object and returns an `OutputFile` with the resolution/scaling flags applied. Throws `ArgumentNullException` or `ArgumentException` if the supplied value is null or invalid.

### Metadata

- **`WithTitle`**  
  Accepts a string and returns an `OutputFile` with the title metadata flag applied (`-metadata title=...`). Throws `ArgumentNullException` if the supplied value is null.

- **`WithAuthor`**  
  Accepts a string and returns an `OutputFile` with the author/artist metadata flag applied (`-metadata author=...`). Throws `ArgumentNullException` if the supplied value is null.

- **`WithCopyright`**  
  Accepts a string and returns an `OutputFile` with the copyright metadata flag applied (`-metadata copyright=...`). Throws `ArgumentNullException` if the supplied value is null.

### File Conflict Behavior

- **`ForceOverwrite`**  
  Returns an `OutputFile` configured to overwrite the output file if it already exists (applies the `-y` flag). No parameters; never throws.

- **`PreserveExisting`**  
  Returns an `OutputFile` configured to fail if the output file already exists (applies the `-n` flag). No parameters; never throws.

## Usage

### Example 1: Transcode to H.264 MP4 with Metadata

```csharp
var output = OutputFileExtensions.AsMp4
    .WithH264Codec
    .WithAacAudio
    .WithVideoBitrate("5000k")
    .WithResolution("1920x1080")
    .WithFrameRate(30)
    .WithTitle("Conference Recording")
    .WithAuthor("Jane Doe")
    .ForceOverwrite;

// Use 'output' in an FFmpeg pipeline:
// ffmpeg.Input("input.mov").Output(output).Run();
```

### Example 2: Create VP9 WebM with Opus Audio

```csharp
var output = OutputFileExtensions.AsWebM
    .WithVp9Codec
    .WithOpusAudio
    .WithVideoBitrate("2500k")
    .WithAudioBitrate("128k")
    .PreserveExisting;

// Use 'output' in an FFmpeg pipeline:
// ffmpeg.Input("source.avi").Output(output).Run();
```

## Notes

- Each member returns a new `OutputFile` instance; the original instance is not mutated. This allows chaining without unintended side effects.
- The order of chaining generally does not matter because each call produces a fresh object with the cumulative configuration applied. However, calling a format preset after a codec preset will override the container format, which may be intentional or not depending on the desired output.
- Members that accept parameters perform validation immediately. Passing a null value to `WithTitle`, `WithAuthor`, `WithCopyright`, or an invalid bitrate/resolution string to the encoding parameter members will result in an exception at the point of invocation, not during pipeline execution.
- `ForceOverwrite` and `PreserveExisting` are mutually exclusive in effect; the last one applied in a chain wins. Combining them in the same chain is allowed but pointless—only the final flag will be present in the generated arguments.
- These members are static and stateless; they are safe to call concurrently from multiple threads without any synchronization. The returned `OutputFile` objects are independent and not shared.
