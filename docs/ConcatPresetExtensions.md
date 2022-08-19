# ConcatPresetExtensions

Extension methods for configuring and executing FFmpeg concat operations with preset-based re-encoding options. These methods simplify the construction of complex concat pipelines by chaining preset configurations onto a base `ConcatPreset` instance.

## API

### `AddInputs(params InputFile[] inputs)`
Appends one or more input files to the concat operation. These files will be concatenated in the order they are provided.

- **Parameters**
  - `inputs`: Variable array of `InputFile` instances representing the media files to concatenate.
- **Return Value**
  Returns the modified `ConcatPreset` instance to allow method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `inputs` is `null` or contains `null` elements.

---

### `AddInputsFromDirectory(string directoryPath, string searchPattern = "*.*")`
Adds all files matching a search pattern from a specified directory as inputs to the concat operation.

- **Parameters**
  - `directoryPath`: Absolute or relative path to the directory containing input files.
  - `searchPattern`: Optional search pattern (e.g., `"*.mp4"`). Defaults to `"*.*"`.
- **Return Value**
  Returns the modified `ConcatPreset` instance.
- **Exceptions**
  Throws `ArgumentNullException` if `directoryPath` is `null`.
  Throws `DirectoryNotFoundException` if `directoryPath` does not exist.
  Throws `UnauthorizedAccessException` if access to the directory is denied.

---

### `WithNvencReencode(VideoQualityPreset quality = VideoQualityPreset.Medium)`
Configures the concat operation to re-encode video using NVIDIA NVENC hardware acceleration with a specified quality preset.

- **Parameters**
  - `quality`: Optional `VideoQualityPreset` value. Defaults to `VideoQualityPreset.Medium`.
- **Return Value**
  Returns the modified `ConcatPreset` instance.
- **Exceptions**
  Throws `NotSupportedException` if NVENC hardware acceleration is unavailable on the system.

---

### `WithQsvReencode(VideoQualityPreset quality = VideoQualityPreset.Medium)`
Configures the concat operation to re-encode video using Intel Quick Sync Video (QSV) hardware acceleration with a specified quality preset.

- **Parameters**
  - `quality`: Optional `VideoQualityPreset` value. Defaults to `VideoQualityPreset.Medium`.
- **Return Value**
  Returns the modified `ConcatPreset` instance.
- **Exceptions**
  Throws `NotSupportedException` if QSV hardware acceleration is unavailable on the system.

---
### `WithVideoQualityPreset(VideoQualityPreset quality)`
Sets a global video quality preset for the concat operation. This affects re-encoding quality when combined with hardware or software encoders.

- **Parameters**
  - `quality`: A `VideoQualityPreset` value (e.g., `High`, `Medium`, `Low`).
- **Return Value**
  Returns the modified `ConcatPreset` instance.
- **Exceptions**
  Throws `ArgumentOutOfRangeException` if `quality` is not a defined enum value.

---
### `WithLeadInDelay(TimeSpan delay)`
Inserts a fixed delay before the first frame of the concatenated output. Useful for syncing audio or avoiding glitches.

- **Parameters**
  - `delay`: The duration to delay playback.
- **Return Value**
  Returns the modified `ConcatPreset` instance.
- **Exceptions**
  Throws `ArgumentOutOfRangeException` if `delay` is negative.

---
### `async Task<string> RunAndGetOutputPathAsync()`
Executes the configured concat operation and returns the filesystem path to the generated output file.

- **Return Value**
  A `Task<string>` resolving to the absolute path of the output file upon completion.
- **Exceptions**
  Throws `InvalidOperationException` if no inputs have been added.
  Throws `FFmpegException` if FFmpeg execution fails (e.g., invalid codec, missing input).
  Throws `OperationCanceledException` if the operation is canceled via the configured `CancellationToken`.

## Usage

### Example 1: Concatenate two MP4 files with NVENC re-encoding
