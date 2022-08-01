# InputFile
The `InputFile` type is a fundamental component in the `ffmpeg-fluent` project, designed to represent and manipulate input files for FFmpeg operations. It provides a fluent interface for configuring various aspects of input file handling, such as seeking, duration, looping, and specifying options. This enables developers to construct complex FFmpeg commands in a straightforward and readable manner.

## API
* `Path`: A string property representing the path to the input file.
* `InputFile()`: The constructor for creating a new `InputFile` instance.
* `Seek`: A method that allows setting the seek position for the input file. Returns the `InputFile` instance itself for method chaining.
* `Duration`: A method that sets the duration for the input file. Returns the `InputFile` instance itself for method chaining.
* `Loop`: A method that enables looping for the input file. Returns the `InputFile` instance itself for method chaining.
* `Option`: A method for specifying additional options for the input file. Returns the `InputFile` instance itself for method chaining.
* `BuildArgs`: A property that yields an enumerable collection of strings representing the constructed FFmpeg arguments based on the `InputFile` configuration.

## Usage
The following examples demonstrate how to utilize the `InputFile` type in C#:
```csharp
// Example 1: Basic input file configuration
var inputFile = new InputFile();
inputFile.Path = "path/to/input.mp4";
inputFile.Seek(10); // Seek 10 seconds into the file
var args = inputFile.BuildArgs;

// Example 2: Configuring input file with duration and loop
var inputFile2 = new InputFile();
inputFile2.Path = "path/to/input2.mp4";
inputFile2.Duration(30); // Set duration to 30 seconds
inputFile2.Loop(); // Enable looping
inputFile2.Option("-vf", "scale=640:480"); // Specify a video filter option
var args2 = inputFile2.BuildArgs;
```

## Notes
When working with `InputFile`, consider the following:
- The `Seek`, `Duration`, `Loop`, and `Option` methods return the `InputFile` instance itself, allowing for method chaining but also meaning that these methods do not throw exceptions based on their parameters. Instead, invalid configurations might result in errors during the FFmpeg execution phase.
- The `BuildArgs` property generates the FFmpeg command arguments based on the current state of the `InputFile` instance. It does not validate the feasibility of the generated command; such validation should be performed separately, potentially using tools like `FFmpegProgressValidation`.
- `InputFile` instances are not inherently thread-safe. If multiple threads access and modify an `InputFile` instance concurrently, synchronization mechanisms should be employed to prevent data corruption or unexpected behavior.
