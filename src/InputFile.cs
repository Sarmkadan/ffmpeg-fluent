namespace FFmpegFluent;

/// <summary>
/// Represents an input file for FFmpeg with fluent-style modifiers.
/// </summary>
public sealed class InputFile
{
    private readonly List<string> _options = [];
    private TimeSpan? _start;
    private TimeSpan? _duration;
    private int? _loopCount;

    /// <summary>
    /// Gets the path to the input file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputFile"/> class.
    /// </summary>
    /// <param name="path">The path to the input file.</param>
    public InputFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        Path = path;
    }

    /// <summary>
    /// Sets the start time for the input file (-ss option).
    /// </summary>
    /// <param name="start">The start time.</param>
    /// <returns>The current <see cref="InputFile"/> instance for fluent chaining.</returns>
    public InputFile Seek(TimeSpan start)
    {
        _start = start;
        return this;
    }

    /// <summary>
    /// Sets the duration for the input file (-t option).
    /// </summary>
    /// <param name="d">The duration.</param>
    /// <returns>The current <see cref="InputFile"/> instance for fluent chaining.</returns>
    public InputFile Duration(TimeSpan d)
    {
        _duration = d;
        return this;
    }

    /// <summary>
    /// Sets the number of times the input file should be looped.
    /// </summary>
    /// <param name="times">The number of loops. Use 0 for infinite looping.</param>
    /// <returns>The current <see cref="InputFile"/> instance for fluent chaining.</returns>
    public InputFile Loop(int times)
    {
        _loopCount = times;
        return this;
    }

    /// <summary>
    /// Adds a custom option for the input file.
    /// </summary>
    /// <param name="key">The option key (e.g., "re").</param>
    /// <param name="value">The optional value. If null, only the key is added.</param>
    /// <returns>The current <see cref="InputFile"/> instance for fluent chaining.</returns>
    /// <summary>
    /// Adds a custom option for the input file.
    /// </summary>
    /// <param name="key">The option key (e.g., "re").</param>
    /// <param name="value">The optional value. If null, only the key is added.</param>
    public InputFile Option(string key, string? value = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _options.Add(value is null ? $"-{key}" : $"-{key} {ArgumentEscaper.EscapeArgument(value)}");
        return this;
    }

    /// <summary>
    /// Builds the FFmpeg arguments for this input file.
    /// </summary>
    /// <returns>An enumerable of argument strings in the correct order.</returns>
    public IEnumerable<string> BuildArgs()
    {
        // Add options that come before -i
        if (_start.HasValue)
        {
            yield return $"-ss {FormatTime(_start.Value)}";
        }

        if (_loopCount.HasValue)
        {
            yield return _loopCount.Value switch
            {
                0 => "-stream_loop -1",
                _ => $"-stream_loop {_loopCount.Value}"
            };
        }

        // Add custom options
        foreach (var option in _options)
        {
            yield return option;
        }

        if (_duration.HasValue)
        {
            yield return $"-t {FormatTime(_duration.Value)}";
        }

        // The -i option with the properly escaped file path
        yield return $"-i {ArgumentEscaper.EscapePath(Path)}";
    }

    private static string FormatTime(TimeSpan time)
    {
        // Format as HH:mm:ss.ffffff
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
    }
}
