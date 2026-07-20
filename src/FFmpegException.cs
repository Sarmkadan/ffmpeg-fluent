namespace FFmpegFluent;

/// <summary>
/// Represents an exception thrown when an FFmpeg command fails.
/// </summary>
public class FFmpegException : Exception
{
    /// <summary>
    /// Gets the exit code of the FFmpeg command.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Gets the standard error output of the FFmpeg command.
    /// </summary>
    public string StdErr { get; }

    /// <summary>
    /// Gets the command line used to run the FFmpeg command.
    /// </summary>
    public string CommandLine { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegException"/> class.
    /// </summary>
    /// <param name="commandLine">The command line used to run the FFmpeg command.</param>
    /// <param name="exitCode">The exit code of the FFmpeg command.</param>
    /// <param name="stdErr">The standard error output of the FFmpeg command.</param>
    public FFmpegException(string commandLine, int exitCode, string stdErr)
    : base($"FFmpeg command '{commandLine}' failed with exit code {exitCode}. StdErr: {stdErr}")
    {
        ExitCode = exitCode;
        StdErr = stdErr;
        CommandLine = commandLine;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegException"/> class with a custom error message.
    /// </summary>
    /// <param name="message">The error message describing the exception.</param>
    public FFmpegException(string message)
    : base(message)
    {
        ExitCode = 0;
        StdErr = message;
        CommandLine = string.Empty;
    }
}