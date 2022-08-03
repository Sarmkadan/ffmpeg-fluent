# ConcatPreset

`ConcatPreset` provides a fluent interface for building and executing media concatenation operations using FFmpeg. It abstracts the construction of a concat demuxer input list and the invocation of FFmpeg with appropriate arguments, supporting both stream copy and re-encode workflows.

## API

### `public ConcatPreset`

Creates a new instance of the concatenation preset. The constructor initializes an empty input list and default options. No arguments are required.

### `public ConcatPreset AddInput`

Registers a media file to be included in the concatenation sequence. The method appends the specified file path to the internal input list and returns the same `ConcatPreset` instance for chaining.

- **Parameters:** A string representing the absolute or relative path to a media file.
- **Returns:** The current `ConcatPreset` instance (fluent design).
- **Throws:** `ArgumentException` if the provided path is null or empty. `FileNotFoundException` if the file does not exist on disk at the time of the call.

### `public ConcatPreset WithReencode`

Configures the concatenation to re-encode streams rather than using stream copy mode. When invoked, the resulting FFmpeg command will decode and re-encode all streams, which is necessary when input files have differing codecs, resolutions, or other stream parameters that would make stream copy invalid.

- **Parameters:** None.
- **Returns:** The current `ConcatPreset` instance (fluent design).
- **Throws:** Nothing.

### `public string BuildConcatListContent`

Generates the content of a temporary concat demuxer playlist file. The output is a newline-separated list of `file '...'` entries corresponding to each input added via `AddInput`. This string is intended to be written to a temporary file and passed to FFmpeg via the `-f concat` option.

- **Parameters:** None.
- **Returns:** A string containing the formatted concat demuxer playlist.
- **Throws:** `InvalidOperationException` if no inputs have been added.

### `public async Task RunAsync`

Executes the concatenation operation asynchronously. The method writes the concat list to a temporary file, constructs the FFmpeg arguments (including `-f concat` and either `-c copy` or re-encode flags depending on `WithReencode`), and runs the FFmpeg process. The output file path must have been specified through the underlying base mechanism before calling this method.

- **Parameters:** None.
- **Returns:** A `Task` that completes when the FFmpeg process exits successfully.
- **Throws:** `InvalidOperationException` if no inputs have been added or no output file has been configured. `FFmpegException` if the underlying FFmpeg process returns a non-zero exit code.

## Usage

### Example 1: Simple Stream Copy Concatenation

```csharp
var concat = new ConcatPreset();
concat
    .AddInput("part1.mp4")
    .AddInput("part2.mp4")
    .AddInput("part3.mp4");

// Output path is set via the underlying fluent infrastructure
concat.Output.ToFile("joined.mp4");

await concat.RunAsync();
```

This produces a lossless concatenation when all input files share identical codecs and stream parameters.

### Example 2: Re-encode Concatenation with Mixed Formats

```csharp
var concat = new ConcatPreset();
concat
    .AddInput("intro.mkv")
    .AddInput("main.mp4")
    .AddInput("outro.webm")
    .WithReencode();

concat.Output.ToFile("complete.mp4");

await concat.RunAsync();
```

Re-encoding ensures compatibility when inputs have heterogeneous codecs, resolutions, or pixel formats.

## Notes

- **Input Order:** Files are concatenated in the exact order they are added via `AddInput`. The order of entries in the generated concat list matches the call sequence.
- **Stream Copy Limitations:** Omitting `WithReencode` uses `-c copy`, which requires all input files to have matching codecs, stream counts, and stream parameters. Mismatched inputs will produce corrupted output or cause FFmpeg to fail.
- **Temporary Files:** `RunAsync` creates a temporary concat list file in the system's temp directory. The file is deleted after the FFmpeg process completes, regardless of success or failure.
- **Thread Safety:** Instance members are not thread-safe. A `ConcatPreset` instance should not be mutated concurrently from multiple threads. Each invocation of `RunAsync` should operate on its own instance or be externally synchronized.
- **Output Configuration:** The output file path is not a direct member of `ConcatPreset` but is set through an inherited or composed output builder. `RunAsync` throws if this has not been configured prior to execution.
- **File Existence Checks:** `AddInput` validates file existence eagerly. Files that are created or become available after the call will not be recognized unless re-added.
