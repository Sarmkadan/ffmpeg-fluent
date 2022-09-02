namespace FFmpegFluent;

/// <summary>
/// Provides extension methods for <see cref="InputFile"/> to enhance fluent API capabilities.
/// </summary>
public static class InputFileExtensions
{
    /// <summary>
    /// Sets the start time for the input file using a time string format (HH:mm:ss.fff).
    /// </summary>
    /// <param name="inputFile">The input file instance.</param>
    /// <param name="timeString">The start time as a string in format HH:mm:ss.fff.</param>
    /// <returns>The input file instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="timeString"/> is null or empty.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="timeString"/> has an invalid format.</exception>
    public static InputFile Seek(this InputFile inputFile, string timeString)
    {
        ArgumentException.ThrowIfNullOrEmpty(timeString);

        var timeParts = timeString.Split(':', '.', StringSplitOptions.RemoveEmptyEntries);
        if (timeParts.Length < 2 || timeParts.Length > 3)
        {
            throw new ArgumentException("Time string must be in format HH:mm:ss.fff", nameof(timeString));
        }

        if (!int.TryParse(timeParts[0], out int hours) ||
            !int.TryParse(timeParts[1], out int minutes))
        {
            throw new FormatException("Hours and minutes must be valid integers");
        }

        string[] secondsParts = timeParts[2].Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (!int.TryParse(secondsParts[0], out int seconds))
        {
            throw new FormatException("Seconds must be a valid integer");
        }

        int milliseconds = secondsParts.Length > 1 && int.TryParse(secondsParts[1], out var ms) ? ms : 0;

        var startTime = new TimeSpan(hours, minutes, seconds).Add(TimeSpan.FromMilliseconds(milliseconds));
        return inputFile.Seek(startTime);
    }

    /// <summary>
    /// Sets the duration for the input file using a time string format (HH:mm:ss.fff).
    /// </summary>
    /// <param name="inputFile">The input file instance.</param>
    /// <param name="timeString">The duration as a string in format HH:mm:ss.fff.</param>
    /// <returns>The input file instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="timeString"/> is null or empty.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="timeString"/> has an invalid format.</exception>
    public static InputFile Duration(this InputFile inputFile, string timeString)
    {
        ArgumentException.ThrowIfNullOrEmpty(timeString);

        var timeParts = timeString.Split(':', '.', StringSplitOptions.RemoveEmptyEntries);
        if (timeParts.Length < 2 || timeParts.Length > 3)
        {
            throw new ArgumentException("Time string must be in format HH:mm:ss.fff", nameof(timeString));
        }

        if (!int.TryParse(timeParts[0], out int hours) ||
            !int.TryParse(timeParts[1], out int minutes))
        {
            throw new FormatException("Hours and minutes must be valid integers");
        }

        string[] secondsParts = timeParts[2].Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (!int.TryParse(secondsParts[0], out int seconds))
        {
            throw new FormatException("Seconds must be a valid integer");
        }

        int milliseconds = secondsParts.Length > 1 && int.TryParse(secondsParts[1], out var ms) ? ms : 0;

        var duration = new TimeSpan(hours, minutes, seconds).Add(TimeSpan.FromMilliseconds(milliseconds));
        return inputFile.Duration(duration);
    }

    /// <summary>
    /// Adds multiple custom options for the input file at once.
    /// </summary>
    /// <param name="inputFile">The input file instance.</param>
    /// <param name="options">Collection of key-value pairs representing options.</param>
    /// <returns>The input file instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static InputFile Options(this InputFile inputFile, IEnumerable<KeyValuePair<string, string?>> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        foreach (var option in options)
        {
            inputFile = inputFile.Option(option.Key, option.Value);
        }

        return inputFile;
    }

    /// <summary>
    /// Adds multiple custom options for the input file at once using string tuples.
    /// </summary>
    /// <param name="inputFile">The input file instance.</param>
    /// <param name="options">Collection of option tuples (key, value).</param>
    /// <returns>The input file instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static InputFile Options(this InputFile inputFile, params (string Key, string? Value)[] options)
    {
        ArgumentNullException.ThrowIfNull(options);

        foreach (var option in options)
        {
            inputFile = inputFile.Option(option.Key, option.Value);
        }

        return inputFile;
    }
}