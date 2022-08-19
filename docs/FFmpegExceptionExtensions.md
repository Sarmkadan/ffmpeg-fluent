# FFmpegExceptionExtensions
The `FFmpegExceptionExtensions` type provides a set of extension methods for handling exceptions related to FFmpeg operations. It offers a range of methods to extract and analyze error information from exceptions, making it easier to diagnose and handle issues that may arise during FFmpeg processing.

## API
The `FFmpegExceptionExtensions` type includes the following public members:
* `GetErrorMessage`: Retrieves the error message associated with the exception.
	+ Parameters: The exception instance.
	+ Return Value: A string representing the error message.
	+ Throws: None.
* `IsSuccessful`: Determines whether the FFmpeg operation was successful.
	+ Parameters: The exception instance.
	+ Return Value: A boolean indicating whether the operation was successful.
	+ Throws: None.
* `GetCommandLineWithExitCode`: Retrieves the command line used for the FFmpeg operation, including the exit code.
	+ Parameters: The exception instance.
	+ Return Value: A string representing the command line with the exit code.
	+ Throws: None.
* `GetDetailedErrorMessage`: Retrieves a detailed error message associated with the exception.
	+ Parameters: The exception instance.
	+ Return Value: A string representing the detailed error message.
	+ Throws: None.

## Usage
Here are two examples of using the `FFmpegExceptionExtensions` type:
```csharp
try
{
    // Perform an FFmpeg operation
}
catch (FFmpegException ex)
{
    string errorMessage = ex.GetErrorMessage();
    Console.WriteLine($"Error: {errorMessage}");
}

try
{
    // Perform an FFmpeg operation
}
catch (FFmpegException ex)
{
    if (!ex.IsSuccessful())
    {
        string commandLine = ex.GetCommandLineWithExitCode();
        string detailedErrorMessage = ex.GetDetailedErrorMessage();
        Console.WriteLine($"Command Line: {commandLine}");
        Console.WriteLine($"Detailed Error: {detailedErrorMessage}");
    }
}
```

## Notes
When using the `FFmpegExceptionExtensions` type, consider the following:
* The `GetErrorMessage` and `GetDetailedErrorMessage` methods may return null or empty strings if no error message is available.
* The `IsSuccessful` method returns a boolean value indicating whether the FFmpeg operation was successful. A value of `false` does not necessarily indicate an error, but rather that the operation did not complete as expected.
* The `GetCommandLineWithExitCode` method returns a string representing the command line used for the FFmpeg operation, including the exit code. This can be useful for debugging purposes.
* The `FFmpegExceptionExtensions` type is designed to be thread-safe, allowing it to be used safely in multi-threaded environments. However, the underlying FFmpeg operations may not be thread-safe, and care should be taken to ensure that FFmpeg operations are properly synchronized when using this type in a multi-threaded context.
