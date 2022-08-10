#nullable enable

using System;
using System.Collections.Generic;

namespace FFmpegFluent;

/// <summary>
/// Provides extension methods for <see cref="OutputFile"/> to simplify common FFmpeg operations.
/// </summary>
public static class OutputFileExtensions
{
    /// <summary>
    /// Sets the output format to MP4 container.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile AsMp4(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        return outputFile.Format("mp4");
    }

    /// <summary>
    /// Sets the output format to MKV container.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile AsMkv(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        return outputFile.Format("matroska");
    }

    /// <summary>
    /// Sets the output format to WebM container.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile AsWebM(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        return outputFile.Format("webm");
    }

    /// <summary>
    /// Sets the output format to MOV container.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile AsMov(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        return outputFile.Format("mov");
    }

    /// <summary>
    /// Configures the video codec to H.264 (libx264).
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithH264Codec(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        outputFile.Video.Codec("libx264");
        return outputFile;
    }

    /// <summary>
    /// Configures the video codec to H.265/HEVC (libx265).
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithH265Codec(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        outputFile.Video.Codec("libx265");
        return outputFile;
    }

    /// <summary>
    /// Configures the video codec to VP9.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithVp9Codec(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        outputFile.Video.Codec("libvpx-vp9");
        return outputFile;
    }

    /// <summary>
    /// Configures the audio codec to AAC.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithAacAudio(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        outputFile.Audio.Codec("aac");
        return outputFile;
    }

    /// <summary>
    /// Configures the audio codec to MP3.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithMp3Audio(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        outputFile.Audio.Codec("libmp3lame");
        return outputFile;
    }

    /// <summary>
    /// Configures the audio codec to Opus.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithOpusAudio(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        outputFile.Audio.Codec("libopus");
        return outputFile;
    }

    /// <summary>
    /// Sets the video bitrate to the specified value.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <param name="bitrate">The bitrate value (e.g., "5000k" for 5000 kbps).</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithVideoBitrate(this OutputFile outputFile, string bitrate)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        ArgumentException.ThrowIfNullOrEmpty(bitrate);
        outputFile.Video.Bitrate(bitrate);
        return outputFile;
    }

    /// <summary>
    /// Sets the audio bitrate to the specified value.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <param name="bitrate">The bitrate value (e.g., "192k" for 192 kbps).</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithAudioBitrate(this OutputFile outputFile, string bitrate)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        ArgumentException.ThrowIfNullOrEmpty(bitrate);
        outputFile.Audio.Bitrate(bitrate);
        return outputFile;
    }

    /// <summary>
    /// Sets the frame rate to the specified value.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <param name="fps">The frames per second value (e.g., 30.0, 60.0).</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithFrameRate(this OutputFile outputFile, double fps)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        outputFile.Video.FrameRate(fps);
        return outputFile;
    }

    /// <summary>
    /// Sets the resolution to the specified value.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithResolution(this OutputFile outputFile, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        outputFile.Video.Resolution(width, height);
        return outputFile;
    }

    /// <summary>
    /// Adds a title metadata tag to the output file.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <param name="title">The title text.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithTitle(this OutputFile outputFile, string title)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        ArgumentException.ThrowIfNullOrEmpty(title);
        return outputFile.Metadata("title", title);
    }

    /// <summary>
    /// Adds an author metadata tag to the output file.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <param name="author">The author name.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithAuthor(this OutputFile outputFile, string author)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        ArgumentException.ThrowIfNullOrEmpty(author);
        return outputFile.Metadata("artist", author);
    }

    /// <summary>
    /// Adds a copyright metadata tag to the output file.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <param name="copyright">The copyright text.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile WithCopyright(this OutputFile outputFile, string copyright)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        ArgumentException.ThrowIfNullOrEmpty(copyright);
        return outputFile.Metadata("copyright", copyright);
    }

    /// <summary>
    /// Sets the output file to overwrite existing files without prompting.
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile ForceOverwrite(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        return outputFile.Overwrite(true);
    }

    /// <summary>
    /// Sets the output file to not overwrite existing files (default behavior).
    /// </summary>
    /// <param name="outputFile">The output file instance.</param>
    /// <returns>The output file instance for method chaining.</returns>
    public static OutputFile PreserveExisting(this OutputFile outputFile)
    {
        ArgumentNullException.ThrowIfNull(outputFile);
        return outputFile.Overwrite(false);
    }
}