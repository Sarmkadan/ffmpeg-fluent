# GifPreset

A fluent builder that constructs ffmpeg command-line arguments for converting a video segment into an animated GIF. `GifPreset` encapsulates common GIF‑optimization parameters (frame rate, width, time range) and exposes a method to execute the conversion asynchronously. Instances are mutable and intended for single‑use argument construction; after calling `BuildArguments` or `RunAsync` the preset should not be reused.

## API

### `public GifPreset()`

Initializes a new instance with default settings (no constraints on frame rate, width, or time range). The default palette generation and dithering are applied by ffmpeg.

### `public GifPreset WithFps(double fps)`

Sets the output frame rate.  

- **Parameters**  
  `fps` – Frames per second. Must be greater than zero.  
- **Returns**  
  The same `GifPreset` instance for chaining.  
- **Throws**  
  `ArgumentOutOfRangeException` if `fps` is not a positive finite number.

### `public GifPreset WithWidth(int width)`

Sets the output width in pixels; height is scaled proportionally.  

- **Parameters**  
  `width` – Target width in pixels. Must be greater than zero.  
- **Returns**  
  The same `GifPreset` instance for chaining.  
- **Throws**  
  `ArgumentOutOfRangeException` if `width` is less than or equal to zero.

### `public GifPreset WithRange(TimeSpan start, TimeSpan duration)`

Defines a time segment of the input video to convert.  

- **Parameters**  
  `start` – Start time offset from the beginning of the video.  
  `duration` – Length of the segment to include.  
- **Returns**  
  The same `GifPreset` instance for chaining.  
- **Throws**  
  `ArgumentOutOfRangeException` if `start` or `duration` is negative, or if `duration` is zero.

### `public string[] BuildArguments()`

Returns the complete set of command‑line arguments that would be passed to ffmpeg for the current preset configuration. The array includes the input file placeholder (typically `"-i"` followed by an input path) – the caller must supply the actual input path when executing ffmpeg.  

- **Returns**  
  A `string[]` containing ffmpeg arguments.  
- **Throws**  
  `InvalidOperationException` if required parameters (e.g., input file) have not been set through other means (the preset itself does not store an input path; this exception may be raised if the builder detects an incomplete configuration).

### `public async Task RunAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)`

Executes the ffmpeg conversion synchronously on the calling thread (the method is `async` but the underlying process is started and awaited).  

- **Parameters**  
  `inputPath` – Path to the source video file.  
  `outputPath` – Path where the resulting GIF will be written.  
  `cancellationToken` – Optional token to cancel the operation.  
- **Returns**  
  A `Task` that completes when the ffmpeg process exits.  
- **Throws**  
  `FileNotFoundException` if `inputPath` does not exist.  
  `InvalidOperationException` if the preset configuration is invalid (e.g., missing required parameters).  
  `OperationCanceledException` if cancellation is requested.  
  `IOException` or `Win32Exception` if the ffmpeg executable cannot be found or started.

## Usage

### Basic conversion with default settings

```csharp
var preset = new GifPreset();
await preset.RunAsync("input.mp4", "output.gif");
```

### Custom frame rate, width, and time segment

```csharp
var preset = new GifPreset()
    .WithFps(15)
    .WithWidth(480)
    .WithRange(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));

string[] args = preset.BuildArguments();
// args can be inspected or passed to a custom ffmpeg runner
await preset.RunAsync("input.mp4", "output.gif");
```

## Notes

- **Thread safety**: `GifPreset` is not thread‑safe. Concurrent calls to `WithFps`, `WithWidth`, `WithRange`, `BuildArguments`, or `RunAsync` on the same instance will produce undefined behavior. Each thread should use its own instance.
- **Argument ordering**: The order in which fluent methods are called does not affect the final argument sequence; the builder internally arranges parameters correctly.
- **Default values**: If `WithFps` or `WithWidth` are not called, ffmpeg uses the source video’s native frame rate and width. If `WithRange` is omitted, the entire video is converted.
- **Input path**: `BuildArguments` does not include the input file path; the caller must prepend `"-i"` and the path when invoking ffmpeg directly. `RunAsync` handles this automatically.
- **Cancellation**: `RunAsync` respects the cancellation token only between process start and exit; it does not interrupt ffmpeg mid‑frame. For immediate termination, consider using `Process.Kill` as a fallback.
