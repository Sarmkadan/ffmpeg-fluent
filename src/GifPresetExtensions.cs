using System;
using System.Threading.Tasks;

namespace FFmpegFluent
{
    public static class GifPresetExtensions
    {
        public static GifPreset WithDefaultSettings(this GifPreset preset)
        {
            return preset
                .WithFps(30)
                .WithWidth(640);
        }

        public static async Task RunAndBuildArgumentsAsync(this GifPreset preset)
        {
            await preset.RunAsync();
            var arguments = preset.BuildArguments;
            Console.WriteLine("Arguments: " + string.Join(" ", arguments));
        }

        public static GifPreset WithLoopingRange(this GifPreset preset, TimeSpan start, TimeSpan duration, int loopCount)
        {
            for (int i = 0; i < loopCount; i++)
            {
                preset = preset.WithRange(start, duration);
            }
            return preset;
        }

        public static GifPreset WithStandardDefinition(this GifPreset preset)
        {
            return preset
                .WithFps(25)
                .WithWidth(720);
        }
    }
}
