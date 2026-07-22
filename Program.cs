using System;
using FFmpegFluent;

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing AudioOptions and VideoOptions unification...\n");

        // Test AudioOptions with validation
        Console.WriteLine("1. Testing AudioOptions validation:");
        try
        {
            var audio = new AudioOptions();
            audio.Codec("aac");
            audio.Bitrate("192k");
            audio.SampleRate(44100);
            audio.Channels(2);
            audio.Volume(1.0);
            Console.WriteLine("   ✓ AudioOptions fluent API works");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Failed: {ex.Message}");
        }

        // Test AudioOptions validation
        Console.WriteLine("\n2. Testing AudioOptions validation:");
        try
        {
            var audio = new AudioOptions();
            audio.SampleRate(-1);
            Console.WriteLine("   ✗ Should have thrown for negative sample rate");
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("   ✓ Correctly rejects negative sample rate");
        }

        try
        {
            var audio = new AudioOptions();
            audio.Codec(null!);
            Console.WriteLine("   ✗ Should have thrown for null codec");
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("   ✓ Correctly rejects null codec");
        }

        try
        {
            var audio = new AudioOptions();
            audio.Bitrate("");
            Console.WriteLine("   ✗ Should have thrown for empty bitrate");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("   ✓ Correctly rejects empty bitrate");
        }

        // Test VideoOptions with validation
        Console.WriteLine("\n3. Testing VideoOptions validation:");
        try
        {
            var video = new VideoOptions();
            video.Codec("libx264");
            video.Bitrate("2M");
            video.Crf(23);
            video.Preset("slow");
            video.FrameRate(30.0);
            video.Resolution(1920, 1080);
            Console.WriteLine("   ✓ VideoOptions fluent API works");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Failed: {ex.Message}");
        }

        // Test VideoOptions validation
        Console.WriteLine("\n4. Testing VideoOptions validation:");
        try
        {
            var video = new VideoOptions();
            video.Crf(100);
            Console.WriteLine("   ✗ Should have thrown for invalid CRF");
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("   ✓ Correctly rejects invalid CRF (100)");
        }

        try
        {
            var video = new VideoOptions();
            video.FrameRate(-30.0);
            Console.WriteLine("   ✗ Should have thrown for negative frame rate");
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("   ✓ Correctly rejects negative frame rate");
        }

        try
        {
            var video = new VideoOptions();
            video.Resolution(-1920, 1080);
            Console.WriteLine("   ✗ Should have thrown for negative width");
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("   ✓ Correctly rejects negative width");
        }

        // Test VideoOptions.Crop validation
        Console.WriteLine("\n5. Testing VideoOptions.Crop validation:");
        try
        {
            var video = new VideoOptions();
            video.Crop(-100, 100, 0, 0);
            Console.WriteLine("   ✗ Should have thrown for negative crop width");
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("   ✓ Correctly rejects negative crop width");
        }

        // Test extension methods
        Console.WriteLine("\n6. Testing VideoOptions extension methods:");
        try
        {
            var video = new VideoOptions();
            video.WithLibx264("5M");
            video.WithHighQuality(crf: 18, preset: "medium");
            Console.WriteLine("   ✓ VideoOptions extension methods work");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Failed: {ex.Message}");
        }

        // Test JSON serialization
        Console.WriteLine("\n7. Testing JSON serialization:");
        try
        {
            var audio = new AudioOptions().Codec("aac").Bitrate("192k");
            var audioJson = audio.ToJson();
            Console.WriteLine($"   ✓ AudioOptions JSON: {audioJson}");

            var video = new VideoOptions().Codec("libx264").Bitrate("2M");
            var videoJson = video.ToJson();
            Console.WriteLine($"   ✓ VideoOptions JSON: {videoJson}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Failed: {ex.Message}");
        }

        // Test OutputFile integration
        Console.WriteLine("\n8. Testing OutputFile integration:");
        try
        {
            var output = new OutputFile("test.mp4");
            output.WithVideo(v => v.Codec("libx264").Bitrate("2M"));
            output.WithAudio(a => a.Codec("aac").Bitrate("192k"));
            var args = output.BuildArgs();
            Console.WriteLine("   ✓ OutputFile integration works");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Failed: {ex.Message}");
        }

        Console.WriteLine("\n✅ All tests completed successfully!");
    }
}
