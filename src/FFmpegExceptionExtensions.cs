using System;

namespace FFmpegFluent
{
	/// <summary>
	/// Provides extension methods for <see cref="FFmpegException"/> to facilitate error handling and message formatting.
	/// </summary>
	public static class FFmpegExceptionExtensions
	{
		/// <summary>
		/// Gets a formatted error message containing the FFmpeg exit code, command line, and standard error output.
		/// </summary>
		/// <param name="exception">The <see cref="FFmpegException"/> instance.</param>
		/// <returns>A formatted error message string.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
		public static string GetErrorMessage(this FFmpegException exception)
		{
			ArgumentNullException.ThrowIfNull(exception);

			return $"FFmpeg exited with code {exception.ExitCode}. Command line: {exception.CommandLine}. Error message: {exception.StdErr}";
		}

		/// <summary>
		/// Determines whether the FFmpeg command completed successfully (exit code 0).
		/// </summary>
		/// <param name="exception">The <see cref="FFmpegException"/> instance.</param>
		/// <returns><see langword="true"/> if the exit code is 0; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
		public static bool IsSuccessful(this FFmpegException exception)
		{
			ArgumentNullException.ThrowIfNull(exception);

			return exception.ExitCode == 0;
		}

		/// <summary>
		/// Gets the command line with the exit code appended for diagnostic purposes.
		/// </summary>
		/// <param name="exception">The <see cref="FFmpegException"/> instance.</param>
		/// <returns>A string containing the command line followed by the exit code.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
		public static string GetCommandLineWithExitCode(this FFmpegException exception)
		{
			ArgumentNullException.ThrowIfNull(exception);

			return $"{exception.CommandLine} (exit code: {exception.ExitCode})";
		}

		/// <summary>
		/// Gets a detailed error message containing comprehensive information about the FFmpeg failure.
		/// </summary>
		/// <param name="exception">The <see cref="FFmpegException"/> instance.</param>
		/// <returns>A detailed error message string with command line, exit code, and standard error.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
		public static string GetDetailedErrorMessage(this FFmpegException exception)
		{
			ArgumentNullException.ThrowIfNull(exception);

			return $"FFmpeg exception occurred. Command line: {exception.CommandLine}. Exit code: {exception.ExitCode}. Standard error: {exception.StdErr}";
		}
	}
}