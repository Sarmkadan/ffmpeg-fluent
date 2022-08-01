# WatermarkHelper

The `WatermarkHelper` type provides a fluent API for constructing FFmpeg filter complexes and argument lists that apply an image or video watermark to a media stream. It enables chainable configuration of the watermark’s position, opacity, and scale, and then exposes methods to retrieve the generated FFmpeg filter complex string, the full argument array, or to execute the FFmpeg process asynchronously.

## API

### WatermarkHelper()
Initializes a new instance of the `WatermarkHelper` class with no watermark settings applied. The instance is ready to receive configuration calls such as `At`, `WithOpacity`, and `WithScale`.

### WatermarkHelper At(double x, double y)
Sets the watermark’s top‑left corner position within the output frame.

- **Parameters**  
  - `x`: Horizontal offset in pixels (or as a proportion of video width if expressed as a fraction).  
  - `y`: Vertical offset in pixels (or as a proportion of video height if expressed as a fraction).

- **Return value**  
  The same `WatermarkHelper` instance to allow further chaining.

- **Exceptions**  
  - `ArgumentException` if either `x` or `y` is `double.NaN`.  
  - `ArgumentOutOfRangeException` if the values are outside the permissible range defined by the underlying FFmpeg filter (typically negative values are allowed for offsetting outside the frame, but extremely large values may be rejected).

### WatermarkHelper WithOpacity(double opacity)
Configures the watermark’s opacity level.

- **Parameters**  
  - `opacity`: A value between `0.0` (fully transparent) and `1.0` (fully opaque).

- **Return value**  
  The same `WatermarkHelper` instance for chaining.

- **Exceptions**  
  - `ArgumentOutOfRangeException` if `opacity` is less than `0.0` or greater than `1.0`.

### WatermarkHelper WithScale(double scale)
Applies a scaling factor to the watermark image or video.

- **Parameters**  
  - `scale`: Multiplicative factor; `1.0` leaves the original size, values `<1.0` shrink, values `>1.0` enlarge.

- **Return value**  
  The same `WatermarkHelper` instance for chaining.

- **Exceptions**  
  - `ArgumentOutOfRangeException` if `scale` is less than or equal to `0.0`.

### string BuildFilterComplex()
Generates the FFmpeg filter‑complex string that encodes the watermark configuration.

- **Parameters**  
  None.

- **Return value**  
  A string suitable for passing to the `-filter_complex` FFmpeg option. If no watermark settings have been specified, returns an empty string.

- **Exceptions**  
  - `InvalidOperationException` if the internal state is inconsistent (e.g., position set but opacity or scale missing, depending on implementation).

### string[] BuildArguments()
Produces the complete FFmpeg argument array that includes the filter complex and any required input/output options for the watermark operation.

- **Parameters**  
  None.

- **Return value**  
  An array of strings where each element corresponds to a single FFmpeg command‑line argument.

- **Exceptions**  
  - `InvalidOperationException` if the argument list cannot be constructed due to missing or invalid configuration.

### Task RunAsync()
Asynchronously executes the FFmpeg process with the arguments produced by `BuildArguments`.

- **Parameters**  
  None.

- **Return value**  
  A `Task` that completes when the FFmpeg process exits. The task result is `void`; any FFmpeg output can be captured via redirected streams if the caller has configured them.

- **Exceptions**  
  - `InvalidOperationException` if `BuildArguments` would fail.  
  - `System.ComponentModel.Win32Exception` if the FFmpeg executable cannot be found or started.  
  - `FFmpegException` (or similar domain‑specific exception) if the FFmpeg process exits with a non‑zero status code, indicating an error during execution.

## Usage

```csharp
using FFmpegFluent;

// Build a watermark positioned at (10,10), half opacity, and 0.5 scale.
var watermark = new WatermarkHelper()
                .At(10, 10)
                .WithOpacity(0.5)
                .WithScale(0.5);

// Obtain the filter complex for manual FFmpeg invocation.
string filter = watermark.BuildFilterComplex();
// filter might look like: "[1:v]scale=iw*0.5:ih*0.5,format=rgba,colorchannelmixer=aa=0.5[wm];[0:v][wm]overlay=10:10"

string[] args = watermark.BuildArguments();
// args can be passed directly to Process.Start("ffmpeg", string.Join(" ", args));
```

```csharp
using System.Threading.Tasks;
using FFmpegFluent;

public async Task EncodeWithWatermarkAsync(string input, string output, string watermarkPath)
{
    // Configure watermark: bottom‑right corner, full opacity, original size.
    var watermark = new WatermarkHelper()
                    .At("main_w-overlay_w-10", "main_h-overlay_h-10") // example using FFmpeg expressions
                    .WithOpacity(1.0)
                    .WithScale(1.0);

    // Run the FFmpeg command asynchronously.
    await watermark.RunAsync()
                   .ConfigureAwait(false);
}
```

## Notes

- The `WatermarkHelper` instance is mutable; each configuration method (`At`, `WithOpacity`, `WithScale`) modifies the internal state and returns the same instance to enable fluent chaining. Consequently, the same instance should not be used concurrently by multiple threads without external synchronization, as simultaneous calls could lead to race conditions on the underlying state.
- Once the configuration is complete, the methods `BuildFilterComplex`, `BuildArguments`, and `RunAsync` are safe to call from multiple threads because they only read the immutable snapshot of the state at the moment of invocation. However, if the instance is mutated after one of these methods has been called, subsequent calls may reflect the updated state.
- Providing values outside the expected ranges (e.g., negative scale, opacity > 1) results in `ArgumentOutOfRangeException` before any FFmpeg process is launched, allowing early validation.
- The `At` method accepts pixel offsets or FFmpeg‑compatible expressions (such as `"main_w-overlay_w-10"`). Supplying an invalid expression will not be caught by the helper; FFmpeg will report an error during execution, causing `RunAsync` to throw a FFmpeg‑related exception.
- If no watermark parameters are set, `BuildFilterComplex` returns an empty string and `BuildArguments` yields an argument list that omits any watermark‑related options; calling `RunAsync` in this state effectively runs FFmpeg without a watermark.
- The helper does not manage the lifetime of the FFmpeg executable; callers must ensure that `ffmpeg` is accessible in the system PATH or provide a fully qualified path via whatever mechanism the surrounding ffmpeg‑fluent library uses. Thread‑guaranteed for the mutable configuration methods modify internal state; read‑only methods are safe after configuration if no further mutation occurs.
