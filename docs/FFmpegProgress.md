# FFmpegProgress

Represents progress information emitted by an FFmpeg process during media conversion or processing.

## API

### `ProcessedTime`
Gets the amount of media processed so far.

- **Type**: `TimeSpan`
- **Remarks**: This value represents the current position in the media timeline. It is always non-negative and increases monotonically during processing.

### `Fps`
Gets the current encoding frame rate, if available.

- **Type**: `double?`
- **Remarks**: The value is `null` if the frame rate cannot be determined. The unit is frames per second.

### `Bitrate`
Gets the current encoding bitrate, if available.

- **Type**: `string?`
- **Remarks**: The value is `null` if the bitrate cannot be determined. The string typically includes a unit (e.g., "1.234M").

### `FrameCount`
Gets the total number of frames processed so far.

- **Type**: `long?`
- **Remarks**: The value is `null` if the frame count is unavailable. This counter increases monotonically during processing.

### `SpeedX`
Gets the current processing speed multiplier relative to real time.

- **Type**: `double?`
- **Remarks**: The value is `null` if the speed cannot be determined. A value of `1.0` indicates real-time processing; values greater than `1.0` indicate faster-than-real-time processing.

### `TryParse`
Attempts to parse a string into an `FFmpegProgress` instance.

- **Signature**: `public static bool TryParse(string input, out FFmpegProgress? progress)`
- **Parameters**:
  - `input`: The string to parse.
  - `progress`: When this method returns, contains the parsed `FFmpegProgress` if successful; otherwise, `null`.
- **Return Value**: `true` if parsing succeeds; otherwise, `false`.
- **Remarks**: The input string must conform to the expected format emitted by FFmpeg’s progress protocol. If parsing fails, `progress` is set to `null` and the method returns `false`.

## Usage

```csharp
// Example 1: Handling progress events during conversion
using var ffmpeg = new FFmpeg();
ffmpeg.OnProgress += (sender, progress) =>
{
    Console.WriteLine($"Processed: {progress.ProcessedTime.TotalSeconds:F2}s");
    if (progress.Fps.HasValue)
    {
        Console.WriteLine($"FPS: {progress.Fps.Value:F2}");
    }
    if (progress.SpeedX.HasValue)
    {
        Console.WriteLine($"Speed: {progress.SpeedX.Value:F2}x");
    }
};
await ffmpeg.ExecuteAsync("-i input.mp4 -c:v libx264 output.mp4");
```

```csharp
// Example 2: Parsing a progress string
var progressText = "out_time_ms=123456789\nfps=23.976\ntotal_size=12345678\n";
if (FFmpegProgress.TryParse(progressText, out var progress))
{
    Console.WriteLine($"Parsed time: {progress.ProcessedTime}");
    Console.WriteLine($"Parsed FPS: {progress.Fps}");
}
```

## Notes

- Thread-safety: Instances of `FFmpegProgress` are immutable after construction. The `TryParse` method is thread-safe.
- Edge cases: If FFmpeg emits malformed progress data, `TryParse` will return `false` and set `progress` to `null`. Numeric fields (`Fps`, `SpeedX`, `FrameCount`) may be `null` if FFmpeg does not provide the corresponding values.
