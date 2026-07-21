using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FFmpegFluent;

/// <summary>
/// Audio normalization preset using FFmpeg's loudnorm filter.
/// </summary>
public sealed class NormalizeAudioPreset
{
    private readonly string _inputPath;
    private readonly string _outputPath;
    private double _targetIntegrated = -23.0;
    private double _targetTruePeak = -1.5;
    private double _targetLra = 7.0;
    private bool _normalize = true;
    private bool _printMetadata = false;
    private string? _measurementInput;

    /// <summary>
    /// Initializes a new instance of the <see cref="NormalizeAudioPreset"/> class.
    /// </summary>
    /// <param name="inputPath">The path to the input file.</param>
    /// <param name="outputPath">The path to the output file.</param>
    public NormalizeAudioPreset(string inputPath, string outputPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(inputPath);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        _inputPath = inputPath;
        _outputPath = outputPath;
    }

    /// <summary>
    /// Sets the target integrated loudness in LUFS.
    /// </summary>
    /// <param name="lufs">The target integrated loudness in LUFS (default: -23.0).</param>
    /// <returns>The current instance of <see cref="NormalizeAudioPreset"/>.</returns>
    [Display(Name = "Target Integrated Loudness (LUFS)")]
    public NormalizeAudioPreset WithTargetIntegrated(double lufs)
    {
        _targetIntegrated = lufs;
        return this;
    }

    /// <summary>
    /// Sets the target true peak in dBTP.
    /// </summary>
    /// <param name="dbTp">The target true peak in dBTP (default: -1.5).</param>
    /// <returns>The current instance of <see cref="NormalizeAudioPreset"/>.</returns>
    [Display(Name = "Target True Peak (dBTP)")]
    public NormalizeAudioPreset WithTargetTruePeak(double dbTp)
    {
        _targetTruePeak = dbTp;
        return this;
    }

    /// <summary>
    /// Sets the target loudness range in LU.
    /// </summary>
    /// <param name="lra">The target loudness range in LU (default: 7.0).</param>
    /// <returns>The current instance of <see cref="NormalizeAudioPreset"/>.</returns>
    [Display(Name = "Target Loudness Range (LU)")]
    public NormalizeAudioPreset WithTargetLra(double lra)
    {
        _targetLra = lra;
        return this;
    }

    /// <summary>
    /// Sets whether to normalize to the specified targets.
    /// </summary>
    /// <param name="normalize">Whether to normalize (default: true).</param>
    /// <returns>The current instance of <see cref="NormalizeAudioPreset"/>.</returns>
    [Display(Name = "Normalize")]
    public NormalizeAudioPreset WithNormalize(bool normalize)
    {
        _normalize = normalize;
        return this;
    }

    /// <summary>
    /// Sets whether to print metadata only and exit.
    /// </summary>
    /// <param name="printMetadata">Whether to print metadata only (default: false).</param>
    /// <returns>The current instance of <see cref="NormalizeAudioPreset"/>.</returns>
    [Display(Name = "Print Metadata Only")]
    public NormalizeAudioPreset WithPrintMetadata(bool printMetadata)
    {
        _printMetadata = printMetadata;
        return this;
    }

    /// <summary>
    /// Sets the optional measurement input file for loudnorm filter.
    /// </summary>
    /// <param name="measurementInput">The measurement input file path.</param>
    /// <returns>The current instance of <see cref="NormalizeAudioPreset"/>.</returns>
    [Display(Name = "Measurement Input File")]
    public NormalizeAudioPreset WithMeasurementInput(string? measurementInput)
    {
        _measurementInput = measurementInput;
        return this;
    }

    /// <summary>
    /// Builds the arguments for the FFmpeg command.
    /// </summary>
    /// <returns>An array of arguments for the FFmpeg command.</returns>
    public string[] BuildArguments()
    {
        var args = new List<string>();
        args.Add("-y");
        args.Add("-i");
        args.Add(_inputPath);

        if (_measurementInput != null)
        {
            args.Add("-f");
            args.Add("lavfi");
            args.Add("-i");
            args.Add($"amovie={_measurementInput},astats=metadata=1:reset=1");
        }

        if (_normalize && !_printMetadata)
        {
            args.Add("-filter_complex");
            var loudnormArgs = new List<string>
            {
                "loudnorm=I=" + _targetIntegrated.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "print_format=json"
            };

            if (_targetTruePeak != -1.5)
            {
                loudnormArgs.Add("TP=" + _targetTruePeak.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (_targetLra != 7.0)
            {
                loudnormArgs.Add("LRA=" + _targetLra.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (_measurementInput != null)
            {
                loudnormArgs.Add("measured_I=0");
                loudnormArgs.Add("measured_TP=0");
                loudnormArgs.Add("measured_LRA=0");
                loudnormArgs.Add("measured_thresh=0");
            }

            args.Add(string.Join(":", loudnormArgs));
            args.Add("-f");
            args.Add("null");
            args.Add("-");
        }
        else if (_printMetadata)
        {
            args.Add("-af");
            args.Add("astats=metadata=1:reset=1");
            args.Add("-f");
            args.Add("lavfi");
            args.Add("-");
        }

        args.Add(_outputPath);

        return args.ToArray();
    }

    /// <summary>
    /// Runs the FFmpeg command to normalize the audio.
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

        await process.WaitForExitAsync(ct).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
            throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}. Error: {error}");
        }
    }

    public override string ToString() =>
        $"normalize-audio(target_integrated={_targetIntegrated},target_true_peak={_targetTruePeak},target_lra={_targetLra},normalize={_normalize},print_metadata={_printMetadata}{(string.IsNullOrEmpty(_measurementInput) ? "" : $",measurement_input=" + _measurementInput)}";
}