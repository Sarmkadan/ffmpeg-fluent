# HardwareAccelOptions

`HardwareAccelOptions` is a configuration type in `ffmpeg-fluent` that specifies hardware acceleration settings for FFmpeg operations. It defines the type of hardware acceleration (`HwAccelKind`), the target device (if applicable), and provides helper methods to generate FFmpeg command-line arguments for input processing and encoder selection.

## API

### `public HwAccelKind Kind`
The type of hardware acceleration to use. This determines the underlying FFmpeg hardware acceleration API (e.g., NVENC, VAAPI).

### `public string? Device`
The optional device identifier for hardware acceleration. If `null`, the default device for the selected `Kind` is used. This is typically required for APIs like VAAPI or CUDA where multiple devices may be present.

### `public string[] GetInputArguments()`
Generates the FFmpeg command-line arguments required to configure hardware-accelerated input decoding.

- **Returns**: An array of FFmpeg arguments (e.g., `-hwaccel`, `-hwaccel_device`).
- **Throws**: `InvalidOperationException` if `Kind` is not supported or if required fields (e.g., `Device`) are missing for the selected `Kind`.

### `public string GetEncoderName()`
Returns the FFmpeg encoder name associated with the configured hardware acceleration.

- **Returns**: The encoder name (e.g., `h264_nvenc`, `h264_vaapi`).
- **Throws**: `InvalidOperationException` if `Kind` is not supported or lacks a corresponding encoder.

### `public static HardwareAccelOptions Nvenc`
A pre-configured instance for NVIDIA NVENC hardware acceleration. Uses the default device.

### `public static HardwareAccelOptions Vaapi`
A pre-configured instance for VAAPI hardware acceleration. Uses the default device.

## Usage

### Example 1: Configure NVENC Hardware Acceleration
