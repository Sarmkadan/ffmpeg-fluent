# OutputFile

`OutputFile` is a builder-style class in the `ffmpeg-fluent` project used to configure and generate FFmpeg output file arguments. It encapsulates video, audio, format, metadata, and other FFmpeg-specific options, providing a fluent interface to construct the final command-line arguments for FFmpeg execution.

## API

### `public string Path`
Gets or sets the output file path. This is the destination file where FFmpeg will write the processed media.

- **Parameters**: None (getter/setter).
- **Return value**: The current output file path as a `string`.
- **Exceptions**: Throws `ArgumentNullException` if the path is set to `null`.

---

### `public VideoOptions Video`
Gets the video configuration options. Modifying this property allows adjusting video encoding parameters such as codec, bitrate, resolution, and filters.

- **Parameters**: None (getter only).
- **Return value**: A `VideoOptions` instance representing the current video configuration.
- **Exceptions**: None.

---

### `public AudioOptions Audio`
Gets the audio configuration options. Modifying this property allows adjusting audio encoding parameters such as codec, bitrate, sample rate, and channels.

- **Parameters**: None (getter only).
- **Return value**: An `AudioOptions` instance representing the current audio configuration.
- **Exceptions**: None.

---
### `public OutputFile()`
Initializes a new instance of the `OutputFile` class with default video and audio options.

- **Parameters**: None.
- **Return value**: A new `OutputFile` instance.
- **Exceptions**: None.

---
### `public OutputFile WithVideo(Action<VideoOptions> configure)`
Configures the video options using a fluent action.

- **Parameters**:
  - `configure`: An `Action<VideoOptions>` delegate to apply video-specific settings.
- **Return value**: The current `OutputFile` instance for method chaining.
- **Exceptions**: Throws `ArgumentNullException` if `configure` is `null`.

---
### `public OutputFile WithAudio(Action<AudioOptions> configure)`
Configures the audio options using a fluent action.

- **Parameters**:
  - `configure`: An `Action<AudioOptions>` delegate to apply audio-specific settings.
- **Return value**: The current `OutputFile` instance for method chaining.
- **Exceptions**: Throws `ArgumentNullException` if `configure` is `null`.

---
### `public OutputFile Format(string format)`
Sets the output format (e.g., `mp4`, `avi`, `mkv`).

- **Parameters**:
  - `format`: The format identifier as a `string`.
- **Return value**: The current `OutputFile` instance for method chaining.
- **Exceptions**: Throws `ArgumentNullException` if `format` is `null` or empty.

---
### `public OutputFile Overwrite(bool overwrite = true)`
Configures whether to overwrite the output file if it already exists.

- **Parameters**:
  - `overwrite`: A `bool` indicating whether to overwrite (`true`) or fail (`false`).
- **Return value**: The current `OutputFile` instance for method chaining.
- **Exceptions**: None.

---
### `public OutputFile Option(string option)`
Appends a raw FFmpeg command-line option to the output file configuration.

- **Parameters**:
  - `option`: The raw FFmpeg option as a `string` (e.g., `-y`, `-hide_banner`).
- **Return value**: The current `OutputFile` instance for method chaining.
- **Exceptions**: Throws `ArgumentNullException` if `option` is `null` or empty.

---
### `public OutputFile Metadata(string metadata)`
Appends a metadata tag to the output file.

- **Parameters**:
  - `metadata`: The metadata tag as a `string` (e.g., `title="My Video"`).
- **Return value**: The current `OutputFile` instance for method chaining.
- **Exceptions**: Throws `ArgumentNullException` if `metadata` is `null` or empty.

---
### `public IEnumerable<string> BuildArgs()`
Generates the complete FFmpeg command-line arguments for the configured output file.

- **Parameters**: None.
- **Return value**: An `IEnumerable<string>` containing the command-line arguments.
- **Exceptions**: Throws `InvalidOperationException` if the output path is not set or if required options (e.g., video/audio codecs) are missing.

## Usage

```csharp
// Example 1: Basic conversion with video and audio adjustments
var output = new OutputFile()
    .WithVideo(video => video.Codec("libx264").Bitrate("5000k"))
    .WithAudio(audio => audio.Codec("aac").Bitrate("192k"))
    .Format("mp4")
    .Overwrite()
    .Path("output.mp4");

var args = output.BuildArgs();
// args: ["-i", "input.mp4", "-c:v", "libx264", "-b:v", "5000k",
//        "-c:a", "aac", "-b:a", "192k", "-f", "mp4", "-y", "output.mp4"]
```

```csharp
// Example 2: Adding metadata and raw FFmpeg options
var output = new OutputFile()
    .WithVideo(video => video.Codec("libvpx-vp9").Quality("good"))
    .WithAudio(audio => audio.Codec("libopus").Bitrate("128k"))
    .Format("webm")
    .Metadata("title=\"Sample Video\"")
    .Option("-hide_banner")
    .Option("-loglevel error")
    .Path("sample.webm");

var args = output.BuildArgs();
// args: ["-i", "input.webm", "-c:v", "libvpx-vp9", "-quality", "good",
//        "-c:a", "libopus", "-b:a", "128k", "-f", "webm",
//        "-metadata", "title=\"Sample Video\"", "-hide_banner",
//        "-loglevel error", "sample.webm"]
```

## Notes

- **Thread Safety**: `OutputFile` is not thread-safe. Concurrent modifications to the same instance (e.g., via `WithVideo` or `WithAudio`) may lead to inconsistent state. Instances should be created and used per-thread or protected by external synchronization.
- **Path Validation**: The `Path` property does not validate the file path format or existence. Invalid paths (e.g., containing illegal characters) may cause FFmpeg to fail at runtime.
- **Option Order**: The order of options in the generated arguments follows the order of method calls. FFmpeg may require specific option ordering for certain operations (e.g., `-y` before the output path).
- **Empty Configuration**: Calling `BuildArgs()` without setting a `Path` or configuring at least one of `Video`/`Audio` may result in an `InvalidOperationException`. Ensure mandatory properties are set.
- **Metadata Escaping**: Metadata values are appended as-is. Users must ensure proper escaping of quotes and special characters to avoid malformed FFmpeg arguments.
