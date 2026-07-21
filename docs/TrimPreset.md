# TrimPreset

The `TrimPreset` type provides a fluent API for defining trim operations (start time, end time or duration, and encoding mode) that can be turned into an executable FFmpeg command. It enables concise, readable configuration of video or audio trimming before execution.

## API

### TrimPreset()
- **Purpose:** Creates a new, empty trim preset.
- **Parameters:** None.
- **Return:** A new `TrimPreset` instance.
- **Throws:** Does not throw under normal circumstances.

### TrimPreset From(TimeSpan start)
- **Purpose:** Sets the start timestamp for the trim operation.
- **Parameters:** `start` – The time offset from the beginning of the input where trimming should begin.
- **Return:** The same `TrimPreset` instance to allow method chaining.
- **Throws:** `ArgumentException` if `start` is negative.

### TrimPreset To(TimeSpan end)
- **Purpose:** Sets the end timestamp for the trim operation.
- **Parameters:** `end` – The time offset from the beginning of the input where trimming should stop.
- **Return:** The same `TrimPreset` instance to allow method chaining.
- **Throws:** `ArgumentException` if `end` is negative or less than the previously set start time.

### TrimPreset Duration(TimeSpan duration)
- **Purpose:** Sets the duration of the trim operation, interpreted relative to the start time.
- **Parameters:** `duration` – The length of the segment to keep after the start point.
- **Return:** The same `TrimPreset` instance to allow method chaining.
- **Throws:** `ArgumentException` if `duration` is zero or negative.

### TrimPreset StreamCopy()
- **Purpose:** Configures the trim to use stream copy mode, avoiding re‑encoding.
- **Parameters:** None.
- **Return:** The same `TrimPreset` instance to allow method chaining.
- **Throws:** Does not throw; calling this after `WithReencode()` simply overrides the previous mode.

### TrimPreset WithReencode()
- **Purpose:** Configures the trim to re‑encode the selected segment using the default codec settings.
- **Parameters:** None.
- **Return:** The same `TrimPreset` instance to allow method chaining.
- **Throws:** Does not throw; calling this after `StreamCopy()` simply overrides the previous mode.

### FFmpegCommand Build()
- **Purpose:** Materializes the FFmpeg command based on the current trim settings.
- **Parameters:** None.
- **Return:** An `FFmpegCommand` object that can be further customized or executed.
- **Throws:** `InvalidOperationException` if neither a start time nor a stop condition (end time or duration) has been specified, or if both an end time and a duration are set ambiguously.

### string[] BuildArguments()
- **Purpose:** Provides the raw argument array that would be sent to FFmpeg for the configured trim.
- **Parameters:** None.
- **Return:** A `string[]` containing the FFmpeg arguments.
- **Throws:** Same conditions as `Build()` – throws `InvalidOperationException` when the configuration is insufficient or contradictory.

### void Run()
- **Purpose:** Executes the FFmpeg process with the arguments generated from the current configuration and waits for it to exit.
- **Parameters:** None.
- **Return:** None.
- **Throws:** `FFmpegException` if the FFmpeg process fails to start or returns a non‑zero exit code; `InvalidOperationException` if the configuration is invalid (same checks as `Build()`).

## Usage

```csharp
using FluentFFmpeg;

// Example 1: Trim a video from 5s to 15s using stream copy (no re‑encode).
var cmd = new TrimPreset()
    .From(TimeSpan.FromSeconds(5))
    .To(TimeSpan.FromSeconds(15))
    .StreamCopy()
    .Build();

cmd.Run(); // executes ffmpeg -ss 00:00:05 -to 00:00:15 -c copy …
```

```csharp
using FluentFFmpeg;

// Example 2: Trim a 10‑second segment starting at 2s with re‑encode.
var args = new TrimPreset()
    .From(TimeSpan.FromSeconds(2))
    .Duration(TimeSpan.FromSeconds(10))
    .WithReencode()
    .BuildArguments();

// args can be passed to a custom FFmpeg launcher or inspected.
// Example execution:
new TrimPreset()
    .From(TimeSpan.FromSeconds(2))
    .Duration(TimeSpan.FromSeconds(10))
    .WithReencode()
    .Run(); // runs ffmpeg -ss 00:00:02 -t 00:00:10 …
```

## Notes

- Specifying both an end time (`To`) and a duration (`Duration`) is allowed; the later call overrides the earlier one. To avoid ambiguity, set only one of these after the start time.
- Negative values for start, end, or duration are invalid and will cause an `ArgumentException`.
- `StreamCopy()` and `WithReencode()` are mutually exclusive; the last method called determines the encoding mode.
- The `TrimPreset` instance is **not** thread‑safe. Concurrent calls to any of its configuration methods from multiple threads may result in undefined behavior. Configure the instance on a single thread before invoking `Build()`, `BuildArguments()`, or `Run()`.
- `Build()`, `BuildArguments()`, and `Run()` perform validation only when invoked; they do not throw during the configuration phase. Ensure all required parameters are set before calling these methods to prevent `InvalidOperationException` or `FFmpegException` at execution time.
