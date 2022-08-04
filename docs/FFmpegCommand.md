# FFmpegCommand

The `FFmpegCommand` class provides a fluent interface for constructing FFmpeg command lines. It allows users to add inputs, outputs, filter graphs, and global options in a readable, chainable manner, then either generate the final command line string or execute the FFmpeg process asynchronously.

## API

### Create
- **Signature**: `public static FFmpegCommand Create()`
- **Purpose**: Returns a new, empty `FFmpegCommand` instance ready for configuration.
- **Parameters**: None.
- **Return value**: A fresh `FFmpegCommand` object.
- **Exceptions**: None.

### AddInput
- **Signature**: `public FFmpegCommand AddInput(string inputFile, params string[] arguments)`
- **Purpose**: Adds an input source (file, URL, device, etc.) together with any FFmpeg input‑specific options.
- **Parameters**:
  - `inputFile`: Path or specifier for the input. Must not be `null`.
  - `arguments`: Zero or more options that apply to this input (e.g., `-ss`, `-t`, `-f`).
- **Return value**: The same `FFmpegCommand` instance to allow chaining.
- **Exceptions**:
  - `ArgumentNullException` if `inputFile` is `null`.
  - `ArgumentException` if `inputFile` is empty or consists only of whitespace.

### WithFilterGraph
- **Signature**: `public FFmpegCommand WithFilterGraph(string filterGraph)`
- **Purpose**: Specifies a complex filtergraph to be applied via the `-filter_complex` option.
- **Parameters**:
  - `filterGraph`: A string containing the filtergraph description. Must not be `null`.
- **Return value**: The same `FFmpegCommand` instance.
- **Exceptions**:
  - `ArgumentNullException` if `filterGraph` is `null`.
  - `ArgumentException` if the string is empty.

### AddOutput
- **Signature**: `public FFmpegCommand AddOutput(string outputFile, params string[] arguments)`
- **Purpose**: Adds an output destination (file, URL, device, etc.) together with any FFmpeg output‑specific options.
- **Parameters**:
  - `outputFile`: Path or specifier for the output. Must not be `null`.
  - `arguments`: Zero or more options that apply to this output (e.g., `-c:v`, `-b:a`, `-map`).
- **Return value**: The same `FFmpegCommand` instance.
- **Exceptions**:
  - `ArgumentNullException` if `outputFile` is `null`.
  - `ArgumentException` if `outputFile` is empty or consists only of whitespace.

### GlobalOption
- **Signature**: `public FFmpegCommand GlobalOption(string option, params string[] arguments)`
- **Purpose**: Adds a global FFmpeg option that affects the entire command (e.g., `-y`, `-loglevel`, `-hide_banner`).
- **Parameters**:
  - `option`: The option flag including the leading dash. Must not be `null`.
  - `arguments`: Optional values associated with the option.
- **Return value**: The same `FFmpegCommand` instance.
- **Exceptions**:
  - `ArgumentNull associated with the option.
- **Return value**: The same `FFmpegCommand` instance.
- **Exceptions**:
  - `ArgumentNullException` if `option` is `null`.
  - `ArgumentException` if `option` is empty.

### BuildCommandLine
- **Signature**: `public string BuildCommandLine()`
- **Purpose**: Generates the full FFmpeg command line string based on the current configuration.
- **Parameters**: None.
- **Return value**: A string beginning with `ffmpeg` followed by all collected options, inputs, filter graphs, and outputs, properly formatted for execution.
- **Exceptions**:
  - `InvalidOperationException` if no inputs or no outputs have been added before calling this method.

### RunAsync
- **Signature**: `public async Task<int> RunAsync(CancellationToken cancellationToken = default)`
- **Purpose**: Asynchronously runs the FFmpeg process using the command line produced by `BuildCommandLine` and returns the process exit code.
- **Parameters**:
  - `cancellationToken`: Optional token to cancel the operation.
- **Return value**: A `Task<int>` that completes with the exit code of the FFmpeg process (`0` indicates success).
- **Exceptions**:
  - `OperationCanceledException` if the cancellation token is triggered.
  - `InvalidOperationException` if the command line cannot be built (e.g., missing inputs/outputs).
  - Any exception thrown by `System.Diagnostics.Process` (e.g., `Win32Exception` if the `ffmpeg` executable cannot be found).

## Usage

### Simple transcoding
```csharp
using System.Threading.Tasks;
using FFmpegFluent; // adjust namespace as needed

var command = FFmpegCommand.Create()
    .AddInput("input.mp4", "-ss", "00:00:05")
    .AddOutput("output.mp4", "-c:v", "libx264", "-c:a", "aac")
    .GlobalOption("-y");

int exitCode = await command.RunAsync();
// exitCode == 0 indicates successful encoding
```

### Complex filtergraph
```csharp
using System.Threading.Tasks;
using FFmpegFluent;

var command = FFmpegCommand.Create()
    .AddInput("left.mp4")
    .AddInput("right.mp4")
    .WithFilterGraph("[0:v][1:v]hstack=inputs=2[v]")
    .AddOutput("stacked.mp4", "-map", "[v]", "-c:v", "libx264", "-pix_fmt", "yuv420p")
    .GlobalOption("-hide_banner");

int exitCode = await command.RunAsync();
```

## Notes
- The `FFmpegCommand` instance is mutable; each configuration method returns the same instance to enable fluent chaining. Reusing an instance after `RunAsync` may cause duplicated options if further configuration methods are invoked.
- The class is **not thread‑safe**. Concurrent calls to configuration methods or to `RunAsync` on the same instance from multiple threads can result in malformed command lines or exceptions. For parallel executions, create separate instances via `Create`.
- `BuildCommandLine` does not automatically quote arguments that contain spaces. Callers should ensure that file paths either lack spaces or are manually quoted before being passed to the methods.
- Global options added via `GlobalOption` are placed before any input options, matching typical FFmpeg command line ordering.
- If no inputs or no outputs have been added, both `BuildCommandLine` and `RunAsync` throw an `InvalidOperationException`.
- The `RunAsync` method disposes of the started `System.Diagnostics.Process` automatically; callers do not need to manage process handles.
- Cancellation is cooperative: if the supplied `CancellationToken` is triggered, the FFmpeg process is killed and an `OperationCanceledException` is propagated.
