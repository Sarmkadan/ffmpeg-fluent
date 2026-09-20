#nullable enable

using System;
using System.Runtime.InteropServices;

namespace FFmpegFluent;

/// <summary>
/// Simple unit tests for ArgumentEscaper.
/// </summary>
public static class ArgumentEscaperTests
{
    public static void RunAll()
    {
        Console.WriteLine("Running ArgumentEscaperTests...");

        AssertEqual("video.mp4", ArgumentEscaper.EscapePath("video.mp4"), "Plain path is unchanged");

        var spacedPath = ArgumentEscaper.EscapePath("my video.mp4");
        var expectedSpacedPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "\"my video.mp4\""
            : "'my video.mp4'";
        AssertEqual(expectedSpacedPath, spacedPath, "Path with spaces is quoted");

        var quotedPath = ArgumentEscaper.EscapePath("my 'single' \"double\" video.mp4");
        var expectedQuotedPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "\"my 'single' \\\"double\\\" video.mp4\""
            : "'my '\\\\''single'\\\\'' \"double\" video.mp4'";
        AssertEqual(expectedQuotedPath, quotedPath, "Path quotes use OS-specific escaping");

        AssertEqual("\"scale=1280\\\\:720\"", ArgumentEscaper.EscapeFilterGraph("scale=1280\\:720"),
            "Filter graph backslash is escaped");
        AssertEqual("\"drawtext=text=\\\"hello\\\"\"", ArgumentEscaper.EscapeFilterGraph("drawtext=text=\"hello\""),
            "Filter graph double quotes are escaped");
        AssertEqual("\"drawtext=text=\\'hello\\'\"", ArgumentEscaper.EscapeFilterGraph("drawtext=text='hello'"),
            "Filter graph single quotes are escaped");

        AssertThrowsArgumentException(() => ArgumentEscaper.EscapeArgument(null!),
            "Null argument is rejected");
        AssertThrowsArgumentException(() => ArgumentEscaper.EscapeArgument("   "),
            "Whitespace argument is rejected");

        var escapedArgument = ArgumentEscaper.EscapeArgument("value with spaces");
        AssertTrue(escapedArgument.Length > 0, "Escaped argument is non-empty");

        Console.WriteLine("All ArgumentEscaperTests passed.");
    }

    private static void AssertEqual(string expected, string actual, string message)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{message}. Expected '{expected}', but received '{actual}'.");
        }
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertThrowsArgumentException(Action action, string message)
    {
        try
        {
            action();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }
}
