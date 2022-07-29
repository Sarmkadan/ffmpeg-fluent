# ExtractAudioPreset

`ExtractAudioPreset` is a configuration class used to define parameters for extracting audio streams from media files using FFmpeg. It provides a fluent interface for specifying codec, bitrate, stream selection, and other options, and generates the necessary command-line arguments for the FFmpeg process.

## API

### `ExtractAudioPreset()`
Initializes a new instance of the `ExtractAudioPreset` class with default settings.

---

### `ExtractAudioPreset WithCodec(string codec)`
Configures the audio codec to use during extraction.

**Parameters**
- `codec` (string): The name of the audio codec (e.g., "aac", "mp3", "flac").

**Returns**
- `ExtractAudioPreset`: The current instance for method chaining.

**Throws**
- `ArgumentNullException`: If `codec` is null.

---

### `ExtractAudioPreset WithBitrate(int bitrate)`
Sets the audio bitrate for the output stream.

**Parameters**
- `bitrate` (int): The target bitrate in kilobits per second (e.g., 128, 256).

**Returns**
- `ExtractAudioPreset`: The current instance for method chaining.

**Throws**
- `ArgumentOutOfRangeException`: If `bitrate` is less than or equal to zero.

---

### `ExtractAudioPreset CopyStream()`
Configures the preset to copy the audio stream without re-encoding. This is useful for fast extraction when the source codec is compatible with the desired output format.

**Returns**
- `ExtractAudioPreset`: The current instance for method chaining.

---

### `ExtractAudioPreset StreamIndex(int index)`
Specifies the index of the audio stream to extract from the input file.

**Parameters**
- `index` (int): The zero-based index of the audio stream.

**Returns**
- `ExtractAudioPreset`: The current instance for method chaining.

**Throws**
- `ArgumentOutOfRangeException`: If `index` is negative.

---

### `string[] BuildArguments()`
Generates the FFmpeg command-line arguments based on the configured preset.

**Returns**
- `string[]`: An array of command-line arguments suitable for passing to FFmpeg.

---

### `async Task RunAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)`
Executes the audio extraction process asynchronously.

**Parameters**
- `inputPath` (string): The path to the input media file.
- `outputPath` (string): The path where the extracted audio will be saved.
- `cancellationToken` (CancellationToken, optional): A token to cancel the operation.

**Returns**
- `Task`: A task representing the asynchronous operation.

**Throws**
- `FileNotFoundException`: If the input file does not exist.
- `InvalidOperationException`: If required arguments are missing or invalid.
- `OperationCanceledException`: If the operation is canceled via the cancellation token.

## Usage

### Example 1: Extract with specific codec and bitrate
```csharp
var preset = new ExtractAudioPreset()
    .WithCodec("aac")
    .WithBitrate(192)
    .StreamIndex(0);

await preset.RunAsync("input.mp4", "output.m4a");
```

### Example 2: Copy stream without re-encoding
```csharp
var preset = new ExtractAudioPreset()
    .CopyStream()
    .StreamIndex(1);

await preset.RunAsync("input.mkv", "output.ogg");
```

## Notes

- `BuildArguments()` must be called after all configuration methods to ensure the correct arguments are generated. Modifying the preset after calling `BuildArguments()` may result in inconsistent state.
- `RunAsync` is not thread-safe. Concurrent calls on the same instance may lead to race conditions or unexpected behavior.
- Using `CopyStream()` bypasses re-encoding but requires the output container format to support the source codec. Incompatible combinations will cause FFmpeg to fail.
- The `StreamIndex` method defaults to 0 if not explicitly called. If the input file has no audio streams at the specified index, FFmpeg will throw an error during execution.
