# ThumbnailPresetExtensions

Provides extension methods and static factory members for the `ThumbnailPreset` type, enabling common thumbnail generation configurations and asynchronous execution of thumbnail capture pipelines within the ffmpeg-fluent library.

## API

### `ThumbnailPreset AtMidpoint`

A static preset that configures thumbnail capture to seek to the exact midpoint of the input media's duration. The resulting thumbnail is taken from the frame nearest to the 50% timestamp.

**Purpose:** Quickly obtain a representative frame from the center of a video without manually calculating the seek position.

**Return value:** A `ThumbnailPreset` instance with its seek point set to the media midpoint.

**Throws:** No direct exceptions during property access. Errors related to invalid duration or seek failures surface during pipeline execution.

---

### `ThumbnailPreset WithSquareSize`

A static preset that forces the output thumbnail to have equal width and height. The implementation typically crops the source frame to a centered square region before scaling.

**Purpose:** Produce square thumbnails suitable for grid layouts, avatar images, or platforms requiring 1:1 aspect ratios.

**Return value:** A `ThumbnailPreset` instance configured to output a square image.

**Throws:** No direct exceptions during property access. Execution may fail if the source frame dimensions are incompatible with the requested square size (e.g., when combined with an explicit size constraint that conflicts).

---

### `ThumbnailPreset UseSmartSelectIf`

A static factory method that conditionally applies a "smart select" strategy for thumbnail picking. The method accepts a predicate or condition that determines whether intelligent frame selection (e.g., scene-change detection, sharpness analysis) should be used instead of a fixed timestamp.

**Purpose:** Enable adaptive thumbnail selection based on runtime conditions, falling back to a standard seek when the condition is not met.

**Parameters:**
- `condition` (`bool` or a delegate returning `bool`): Determines whether smart selection is active.

**Return value:** A `ThumbnailPreset` instance with smart selection enabled or disabled according to the condition.

**Throws:** No direct exceptions during the factory call. If a delegate parameter is provided and throws during evaluation, that exception propagates at execution time.

---

### `Task RunAndWaitAsync`

An extension method that asynchronously executes the configured `ThumbnailPreset` pipeline, waits for completion, and returns the resulting thumbnail data or file path.

**Purpose:** Provide a straightforward async execution path for thumbnail generation without manually managing process lifecycle or output streams.

**Parameters:**
- `preset` (`this ThumbnailPreset`): The preset instance to execute.
- `cancellationToken` (`CancellationToken`, optional): Token to cancel the ongoing thumbnail operation.

**Return value:** A `Task` that, when awaited, yields the thumbnail result (typically a `ThumbnailResult` containing file path, stream, or metadata).

**Throws:**
- `OperationCanceledException` if the cancellation token is signaled.
- `InvalidOperationException` if the preset is in an invalid state (e.g., no input file specified).
- `FFmpegException` or derived types when the underlying ffmpeg process exits with an error.

## Usage

### Example 1: Midpoint Square Thumbnail

```csharp
using FFmpegFluent;

// Capture a square thumbnail from the video's midpoint
var preset = ThumbnailPresetExtensions.AtMidpoint
    .WithSquareSize
    .WithSize(320, 320)
    .FromInput("input.mp4")
    .ToOutput("thumbnail.jpg");

ThumbnailResult result = await ThumbnailPresetExtensions.RunAndWaitAsync(preset);
Console.WriteLine($"Thumbnail saved to: {result.OutputPath}");
```

### Example 2: Conditional Smart Selection

```csharp
using FFmpegFluent;

bool useSmartSelect = VideoDurationExceeds("input.mp4", TimeSpan.FromMinutes(10));

var preset = ThumbnailPresetExtensions.UseSmartSelectIf(useSmartSelect)
    .WithSize(640, 360)
    .FromInput("input.mp4")
    .ToOutput("smart_thumb.png");

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
ThumbnailResult result = await ThumbnailPresetExtensions.RunAndWaitAsync(preset, cts.Token);

Console.WriteLine($"Thumbnail generated: {result.OutputPath}");

static bool VideoDurationExceeds(string path, TimeSpan threshold)
{
    // Implementation that probes media duration
    return true;
}
```

## Notes

- **Preset immutability:** Each static member and factory method returns a new `ThumbnailPreset` instance. Presets are designed to be composed fluently without mutating shared state, making them safe for concurrent configuration across multiple threads.
- **Execution thread-safety:** `RunAndWaitAsync` is not thread-safe with respect to the same preset instance being executed simultaneously from multiple callers. Each invocation should operate on its own preset instance or be externally synchronized if reuse is intended.
- **Edge cases with `AtMidpoint`:** If the input media has a duration of zero or is a live stream with no defined duration, midpoint calculation may fail at execution time. Validate input duration before using this preset in such scenarios.
- **`WithSquareSize` cropping behavior:** The square crop is centered by default. If the source frame is already square, no cropping occurs. When combined with explicit width/height constraints that are not equal, the preset may throw during pipeline building or execution due to conflicting size requirements.
- **`UseSmartSelectIf` delegate evaluation:** When a delegate is supplied, it is evaluated at execution time, not at preset construction time. Ensure the delegate captures only thread-safe state if the preset is built on one thread and executed on another.
- **Cancellation handling:** `RunAndWaitAsync` cancels the underlying ffmpeg process. Temporary files created during a canceled operation may remain on disk; the caller is responsible for cleanup if the preset does not configure automatic cleanup.
