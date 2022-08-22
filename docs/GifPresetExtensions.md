# GifPresetExtensions

The `GifPresetExtensions` class provides a set of static extension methods and helpers designed to configure and execute `GifPreset` instances within the `ffmpeg-fluent` library. It simplifies the process of defining common GIF encoding parameters, such as resolution constraints and looping behavior, while offering an asynchronous execution path to generate the underlying FFmpeg command arguments.

## API

### `WithDefaultSettings`
```csharp
public static GifPreset WithDefaultSettings(this GifPreset preset)
```
Applies a standard set of encoding parameters to the specified `GifPreset` instance, typically optimizing for a balance between file size and visual quality suitable for general web usage.
*   **Parameters**: `preset` - The source `GifPreset` to configure.
*   **Returns**: The same `GifPreset` instance with default settings applied, allowing for method chaining.
*   **Throws**: `ArgumentNullException` if `preset` is null.

### `WithLoopingRange`
```csharp
public static GifPreset WithLoopingRange(this GifPreset preset, int startFrame, int endFrame)
```
Configures the preset to encode only a specific subset of frames from the source video or image sequence, defining the start and end points for the animation loop.
*   **Parameters**: 
    *   `preset` - The source `GifPreset` to configure.
    *   `startFrame` - The zero-based index of the first frame to include.
    *   `endFrame` - The zero-based index of the last frame to include.
*   **Returns**: The modified `GifPreset` instance.
*   **Throws**: `ArgumentNullException` if `preset` is null; `ArgumentOutOfRangeException` if `startFrame` is negative or `endFrame` is less than `startFrame`.

### `WithStandardDefinition`
```csharp
public static GifPreset WithStandardDefinition(this GifPreset preset)
```
Restricts the output dimensions of the GIF to standard definition limits (typically capping the width or height to 480p or similar constraints) to ensure compatibility and reduced file size for legacy displays or bandwidth-constrained environments.
*   **Parameters**: `preset` - The source `GifPreset` to configure.
*   **Returns**: The modified `GifPreset` instance.
*   **Throws**: `ArgumentNullException` if `preset` is null.

### `RunAndBuildArgumentsAsync`
```csharp
public static async Task RunAndBuildArgumentsAsync(this GifPreset preset, CancellationToken cancellationToken = default)
```
Asynchronously processes the configuration within the `GifPreset` and generates the corresponding FFmpeg command-line arguments required to perform the encoding. This method may trigger validation logic or internal state resolution before returning the argument list.
*   **Parameters**: 
    *   `preset` - The configured `GifPreset` to process.
    *   `cancellationToken` - A token to monitor for cancellation requests.
*   **Returns**: A `Task` resulting in a collection or string representation of the built FFmpeg arguments.
*   **Throws**: `ArgumentNullException` if `preset` is null; `OperationCanceledException` if the `cancellationToken` is triggered; `InvalidOperationException` if the preset configuration is invalid or incomplete.

## Usage

The following example demonstrates chaining configuration methods to create a GIF with default settings, restricted to standard definition, and limited to a specific frame range before generating the arguments.

```csharp
using FfmpegFluent.Presets;
using FfmpegFluent.Extensions;

// Initialize the base preset
var preset = new GifPreset();

// Configure the preset fluently
var configuredPreset = preset
    .WithDefaultSettings()
    .WithStandardDefinition()
    .WithLoopingRange(startFrame: 10, endFrame: 50);

// Generate the FFmpeg arguments asynchronously
var arguments = await configuredPreset.RunAndBuildArgumentsAsync();

Console.WriteLine($"FFmpeg Command Args: {arguments}");
```

This example illustrates handling cancellation during the argument building phase, which is useful when integrating with long-running configuration pipelines or UI-driven processes.

```csharp
using System.Threading;
using FfmpegFluent.Presets;
using FfmpegFluent.Extensions;

public async Task BuildGifArgsWithTimeoutAsync()
{
    var source = new GifPreset();
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

    try 
    {
        var args = await source
            .WithDefaultSettings()
            .WithLoopingRange(0, 100)
            .RunAndBuildArgumentsAsync(cts.Token);
            
        // Proceed with execution using 'args'
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Argument generation timed out or was cancelled.");
    }
}
```

## Notes

*   **Immutability and Chaining**: The configuration methods (`WithDefaultSettings`, `WithLoopingRange`, `WithStandardDefinition`) return the same instance of `GifPreset` they receive. While this facilitates fluent chaining, callers should be aware that the original object is mutated. If multiple distinct configurations are needed from a single base instance, the base instance should be cloned or re-instantiated before applying different chains.
*   **Thread Safety**: The static methods within `GifPresetExtensions` do not maintain internal static state; however, they are not thread-safe with respect to the `GifPreset` instance passed as an argument. If a single `GifPreset` instance is accessed by multiple threads to apply configurations or run builds simultaneously, external synchronization is required.
*   **Validation Timing**: Specific validation errors (such as invalid frame ranges) may not be thrown until `RunAndBuildArgumentsAsync` is invoked, depending on the internal implementation of the preset. It is recommended to wrap the asynchronous call in appropriate try-catch blocks to handle `InvalidOperationException` scenarios.
*   **Cancellation**: The `RunAndBuildArgumentsAsync` method supports cancellation. If the operation involves heavy computation or I/O during the argument resolution phase, passing a valid `CancellationToken` is advised to prevent resource blocking.
