# FFmpegProgressExtensions
The `FFmpegProgressExtensions` type provides a set of static methods for working with FFmpeg progress, allowing developers to estimate the remaining time, check if the process is complete, and convert progress to a string representation. These extensions are designed to simplify the process of monitoring and reporting on FFmpeg operations, making it easier to integrate FFmpeg into larger applications.

## API
* `GetRemainingTime`: Returns the estimated remaining time for the FFmpeg operation as a `TimeSpan?`. This method does not take any parameters and returns `null` if the remaining time cannot be estimated.
* `IsComplete`: Returns a boolean indicating whether the FFmpeg operation is complete. This method does not take any parameters.
* `ToProgressString`: Returns a string representation of the FFmpeg progress. This method does not take any parameters and returns a string in a human-readable format.
* `GetEstimatedTotalDuration`: Returns the estimated total duration of the FFmpeg operation as a `TimeSpan?`. This method does not take any parameters and returns `null` if the total duration cannot be estimated.

## Usage
The following examples demonstrate how to use the `FFmpegProgressExtensions` type in a C# application:
```csharp
// Example 1: Checking if the FFmpeg operation is complete
if (FFmpegProgressExtensions.IsComplete)
{
    Console.WriteLine("FFmpeg operation is complete.");
}
else
{
    Console.WriteLine("FFmpeg operation is still in progress.");
}

// Example 2: Estimating the remaining time and displaying progress
TimeSpan? remainingTime = FFmpegProgressExtensions.GetRemainingTime;
string progressString = FFmpegProgressExtensions.ToProgressString;

Console.WriteLine($"Remaining time: {remainingTime?.ToString() ?? "Unknown"}");
Console.WriteLine($"Progress: {progressString}");
```

## Notes
When using the `FFmpegProgressExtensions` type, note that the `GetRemainingTime` and `GetEstimatedTotalDuration` methods may return `null` if the required information is not available. Additionally, the `ToProgressString` method returns a string representation of the progress, which may not be suitable for all use cases. The `FFmpegProgressExtensions` type is designed to be thread-safe, but developers should still take care to ensure that the underlying FFmpeg operation is properly synchronized to avoid concurrency issues.
