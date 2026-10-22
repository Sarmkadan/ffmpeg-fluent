# VideoOptions

VideoOptions is a fluent builder class for configuring video encoding parameters in FFmpeg operations. It allows method chaining to specify codec, bitrate, quality, and other video stream settings, which are then converted into command-line arguments for FFmpeg processing.

## API

### Codec
Configures the video codec to use for encoding.

**Parameters:**  
`string codec` - The name of the video codec (e.g., "libx264", "libx265").

**Returns:**  
`VideoOptions` - The current instance for method chaining.

**Throws:**  
`ArgumentException` - If the codec name is null, empty, or not recognized by FFmpeg.

---

### Bitrate
Sets the target video bitrate in kilobits per second.

**Parameters:**  
`int bitrate` - The desired bitrate in kbps.

**Returns:**  
`VideoOptions` - The current instance for method chaining.

**Throws:**  
`ArgumentOutOfRangeException` - If the bitrate is less than or equal to zero.

---

### Crf
Configures the Constant Rate Factor (CRF) for quality-based encoding.

**Parameters:**  
`double crf` - The CRF value (typically between 18 and 28 for H.264/H.265).

**Returns:**  
`VideoOptions` - The current instance for method chaining.

**Throws:**  
`ArgumentOutOfRangeException` - If the CRF value is outside 0-63 (the widest range any supported codec uses).

**Validation:**  
The codec-specific range is checked by `Validate()`, not here, because the codec may be set after the CRF: 0-51 for `libx264`/`libx265`, 0-63 for `libvpx-vp9`/`libaom-av1`.

---

### Preset
Sets the encoding speed/compression tradeoff preset.

**Parameters:**  
`string preset` - The preset name (e.g., "ultrafast", "superfast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow").

**Returns:**  
`VideoOptions` - The current instance for method chaining.

**Throws:**  
`ArgumentException` - If the preset is not supported by the selected codec.

---

### FrameRate
Specifies the output video frame rate.

**Parameters:**  
`double frameRate` - The desired frame rate (e.g., 24, 30, 60).

**Returns:**  
`VideoOptions` - The current instance for method chaining.

**Throws:**  
`ArgumentOutOfRangeException` - If the frame rate is less than or equal to zero.

---

### Resolution
Sets the output video resolution.

**Parameters:**  
`string resolution` - The resolution string in "WIDTHxHEIGHT" format (e.g., "1920x1080").

**Returns:**  
`VideoOptions` - The current instance for method chaining.

**Throws:**  
`FormatException` - If the resolution string is not in the correct format.

**Validation:**  
Width and height may be a positive pixel count or `-1`/`-2` to scale that side automatically. Setting both to automatic is rejected by `Validate()`.

---

### Validate / EnsureValid
Checks the configuration without launching FFmpeg. `Validate()` returns every problem found; `EnsureValid()` throws `FFmpegOptionsValidationException` carrying the same list in its `Errors` property. `FFmpegCommand.RunAsync` runs this check for every output before the process starts.

Rules:
- Width/height: positive, or `-1`/`-2`; not both automatic.
- Frame rate: finite and positive (rejects `NaN` and infinity).
- Bitrate: `<number>[k|K|M|G]` with an optional decimal part (e.g. `500k`, `2M`, `1.5M`), and greater than zero. Strings FFmpeg would accept but that are ambiguous (`2m` is milli-bits, `2MB`) are rejected.
- CRF: within the codec range (see `Crf`).
- CRF with a fixed bitrate is rejected for `libx264`/`libx265` and for an unset codec, where the two rate-control modes conflict. It is allowed for `libvpx-vp9`/`libaom-av1` (constrained quality).

**Returns:**  
`IReadOnlyList<string>` from `Validate()` (empty if valid); `void` from `EnsureValid()`.

**Throws:**  
`FFmpegOptionsValidationException` - From `EnsureValid()` and `RunAsync`, when any rule fails.

---

### NoVideo
Disables the video stream in the output.

**Parameters:**  
None.

**Returns:**  
`VideoOptions` - The current instance for method chaining.

**Throws:**  
None.

---

### BuildArgs
Generates the command-line arguments for FFmpeg based on the configured options.

**Parameters:**  
None.

**Returns:**  
`IEnumerable<string>` - A collection of command-line arguments representing the configured video options.

**Throws:**  
`InvalidOperationException` - If conflicting options are set (e.g., both Bitrate and Crf are specified).

## Usage

```csharp
var options = new VideoOptions()
    .Codec("libx264")
    .Bitrate(5000)
    .Preset("fast")
    .FrameRate(30)
    .Resolution("1280x720");

foreach (var arg in options.BuildArgs())
{
    Console.WriteLine(arg);
}
// Output: -c:v libx264 -b:v 5000k -preset fast -r 30 -vf scale=1280:720
```

```csharp
var options = new VideoOptions()
    .Codec("libx265")
    .Crf(23)
    .Preset("medium")
    .Resolution("1920x1080");

var args = options.BuildArgs();
// args contains: -c:v libx265 -crf 23 -preset medium -vf scale=1920:1080
```

## Notes

- The `BuildArgs` method must be called after all configuration methods to generate valid FFmpeg arguments. Calling it prematurely may result in incomplete or invalid output.
- If `NoVideo()` is called, all other video-related settings (Codec, Bitrate, Crf, etc.) are ignored in the generated arguments.
- Thread safety is not guaranteed. Concurrent modification of a `VideoOptions` instance may lead to undefined behavior.
- Conflicting configurations (e.g., setting both `Bitrate` and `Crf`) will cause `BuildArgs` to throw an `InvalidOperationException`.
