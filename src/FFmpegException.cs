namespace FFmpegFluent;

public class FFmpegException : Exception
{
    public int ExitCode { get; }
    public string StdErr { get; }
    public string CommandLine { get; }

    public FFmpegException(string commandLine, int exitCode, string stdErr)
        : base($"FFmpeg command '{commandLine}' failed with exit code {exitCode}. StdErr: {stdErr}")
    {
        ExitCode = exitCode;
        StdErr = stdErr;
        CommandLine = commandLine;
    }
}
