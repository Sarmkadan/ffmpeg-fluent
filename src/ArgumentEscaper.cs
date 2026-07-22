using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FFmpegFluent;

/// <summary>
/// Provides OS-specific argument escaping and quoting for FFmpeg CLI arguments.
/// This class handles proper escaping of paths, filter graphs, and other arguments
/// to prevent command injection and ensure correct argument parsing on both Windows and Unix systems.
/// </summary>
public static class ArgumentEscaper
{
    /// <summary>
    /// Escapes and quotes a file path for use in FFmpeg command line arguments.
    /// Handles spaces, special characters, and OS-specific path formats.
    /// </summary>
    /// <param name="path">The file path to escape.</param>
    /// <returns>The properly escaped and quoted path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or whitespace.</exception>
    public static string EscapePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Normalize path separators for consistency
        var normalizedPath = path.Replace('\\', Path.DirectorySeparatorChar);

        // Use OS-specific quoting rules
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return EscapePathWindows(normalizedPath);
        }
        else
        {
            return EscapePathUnix(normalizedPath);
        }
    }

    /// <summary>
    /// Escapes and quotes a filter graph string for use in FFmpeg's -filter_complex argument.
    /// Filter graphs contain special characters like colons, brackets, quotes, and backslashes
    /// that need proper escaping for the shell and FFmpeg parser.
    /// </summary>
    /// <param name="filterGraph">The filter graph string to escape.</param>
    /// <returns>The properly escaped and quoted filter graph string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filterGraph"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="filterGraph"/> is empty or whitespace.</exception>
    public static string EscapeFilterGraph(string filterGraph)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filterGraph);

        // Filter graphs use Unix-style escaping regardless of OS
        // We need to escape: backslashes, quotes, and handle special characters
        var sb = new StringBuilder(filterGraph.Length + 10);

        foreach (char c in filterGraph)
        {
            switch (c)
            {
                case '\\':
                    // Escape backslashes
                    sb.Append("\\\\");
                    break;
                case '"':
                    // Escape quotes
                    sb.Append("\\\"");
                    break;
                case '\'':
                    // Escape single quotes
                    sb.Append("\\'");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return $"\"{sb}\"";
    }

    /// <summary>
    /// Escapes and quotes a generic argument value (not a path or filter graph).
    /// Used for option values that may contain special characters.
    /// </summary>
    /// <param name="value">The argument value to escape.</param>
    /// <returns>The properly escaped and quoted argument value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is empty or whitespace.</exception>
    public static string EscapeArgument(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        // For generic arguments, we quote them to handle spaces and special characters
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return EscapeArgumentWindows(value);
        }
        else
        {
            return EscapeArgumentUnix(value);
        }
    }

    /// <summary>
    /// Windows-specific path escaping using the rules from CommandLineToArgvW.
    /// </summary>
    private static string EscapePathWindows(string path)
    {
        // Windows rules:
        // 1. If path contains spaces, it must be quoted
        // 2. If path contains quotes, each quote must be escaped with backslash
        // 3. Backslashes before a quote are treated as escape characters
        // 4. Backslashes at end of path are doubled

        if (path.IndexOf(' ') < 0 && path.IndexOf('"') < 0)
        {
            // No spaces or quotes, no need to quote
            return path;
        }

        var sb = new StringBuilder(path.Length + 2);
        sb.Append('"');

        int backslashes = 0;
        foreach (char c in path)
        {
            switch (c)
            {
                case '\\':
                    backslashes++;
                    sb.Append(c);
                    break;
                case '"':
                    // Escape all backslashes before the quote
                    sb.Append('\\', backslashes * 2);
                    sb.Append("\\\"");
                    backslashes = 0;
                    break;
                default:
                    sb.Append(c);
                    backslashes = 0;
                    break;
            }
        }

        // Double trailing backslashes
        sb.Append('\\', backslashes);
        sb.Append('"');

        return sb.ToString();
    }

    /// <summary>
    /// Unix-specific path escaping.
    /// </summary>
    private static string EscapePathUnix(string path)
    {
        // Unix rules:
        // 1. If path contains spaces, it must be quoted
        // 2. Single quotes within the path are escaped by closing and reopening the quote

        if (path.IndexOf(' ') < 0 && path.IndexOf('\'') < 0)
        {
            // No spaces or quotes, no need to quote
            return path;
        }

        // Use single quotes for Unix, escape single quotes inside by closing quote, adding escaped quote, reopening
        return $"'{path.Replace("'", "'\\\\''")}'";
    }

    /// <summary>
    /// Windows-specific argument escaping.
    /// </summary>
    private static string EscapeArgumentWindows(string value)
    {
        // For Windows, use double quotes and escape internal quotes
        if (value.IndexOf('"') < 0)
        {
            // No quotes, simple case
            if (value.IndexOf(' ') < 0)
            {
                return value;
            }
            return $"\"{value}\"";
        }

        // Has quotes, need to escape them
        var sb = new StringBuilder(value.Length + 2);
        sb.Append('"');

        int backslashes = 0;
        foreach (char c in value)
        {
            switch (c)
            {
                case '\\':
                    backslashes++;
                    sb.Append(c);
                    break;
                case '"':
                    sb.Append('\\', backslashes * 2);
                    sb.Append("\\\"");
                    backslashes = 0;
                    break;
                default:
                    sb.Append(c);
                    backslashes = 0;
                    break;
            }
        }

        sb.Append('\\', backslashes);
        sb.Append('"');

        return sb.ToString();
    }

    /// <summary>
    /// Unix-specific argument escaping.
    /// </summary>
    private static string EscapeArgumentUnix(string value)
    {
        // For Unix, use single quotes and escape single quotes
        if (value.IndexOf('\'') < 0)
        {
            // No single quotes, simple case
            if (value.IndexOf(' ') < 0)
            {
                return value;
            }
            return $"'{value}'";
        }

        // Has single quotes, escape them
        return $"'{value.Replace("'", "'\\\\''")}'";
    }
}