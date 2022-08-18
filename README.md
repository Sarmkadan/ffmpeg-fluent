# ffmpeg-fluent

Typed fluent DSL over the FFmpeg CLI with progress and cancellation.

## AudioOptions

The `AudioOptions` type allows you to configure audio settings for your FFmpeg command. You can specify the codec, bitrate, sample rate, number of channels, volume, or disable audio altogether.

### Example usage:

```csharp
// AudioOptions usage example
```

## ThumbnailPreset

The `ThumbnailPreset` type provides a fluent interface for extracting a single preview frame (thumbnail) from a video file. It allows you to specify the source video, output path, exact timestamp, desired dimensions, and whether to use smart frame selection for better quality thumbnails.

### Example usage:

```csharp
using FFmpegFluent;

// Create a thumbnail at 2 minutes and 30 seconds from the video
var thumbnail = new ThumbnailPreset("input.mp4", "output.jpg")
    .AtTime(TimeSpan.FromMinutes(2.5))
    .WithSize(800, 600);

// Execute the thumbnail extraction
await thumbnail.RunAsync();

// Or use smart selection for better quality
var smartThumbnail = new ThumbnailPreset("input.mp4", "smart.jpg")
    .UseSmartSelect()
    .AtTime(TimeSpan.FromMinutes(1.5))
    .WithSize(1280, 720);

await smartThumbnail.RunAsync();
```

## WatermarkHelper

The `WatermarkHelper` type allows you to create a watermark filter for your FFmpeg command. You can specify the source image, opacity, and scale of the watermark.

### Example usage:

```csharp
using FFmpegFluent;

// Create a watermark with a 50% opacity and 0.5 scale
var watermark = new WatermarkHelper("watermark.png")
    .At(new InputFile("input.mp4")) // Specify the source image
    .WithOpacity(0.5)
    .WithScale(0.5);

// Build the filter complex string
var filterComplex = watermark.BuildFilterComplex();

// Build the FFmpeg arguments
var arguments = watermark.BuildArguments();

// Execute the watermark creation
await watermark.RunAsync();
```

```