# WatermarkHelperExtensions

Provides static factory methods for creating `WatermarkHelper` instances positioned at common screen corners. Useful for adding watermarks to video frames at predefined locations without manual coordinate calculations.

## API

### `TopLeft`
Creates a `WatermarkHelper` positioned at the top-left corner of the video frame.

- **Return value**: A `WatermarkHelper` instance configured to render the watermark at the top-left corner.
- **Throws**: No exceptions are thrown under normal conditions.

### `TopRight`
Creates a `WatermarkHelper` positioned at the top-right corner of the video frame.

- **Return value**: A `WatermarkHelper` instance configured to render the watermark at the top-right corner.
- **Throws**: No exceptions are thrown under normal conditions.

### `BottomLeft`
Creates a `WatermarkHelper` positioned at the bottom-left corner of the video frame.

- **Return value**: A `WatermarkHelper` instance configured to render the watermark at the bottom-left corner.
- **Throws**: No exceptions are thrown under normal conditions.

### `BottomRight`
Creates a `WatermarkHelper` positioned at the bottom-right corner of the video frame.

- **Return value**: A `WatermarkHelper` instance configured to render the watermark at the bottom-right corner.
- **Throws**: No exceptions are thrown under normal conditions.

## Usage

```csharp
// Add a watermark to the top-left corner of the output video
var result = await FFmpeg.Concatenate(inputFiles)
    .Watermark(WatermarkHelperExtensions.TopLeft, "watermark.png")
    .OutputTo("output.mp4")
    .ProcessAsynchronously();

// Add a watermark to the bottom-right corner with custom opacity
var result = await FFmpeg.Concatenate(inputFiles)
    .Watermark(WatermarkHelperExtensions.BottomRight.WithOpacity(0.5), "logo.png")
    .OutputTo("output_with_logo.mp4")
    .ProcessAsynchronously();
```

## Notes

All methods are thread-safe as they return new `WatermarkHelper` instances without shared mutable state. Edge cases such as null input files or invalid image paths are handled by the underlying FFmpeg pipeline rather than these factory methods. The returned `WatermarkHelper` instances assume the watermark image is provided as an `InputFile` in subsequent operations.
