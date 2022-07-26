namespace FFmpegFluent;

/// <summary>
/// Represents a preset for extracting audio from a video file using FFmpeg.
/// </summary>
public sealed class ExtractAudioPreset
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private string? _codec;
    private int? _bitrate;
    private int? _streamIndex;
    private bool _copyStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractAudioPreset"/> class.
    /// </summary>
    /// <param name="inputPath">The path to the input video file.</param>
    /// <param name="outputPath">The path to the output audio file.</param>
    public ExtractAudioPreset(string inputPath, string outputPath)
    {
        _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
        _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
    }

    /// <summary>
    /// Configures the audio codec to use for extraction.
    /// </summary>
    /// <param name="codec">The audio codec to use.</param>
    /// <returns>The current instance of <see cref="ExtractAudioPreset"/>.</returns>
    public ExtractAudioPreset WithCodec(string codec)
    {
        _codec = codec ?? throw new ArgumentNullException(nameof(codec));
        return this;
    }

    /// <summary>
    /// Configures the bitrate of the extracted audio.
    /// </summary>
    /// <param name="kbps">The bitrate in kilobits per second.</param>
    /// <returns>The current instance of <see cref="ExtractAudioPreset"/>.</returns>
    public ExtractAudioPreset WithBitrate(int kbps)
    {
        if (kbps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kbps), "Bitrate must be positive");
        }

        _bitrate = kbps;
        return this;
    }

    /// <summary>
    /// Configures the extraction to copy the audio stream instead of re-encoding it.
    /// </summary>
    /// <returns>The current instance of <see cref="ExtractAudioPreset"/>.</returns>
    public ExtractAudioPreset CopyStream()
    {
        _copyStream = true;
        return this;
    }

    /// <summary>
    /// Configures the stream index of the audio to extract.
    /// </summary>
    /// <param name="index">The stream index of the audio to extract.</param>
    /// <returns>The current instance of <see cref="ExtractAudioPreset"/>.</returns>
    public ExtractAudioPreset StreamIndex(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Stream index must be non-negative");
        }

        _streamIndex = index;
        return this;
    }

    /// <summary>
    /// Builds the FFmpeg arguments for extracting the audio.
    /// </summary>
    /// <returns>An array of FFmpeg arguments.</returns>
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

    /// <summary>
    /// Runs the FFmpeg command to extract the audio.
    /// </summary>
    /// <param name="ffmpegPath">The path to the FFmpeg executable.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
