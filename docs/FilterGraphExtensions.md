# FilterGraphExtensions

The `FilterGraphExtensions` class serves as a static factory for constructing common FFmpeg filter graphs within the `ffmpeg-fluent` library. It provides predefined, ready-to-use instances of `FilterGraph` for standard video processing operations such as overlaying images, scaling resolutions, applying fade effects, and cropping frames, allowing developers to integrate these filters into a media processing pipeline without manually constructing complex filter syntax strings.

## API

### `public static FilterGraph Overlay`
Represents a pre-configured filter graph designed to overlay a secondary video or image stream onto a primary video stream. This member returns a `FilterGraph` instance configured with the standard `overlay` filter. It accepts no parameters upon retrieval as it represents a template; specific input streams and positioning arguments are typically resolved when the graph is bound to a `FFmpegCommand` context. This property never throws exceptions upon access.

### `public static FilterGraph Scale`
Represents a pre-configured filter graph used to resize video frames to a specific resolution or aspect ratio. This member returns a `FilterGraph` instance configured with the `scale` filter. Like other members, it acts as a template where target dimensions are usually specified during the command building phase. Accessing this property is safe and will not throw exceptions.

### `public static FilterGraph Fade`
Represents a pre-configured filter graph for applying fade-in or fade-out effects to video or audio streams. This member returns a `FilterGraph` instance configured with the `fade` filter. The specific type of fade (in/out), duration, and start time are generally configured when applying the graph to a specific stream. Retrieving this static property does not throw exceptions.

### `public static FilterGraph Crop`
Represents a pre-configured filter graph intended to crop a specific region from the video frame. This member returns a `FilterGraph` instance configured with the `crop` filter. Parameters defining the crop area (width, height, x, y coordinates) are supplied during the pipeline configuration. Accessing this property is thread-safe and does not throw exceptions.

## Usage

The following examples demonstrate how to utilize these static filter graphs within a fluent FFmpeg command chain.

### Example 1: Scaling and Overlaying a Watermark
This example loads a video, scales it to 1280x720, and overlays a logo image on the top-right corner.

```csharp
using FFmpegFluent;

var command = FFmpegArguments
    .FromFile("input_video.mp4")
    .WithFilter(FilterGraphExtensions.Scale) // Applies scaling logic
    .WithInput("logo.png")
    .WithFilter(FilterGraphExtensions.Overlay) // Applies overlay logic
    .ToFile("output_video.mp4");

await command.ProcessAsync();
```

### Example 2: Cropping and Fading Out
This example crops a 500x500 region from the center of the video and applies a fade-out effect over the last 2 seconds.

```csharp
using FFmpegFluent;

var command = FFmpegArguments
    .FromFile("source_footage.mov")
    .WithFilter(FilterGraphExtensions.Crop) // Applies cropping logic
    .WithFilter(FilterGraphExtensions.Fade) // Applies fading logic
    .ToFile("clipped_fade.mp4");

await command.ProcessAsync();
```

## Notes

*   **Template Nature**: The members exposed by `FilterGraphExtensions` are static templates. They do not contain hardcoded values for dimensions, coordinates, or durations. These specific parameters must be supplied via the fluent API methods (e.g., `.WithArgument` or specific extension methods) when chaining the filter to a command, or the underlying FFmpeg process may fail due to missing arguments.
*   **Thread Safety**: As the class only exposes static properties that return immutable or newly instantiated `FilterGraph` configurations (depending on internal implementation of the getter), reading these properties is thread-safe. However, once a `FilterGraph` instance is retrieved and modified within a specific command context, that specific instance should not be shared across concurrent modification threads.
*   **Filter Ordering**: When applying multiple filters from this class to a single stream, the order of invocation in the fluent chain determines the order of execution in the FFmpeg filter graph. For example, scaling before cropping yields different results than cropping before scaling.
*   **Input Requirements**: Filters like `Overlay` inherently require multiple inputs (a main video and an overlay source). Ensure that the corresponding input files are added to the `FFmpegArguments` chain prior to or alongside the filter application to prevent runtime errors during process execution.
