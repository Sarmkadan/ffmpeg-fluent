#nullable enable

using System;

namespace FFmpegFluent;

/// <summary>
/// Simple unit tests for FFmpegCommand validation.
/// </summary>
public static class FFmpegCommandValidationTests
{
    public static void RunTests()
    {
        Console.WriteLine("Running FFmpegCommandValidationTests...");

        // Test 1: No inputs
        var cmd1 = FFmpegCommand.Create(new DummyLocator());
        try
        {
            cmd1.EnsureValid();
            throw new Exception("Test 1 Failed: Should have thrown ArgumentException for no inputs");
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("input"))
            {
                Console.WriteLine("Test 1 Passed: No inputs caught");
            }
            else
            {
                throw new Exception("Test 1 Failed: Exception message did not contain 'input'");
            }
        }

        // Test 2: No outputs
        var cmd2 = FFmpegCommand.Create(new DummyLocator());
        cmd2.AddInput("input.mp4");
        try
        {
            cmd2.EnsureValid();
            throw new Exception("Test 2 Failed: Should have thrown ArgumentException for no outputs");
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("output"))
            {
                Console.WriteLine("Test 2 Passed: No outputs caught");
            }
            else
            {
                throw new Exception("Test 2 Failed: Exception message did not contain 'output'");
            }
        }

        Console.WriteLine("All FFmpegCommandValidationTests passed.");
    }
}

public class DummyLocator : IFFmpegLocator
{
    public string FFmpegPath => "dummy";
    public string FFprobePath => "dummy";
    public FFmpegVersion Version => new(0, 0, 0);
    public FFmpegVersion FFprobeVersion => new(0, 0, 0);
    public FFmpegCommand CreateCommand() => throw new NotImplementedException();
}
