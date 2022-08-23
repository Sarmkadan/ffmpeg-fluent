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

// Create a new ThumbnailPreset with the watermark
var thumbnail = new ThumbnailPreset("output.jpg", "output_with_watermark.jpg")
    .AtTime(TimeSpan.FromMinutes(2.5))
    .WithSize(800, 600)
    .WithWatermark(filterComplex);

// Execute the thumbnail extraction
await thumbnail.RunAsync();
```

## InputFile

The `InputFile` type represents a media file that can be fed into an FFmpeg command. It exposes fluent methods to set common FFmpeg input options such as seek position, duration, loop count, and arbitrary options. The `BuildArgs` method returns the command‑line arguments that FFmpeg expects for this input.

### Example usage:

```csharp
using FFmpegFluent;

// Create an input file with a seek offset, duration, loop count and a custom option
var input = new InputFile("input.mp4")
    .Seek(TimeSpan.FromSeconds(5))
    .Duration(TimeSpan.FromMinutes(1))
    .Loop(2)
    .Option("someOption", "value");

// Retrieve the FFmpeg arguments for this input
var args = input.BuildArgs();

// args can be passed to a command builder or executed directly
```

## ExtractAudioPreset

The `ExtractAudioPreset` type allows you to extract audio from a video file. You can specify the codec, bitrate, and stream index of the audio to extract.

### Example usage:

```csharp
using FFmpegFluent;

// Extract audio with the default codec and bitrate
var extractAudio = new ExtractAudioPreset("input.mp4")
    .WithCodec("copy") // Use the default codec
    .WithBitrate(128000) // Set the bitrate to 128 kbps
    .StreamIndex(0); // Extract the first audio stream

// Build the FFmpeg arguments
var arguments = extractAudio.BuildArguments();

// Execute the audio extraction
await extractAudio.RunAsync();
```

## OutputFile

The `OutputFile` type represents the destination file for an FFmpeg command. It lets you specify video and audio options, format, overwrite behavior, custom options, and metadata before building the final argument list.

### Example usage:

```csharp
using FFmpegFluent;

// Create an output file and configure video/audio options
var output = new OutputFile("output.mp4")
    .WithVideo(new VideoOptions()
        .Codec("libx264")
        .Bitrate(2000000))
    .WithAudio(new AudioOptions()
        .SampleRate(44100)
        .Channels(2))
    .Format("mp4")
    .Overwrite()
    .Option("someOption", "value")
    .Metadata("title", "My Video");

// Build the FFmpeg arguments for the output
var args = output.BuildArgs();

// args can be passed to a command builder or executed directly
```

## ConcatPreset

The `ConcatPreset` type allows you to concatenate multiple media files into a single output. It supports adding input files, optionally re‑encoding them, and building the concat list content used by FFmpeg. The `RunAsync` method executes the concatenation asynchronously.

### Example usage:

```csharp
using FFmpegFluent;

// Create a concatenation preset, add two input files, enable re‑encoding, and run
var concat = new ConcatPreset()
    .AddInput("input1.mp4")
    .AddInput("input2.mp4")
    .WithReencode();

await concat.RunAsync();
```
