namespace FFmpegFluent;

public sealed class ExtractAudioPreset
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private string? _codec;
    private int? _bitrate;
    private int? _streamIndex;
    private bool _copyStream;

    public ExtractAudioPreset(string inputPath, string outputPath)
    {
        _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
        _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
    }

    public ExtractAudioPreset WithCodec(string codec)
    {
        _codec = codec ?? throw new ArgumentNullException(nameof(codec));
        return this;
    }

    public ExtractAudioPreset WithBitrate(int kbps)
    {
        if (kbps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kbps), "Bitrate must be positive");
        }

        _bitrate = kbps;
        return this;
    }

    public ExtractAudioPreset CopyStream()
    {
        _copyStream = true;
        return this;
    }

    public ExtractAudioPreset StreamIndex(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Stream index must be non-negative");
        }

        _streamIndex = index;
        return this;
    }

    public string[] BuildArguments()
    {
        var args = new List<string>();
        args.Add("-i");
        args.Add(_inputPath);

        if (_copyStream)
        {
            args.Add("-acodec");
            args.Add("copy");
        }
        else if (_codec != null)
        {
            args.Add("-acodec");
            args.Add(_codec);
        }

        if (_bitrate != null)
        {
            args.Add("-b:a");
            args.Add($"{_bitrate}k");
        }

        if (_streamIndex != null)
        {
            args.Add("-map");
            args.Add($"0:{_streamIndex}");
        }
        else
        {
            args.Add("-map");
            args.Add("0:a");
        }

        args.Add("-y");
        args.Add(_outputPath);

        return args.ToArray();
    }

    public async Task RunAsync(string ffmpegPath = "ffmpeg", CancellationToken ct = default)
    {
        var arguments = BuildArguments();
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = string.Join(" ", arguments.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg)),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}. Error: {error}");
        }
    }
}
