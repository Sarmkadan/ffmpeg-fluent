# NormalizeAudioPreset

`NormalizeAudioPreset` is a configuration class for defining audio normalization settings in FFmpeg operations, enabling control over loudness targets, peak levels, and metadata output through a fluent interface.

## API

### `NormalizeAudioPreset()`
Initializes a new instance of the `NormalizeAudioPreset` class with default normalization parameters.

### `WithTargetIntegrated(double target)`
Configures the target integrated loudness in LUFS (Loudness Units Full Scale).  
**Parameters:**  
- `target` (`double`): The desired integrated loudness level.  
**Returns:**  
- `NormalizeAudioPreset`: The current instance for method chaining.  

### `WithTargetTruePeak(double peak)`
Sets the maximum true peak level in dBTP (dB True Peak).  
**Parameters:**  
- `peak` (`double`): The target true peak threshold.  
**Returns:**  
- `NormalizeAudioPreset`: The current instance for method chaining.  

### `WithTargetLra(double lra)`
Defines the target loudness range (LRA) in LU (Loudness Units).  
**Parameters:**  
- `lra` (`double`): The desired LRA value.  
**Returns:**  
- `NormalizeAudioPreset`: The current instance for method chaining.  

### `WithNormalize(bool enable)`
Enables or disables audio normalization processing.  
**Parameters:**  
- `enable` (`bool`): `true` to apply normalization; `false` to skip.  
**Returns:**  
- `NormalizeAudioPreset`: The current instance for method chaining.  

### `WithPrintMetadata(bool print)`
Controls whether normalization metadata is printed to standard output.  
**Parameters:**  
- `print` (`bool`): `true` to enable metadata printing; `false` to suppress.  
**Returns:**  
- `NormalizeAudioPreset`: The current instance for method chaining.  

### `WithMeasurementInput(string[] inputs)`
Specifies input files or streams used exclusively for loudness measurement (not processed).  
**Parameters:**  
- `inputs` (`string[]`): Array of input file paths or stream identifiers.  
**Returns:**  
- `NormalizeAudioPreset`: The current instance for method chaining.  

### `BuildArguments()`
Generates the FFmpeg command-line arguments for audio normalization based on configured settings.  
**Returns:**  
- `string[]`: An array of command-line arguments.  
**Throws:**  
- `InvalidOperationException`: If required parameters (e.g., `WithTargetIntegrated`) are not set.  

### `RunAsync(CancellationToken cancellationToken = default)`
Executes the audio normalization process asynchronously using the configured settings.  
**Parameters:**  
- `cancellationToken` (`CancellationToken`): Optional token to cancel execution.  
**Returns:**  
- `Task`: A task representing the asynchronous operation.  
**Throws:**  
- `FFmpegException`: On FFmpeg process failure or invalid configuration.  
- `OperationCanceledException`: If cancellation is requested.  

### `ToString()`
Returns a string representation of the current configuration.  
**Returns:**  
- `string`: A formatted string of configured parameters.  

## Usage

```csharp
var preset = new NormalizeAudioPreset()
    .WithTargetIntegrated(-16.0)
    .WithTargetTruePeak(-1.0)
    .WithTargetLra(11.0)
    .WithNormalize(true)
    .WithPrintMetadata(true);

await preset.RunAsync("input.mp4", "output.mp4");
```

```csharp
var preset = new NormalizeAudioPreset()
    .WithTargetIntegrated(-23.0)
    .WithNormalize(true);

string[] args = preset.BuildArguments();
// Use args with custom FFmpeg invocation logic
```

## Notes

- `BuildArguments()` requires at least `WithTargetIntegrated` to be called; otherwise, it throws `InvalidOperationException`.  
- Instances of `NormalizeAudioPreset` are mutable and not thread-safe. Concurrent modification during `RunAsync()` execution may lead to undefined behavior.  
- `WithMeasurementInput` allows decoupling measurement inputs from processing inputs, useful for analyzing reference tracks before applying normalization.
