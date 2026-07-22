#nullable enable

using System;
using System.Collections.Generic;

namespace FFmpegFluent;

/// <summary>
/// Represents an output file for an FFmpeg command, including its path and associated video and audio options.
/// </summary>
public sealed class OutputFile
{
    /// <summary>
    /// Gets the path of the output file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets or sets the video options for the output file.
    /// </summary>
    public VideoOptions Video { get; private set; } = new();

    /// <summary>
    /// Gets or sets the audio options for the output file.
    /// </summary>
    public AudioOptions Audio { get; private set; } = new();

    private readonly List<string> _options = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputFile"/> class with the specified output path.
    /// </summary>
    /// <param name="path">The path to the output file.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or empty.</exception>
    public OutputFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        Path = path;
    }

    /// <summary>
    /// Configures the video options for this output file using the provided configuration action.
    /// </summary>
    /// <param name="cfg">An action that configures the <see cref="VideoOptions"/> instance.</param>
    /// <returns>The current <see cref="OutputFile"/> instance for method chaining.</returns>
    public OutputFile WithVideo(Action<VideoOptions> cfg)
    {
        cfg(Video);
        return this;
    }

    /// <summary>
    /// Configures the audio options for this output file using the provided configuration action.
    /// </summary>
    /// <param name="cfg">An action that configures the <see cref="AudioOptions"/> instance.</param>
    /// <returns>The current <see cref="OutputFile"/> instance for method chaining.</returns>
    public OutputFile WithAudio(Action<AudioOptions> cfg)
    {
        cfg(Audio);
        return this;
    }

    /// <summary>
    /// Sets the output format for the FFmpeg command.
    /// </summary>
    /// <param name="fmt">The format string (e.g., "mp4").</param>
    /// <returns>The current <see cref="OutputFile"/> instance for method chaining.</returns>
    public OutputFile Format(string fmt)
    {
        _options.Add($"-f {fmt}");
        return this;
    }

    /// <summary>
    /// Adds an overwrite flag to the FFmpeg command.
    /// </summary>
    /// <param name="yes">If true, adds the "-y" flag to overwrite existing files; otherwise adds "-n".</param>
    /// <returns>The current <see cref="OutputFile"/> instance for method chaining.</returns>
    public OutputFile Overwrite(bool yes = true)
    {
        _options.Add(yes ? "-y" : "-n");
        return this;
    }

    /// <summary>
    /// Adds a generic FFmpeg option to the command.
    /// </summary>
    /// <param name="key">The option key (without the leading dash).</param>
    /// <param name="value">An optional value for the option. If null, the option is added without a value.</param>
    /// <returns>The current <see cref="OutputFile"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or whitespace.</exception>
    public OutputFile Option(string key, string? value = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _options.Add(value is null ? $"-{key}" : $"-{key} {ArgumentEscaper.EscapeArgument(value)}");
        return this;
    }

    /// <summary>
    /// Adds a metadata key/value pair to the FFmpeg command, escaping the value.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current <see cref="OutputFile"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> or <paramref name="value"/> is empty or whitespace.</exception>
    public OutputFile WithMetadata(string key, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);

        var escapedValue = ArgumentEscaper.EscapeArgument(value);
        _options.Add($"-metadata {key}={escapedValue}");
        return this;
    }

    /// <summary>
    /// Adds a metadata key/value pair to the FFmpeg command.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current <see cref="OutputFile"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> or <paramref name="value"/> is empty or whitespace.</exception>
    public OutputFile Metadata(string key, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);

        var escapedValue = ArgumentEscaper.EscapeArgument(value);
        _options.Add($"-metadata {key}={escapedValue}");
        return this;
    }

    /// <summary>
    /// Builds the sequence of command line arguments for this output file, including options, video and audio arguments, and the file path.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{String}"/> containing the ordered arguments.</returns>
    public IEnumerable<string> BuildArgs()
    {
        foreach (var option in _options)
        {
            yield return option;
        }

        foreach (var videoArg in Video.BuildArgs())
        {
            yield return videoArg;
        }

        foreach (var audioArg in Audio.BuildArgs())
        {
            yield return audioArg;
        }

        yield return ArgumentEscaper.EscapePath(Path);
    }
}
