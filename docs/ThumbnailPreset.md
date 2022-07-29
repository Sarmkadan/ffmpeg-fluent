# ThumbnailPreset

Represents a configuration preset for generating video thumbnails using FFmpeg. Provides a fluent interface for specifying thumbnail extraction parameters such as time position, dimensions, and smart selection strategies.

## API

### `public ThumbnailPreset`

Constructs a new instance of the `ThumbnailPreset` class with default settings.

### `public ThumbnailPreset AtTime(TimeSpan time)`

Specifies the exact time position in the source media where the thumbnail should be extracted.

- **Parameters**:  
  `time` (`TimeSpan`) – The timestamp indicating the frame to capture.  
- **Returns**:  
  The current `ThumbnailPreset` instance to enable method chaining.  
- **Throws**:  
  `ArgumentOutOfRangeException` if `time` is negative or exceeds the media duration.

### `public ThumbnailPreset WithSize(int width, int height)`

Sets the output dimensions for the generated thumbnail image.

- **Parameters**:  
  `width` (`int`) – Target width in pixels. Must be positive.  
  `height` (`int`) – Target height in pixels. Must be positive.  
- **Returns**:  
  The current `ThumbnailPreset` instance to enable method chaining.  
- **Throws**:  
  `ArgumentException` if either dimension is less than or equal to zero.

### `public ThumbnailPreset UseSmartSelect(bool enable = true)`

Enables or disables smart thumbnail selection, which automatically chooses representative frames based on scene changes or visual complexity.

- **Parameters**:  
  `enable` (`bool`) – Whether to activate smart selection logic. Defaults to `true`.  
- **Returns**:  
  The current `ThumbnailPreset` instance to enable method chaining.

### `public string[] BuildArguments()`

Generates an array of command-line arguments suitable for passing to an FFmpeg process based on the current preset configuration.

- **Returns**:  
  A `string[]` containing the constructed FFmpeg arguments.  
- **Throws**:  
  `InvalidOperationException` if required parameters (e.g., input file path) are not set.

### `public async Task RunAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)`

Executes the thumbnail generation process asynchronously using the configured preset settings.

- **Parameters**:  
  `inputPath` (`string`) – Full path to the source media file.  
  `outputPath` (`string`) – Full path where the thumbnail will be saved.  
  `cancellationToken` (`CancellationToken`) – Optional token to cancel the operation.  
- **Returns**:  
  A `Task` representing the asynchronous execution.  
- **Throws**:  
  `ArgumentNullException` if `inputPath` or `outputPath` is null.  
  `FileNotFoundException` if the input file does not exist.  
  `InvalidOperationException` if the FFmpeg process fails or required arguments are missing.

## Usage

```csharp
// Example 1: Generate a thumbnail at a specific time with custom size
var preset = new ThumbnailPreset()
    .AtTime(TimeSpan.FromSeconds(30))
    .WithSize(640, 480);

string[] args = preset.BuildArguments();
// Output: ["-ss", "00:00:30.000", "-i", "<input>", "-vframes", "1", "-vf", "scale=640:480", "<output>"]
```

```csharp
// Example 2: Use smart selection to extract a representative frame
var smartPreset = new ThumbnailPreset()
    .UseSmartSelect()
    .WithSize(320, 240);

await smartPreset.RunAsync("input.mp4", "thumbnail.jpg");
```

## Notes

- `BuildArguments()` must be called after configuring all necessary parameters; otherwise, it may produce incomplete or invalid FFmpeg commands.  
- `RunAsync` performs file I/O and process execution. Concurrent calls with overlapping input/output paths may result in race conditions or file access conflicts.  
- Modifying a `ThumbnailPreset` instance after calling `BuildArguments()` or during `RunAsync()` execution is not thread-safe and may lead to unpredictable behavior.  
- Smart selection (`UseSmartSelect(true)`) requires additional processing time and may not be supported by all FFmpeg builds.
