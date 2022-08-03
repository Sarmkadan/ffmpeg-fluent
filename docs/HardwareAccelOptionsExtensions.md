# HardwareAccelOptionsExtensions

Provides extension methods and a static property for configuring and querying hardware acceleration options in a fluent manner.

## API

### IsEnabled
- **Purpose:** Indicates whether hardware acceleration is currently enabled globally for the application.
- **Return Value:** `true` if hardware acceleration is enabled; otherwise `false`.
- **Exceptions:** None.

### WithDevice
- **Purpose:** Configures the device to be used for hardware acceleration.
- **Parameters:** `devicePath` (string) – the path or identifier of the hardware device (e.g., `"/dev/dri/renderD128"` or `"CUDA:0"`).
- **Return Value:** A new `HardwareAccelOptions` instance with the specified device set.
- **Exceptions:** 
  - `ArgumentNullException` if `devicePath` is `null`.
  - `ArgumentException` if `devicePath` is empty or consists only of whitespace.

### Supports
- **Purpose:** Determines whether a specific hardware acceleration type is supported on the current system.
- **Parameters:** `type` (HardwareAccelType) – the acceleration type to query (e.g., `VAAPI`, `DXVA2`, `CUDA`).
- **Return Value:** `true` if the specified type is supported; otherwise `false`.
- **Exceptions:** None.

### GetDevicePath
- **Purpose:** Retrieves the device path associated with a set of hardware acceleration options.
- **Parameters:** `options` (HardwareAccelOptions) – the options instance to inspect.
- **Return Value:** The device path string configured in `options`, or `null` if no device is set.
- **Exceptions:** 
  - `InvalidOperationException` if `options` is `null`.

## Usage

### Example 1: Enabling VAAPI acceleration with a specific device
```csharp
using FFmpeg.Fluent;
using FFmpeg.Fluent.Enums;

if (HardwareAccelOptionsExtensions.Supports(HardwareAccelType.VAAPI))
{
    var options = new HardwareAccelOptions()
        .WithDevice("/dev/dri/renderD128")
        .EnableHardwareAcceleration(HardwareAccelType.VAAPI);

    // Pass `options` to an FFmpeg pipeline builder
}
```

### Example 2: Checking global state and retrieving device path
```csharp
using FFmpeg.Fluent;

bool isEnabled = HardwareAccelOptionsExtensions.IsEnabled;
Console.WriteLine($"Hardware acceleration enabled: {isEnabled}");

var opts = new HardwareAccelOptions().WithDevice("CUDA:0");
string device = HardwareAccelOptionsExtensions.GetDevicePath(opts);
Console.WriteLine($"Device path: {device}");
```

## Notes

- `IsEnabled` is a static property that reflects a global runtime state; reading it is thread‑safe.
- `WithDevice` does not mutate the original `HardwareAccelOptions` instance; it returns a new instance with the device applied. Supplying a `null` or whitespace‑only `devicePath` will throw the indicated exceptions.
- `Supports` performs a pure query of the underlying system capabilities and has no side effects, making it safe to invoke concurrently from multiple threads.
- `GetDevicePath` returns `null` when the supplied options have no device configured. Passing a `null` options instance results in an `InvalidOperationException`.
- All extension members are stateless and rely only on their input parameters, so they are thread‑safe for concurrent use.
