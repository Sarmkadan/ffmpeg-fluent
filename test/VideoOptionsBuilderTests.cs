#nullable enable

using System;
using System.Linq;

namespace FFmpegFluent;

public static class VideoOptionsBuilderTests
{
    public static void RunTests()
    {
        var options = new VideoOptionsBuilder()
            .Codec("libx264").Bitrate("2M").FrameRate(30).Resolution(1920, 1080)
            .Crf(23).Preset("slow").PixelFormat("yuv420p").Build();

        var args = options.BuildArgs().ToArray();
        AssertContains(args, "-pix_fmt", "yuv420p");
        AssertContains(args, "-c:v", "libx264");
        AssertThrows<InvalidOperationException>(() => options.Codec("libx265"));
        AssertThrows<ArgumentOutOfRangeException>(() => new VideoOptionsBuilder().Crf(52).Build());

        var command = FFmpegCommand.Create(new DummyLocator())
            .AddInput("input.mp4")
            .AddOutput("output.mp4")
            .WithVideo(v => v.Codec("libx264").Crf(23));
        if (!command.BuildCommandLine().Contains("-c:v libx264 -crf 23", StringComparison.Ordinal))
            throw new InvalidOperationException("Command-level video builder was not applied.");

        var configureBeforeOutput = FFmpegCommand.Create(new DummyLocator())
            .AddInput("input.mp4")
            .WithVideo(v => v.PixelFormat("yuv420p"))
            .AddOutput("output.mp4");
        if (!configureBeforeOutput.BuildCommandLine().Contains("-pix_fmt yuv420p", StringComparison.Ordinal))
            throw new InvalidOperationException("Pending command-level video builder was not applied.");

        // Existing mutable API remains supported.
        var output = new OutputFile("output.mp4").WithVideo(v => v.Codec("libx265"));
        AssertContains(output.Video.BuildArgs().ToArray(), "-c:v", "libx265");
    }

    private static void AssertContains(string[] args, string key, string value)
    {
        var index = Array.IndexOf(args, key);
        if (index < 0 || index + 1 >= args.Length || args[index + 1] != value)
            throw new InvalidOperationException($"Expected '{key} {value}'.");
    }

    private static void AssertThrows<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}.");
    }
}
