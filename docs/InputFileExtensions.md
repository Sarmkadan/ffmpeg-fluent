# InputFileExtensions

Provides extension methods for configuring `InputFile` instances with common FFmpeg input options. These methods simplify the construction of input file arguments by offering fluent, chainable helpers for seeking, duration limiting, and passing arbitrary custom options.

## API

### Seek

```csharp
public static InputFile Seek { get; }
```

A static property that returns an `InputFile` instance pre-configured with the `-ss` (seek) option. The returned object acts as a factory or template for creating input files that start processing at a specific timestamp. The actual seek position must be supplied as an argument when using this property.

**Parameters:** None (static property).

**Returns:** `InputFile` – an input file object with the seek flag enabled.

**Throws:** No exceptions under normal usage.

---

### Duration

```csharp
public static InputFile Duration { get; }
```

A static property that returns an `InputFile` instance pre-configured with the `-t` (duration) option. The returned object limits processing to a specified duration from the start point. The actual duration value must be supplied as an argument when using this property.

**Parameters:** None (static property).

**Returns:** `InputFile` – an input file object with the duration flag enabled.

**Throws:** No exceptions under normal usage.

---

### Options

```csharp
public static InputFile Options { get; }
```

A static property that returns an `InputFile` instance pre-configured for receiving arbitrary custom FFmpeg options. The returned object serves as a base for passing one or more key-value option pairs. The actual options must be supplied as arguments when using this property.

**Parameters:** None (static property).

**Returns:** `InputFile` – an input file object ready to accept custom options.

**Throws:** No exceptions under normal usage.

---

### Options (Extension Method)

```csharp
public static InputFile Options(this InputFile inputFile, params /* option arguments */)
```

An extension method that applies one or more custom FFmpeg options to an existing `InputFile` instance. This overload accepts a variable number of arguments representing option key-value pairs, allowing multiple options to be set in a single call.

**Parameters:**
- `inputFile` – the `InputFile` instance to configure.
- `params` – a variable-length list of option arguments (typically alternating keys and values).

**Returns:** `InputFile` – the same instance with the specified options applied, enabling fluent chaining.

**Throws:** May throw if the arguments are malformed or incompatible with the underlying FFmpeg argument builder. Specific exceptions depend on the internal validation logic.

## Usage

### Example 1: Seeking and Limiting Duration

```csharp
using FFmpegFluent;

var input = InputFileExtensions.Seek("00:01:30")
    .Duration("00:00:45")
    .SetInputPath("video.mp4");

// Generates: -ss 00:01:30 -t 00:00:45 -i video.mp4
```

This example creates an input that skips the first 90 seconds of `video.mp4` and processes only 45 seconds from that point.

### Example 2: Applying Custom Options

```csharp
using FFmpegFluent;

var input = InputFileExtensions.Options("hwaccel", "cuda", "hwaccel_output_format", "cuda")
    .SetInputPath("highres.mkv");

// Generates: -hwaccel cuda -hwaccel_output_format cuda -i highres.mkv
```

This example configures hardware-accelerated decoding for an input file by passing multiple custom option pairs.

## Notes

- The static properties `Seek`, `Duration`, and `Options` return `InputFile` objects that require additional arguments (e.g., timestamps, option values) to be meaningful. Using them without supplying the necessary values may produce incomplete or invalid FFmpeg arguments.
- The extension method `Options` accepts a `params` array. Ensure that arguments are provided in correct key-value pairs; an odd number of arguments or missing values may cause runtime errors.
- These methods are designed for fluent chaining. Each call returns the `InputFile` instance, allowing multiple configurations to be combined in a single expression.
- Thread safety is not guaranteed. `InputFile` instances are not documented as immutable or thread-safe; concurrent modification of the same instance across threads may lead to unpredictable argument generation. Create separate instances per thread when building inputs in parallel contexts.
