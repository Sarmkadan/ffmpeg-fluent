#nullable enable

using System;
using System.Collections.Generic;

namespace FFmpegFluent;

public sealed class OutputFile
{
    public string Path { get; }

    public VideoOptions Video { get; private set; } = new();

    public AudioOptions Audio { get; private set; } = new();

    private readonly List<string> _options = [];

    public OutputFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        Path = path;
    }

    public OutputFile WithVideo(Action<VideoOptions> cfg)
    {
        cfg(Video);
        return this;
    }

    public OutputFile WithAudio(Action<AudioOptions> cfg)
    {
        cfg(Audio);
        return this;
    }

    public OutputFile Format(string fmt)
    {
        _options.Add($"-f {fmt}");
        return this;
    }

    public OutputFile Overwrite(bool yes = true)
    {
        _options.Add(yes ? "-y" : "-n");
        return this;
    }

    public OutputFile Option(string key, string? value = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _options.Add(value is null ? $"-{key}" : $"-{key} {value}");
        return this;
    }

    public OutputFile Metadata(string key, string value)
    {
        _options.Add($"-metadata {key}={value}");
        return this;
    }

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

        yield return Path;
    }
}
