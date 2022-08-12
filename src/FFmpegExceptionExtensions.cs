using System;

namespace FFmpegFluent
{
    public static class FFmpegExceptionExtensions
    {
        public static string GetErrorMessage(this FFmpegException exception)
        {
            return $"FFmpeg exited with code {exception.ExitCode}. Command line: {exception.CommandLine}. Error message: {exception.StdErr}";
        }

        public static bool IsSuccessful(this FFmpegException exception)
        {
            return exception.ExitCode == 0;
        }

        public static string GetCommandLineWithExitCode(this FFmpegException exception)
        {
            return $"{exception.CommandLine} (exit code: {exception.ExitCode})";
        }

        public static string GetDetailedErrorMessage(this FFmpegException exception)
        {
            return $"FFmpeg exception occurred. Command line: {exception.CommandLine}. Exit code: {exception.ExitCode}. Standard error: {exception.StdErr}";
        }
    }
}
