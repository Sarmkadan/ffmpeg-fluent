# MediaInfo

`MediaInfo` is an immutable snapshot of the technical metadata for a media file, obtained by probing the file with FFmpeg. It provides essential stream properties such as duration, codec names, resolution, frame rate, bit rate, and audio parameters. Instances are created exclusively through the static factory method `ProbeAsync`, which invokes `ffprobe` under the hood and returns a fully populated object.

## API

### `public static async Task<MediaInfo> ProbeAsync(string filePath)`

Asynchronously probes a media file and returns a `MediaInfo` instance containing its technical metadata.

- **Parameters**
  - `filePath` (`string`): The absolute or relative path to the media file. Must not be `null` or empty.
- **Returns**
  - `Task<MediaInfo>`: A task that, when awaited, yields a `MediaInfo` object with all available metadata fields populated. Fields for which no information is available will be set to their default values (e.g., `null` for strings, `0` for numeric types).
- **Exceptions**
  - `ArgumentNullException`: Thrown when `filePath` is `null`.
  - `ArgumentException`: Thrown when `filePath` is empty or consists only of whitespace.
  - `FileNotFoundException`: Thrown when the specified file does not exist.
  - `InvalidOperationException`: Thrown when FFmpeg is not available on the system or the probe process fails to start.
  - `MediaProbeException`: Thrown when `ffprobe` exits with a non-zero code, indicating the file is corrupt, not a recognized media format, or otherwise unreadable.

### `public TimeSpan Duration`

The total playback duration of the media file. If the duration cannot be determined (e.g., for live streams or certain raw formats), this property is `TimeSpan.Zero`.

### `public long BitRate`

The overall bit rate of the media file in bits per second. A value of `0` indicates the bit rate was not reported by the probe, which is common for variable-bit-rate content or formats that do not store this metadata at the container level.

### `public string? VideoCodec`

The name of the video codec as reported by FFmpeg (e.g., `"h264"`, `"hevc"`, `"vp9"`). Returns `null` if the file contains no video stream.

### `public string? AudioCodec`

The name of the audio codec as reported by FFmpeg (e.g., `"aac"`, `"mp3"`, `"opus"`). Returns `null` if the file contains no audio stream.

### `public int Width`

The width of the primary video stream in pixels. Returns `0` if no video stream is present.

### `public int Height`

The height of the primary video stream in pixels. Returns `0` if no video stream is present.

### `public double FrameRate`

The frame rate of the primary video stream as a floating-point value (e.g., `23.976`, `29.97`, `60.0`). Returns `0.0` if no video stream is present or the frame rate is variable/unknown.

### `public int AudioChannels`

The number of audio channels in the primary audio stream (e.g., `2` for stereo, `6` for 5.1 surround). Returns `0` if no audio stream is present.

### `public int SampleRate`

The sample rate of the primary audio stream in Hertz (e.g., `44100`, `48000`). Returns `0` if no audio stream is present.

### `public string FormatName`

The name of the container format as reported by FFmpeg (e.g., `"mov,mp4,m4a,3gp,3g2,mj2"`, `"matroska,webm"`, `"avi"`). This property is always populated for a successfully probed file.

## Usage

### Example 1: Basic probing and display

```csharp
using FFMpegFluent;

async Task DisplayMediaInfo(string filePath)
{
    MediaInfo info = await MediaInfo.ProbeAsync(filePath);

    Console.WriteLine($"Format: {info.FormatName}");
    Console.WriteLine($"Duration: {info.Duration:hh\\:mm\\:ss}");

    if (info.VideoCodec is not null)
    {
        Console.WriteLine($"Video: {info.VideoCodec} {info.Width}x{info.Height} @ {info.FrameRate:F2} fps");
    }
    else
    {
        Console.WriteLine("No video stream");
    }

    if (info.AudioCodec is not null)
    {
        Console.WriteLine($"Audio: {info.AudioCodec} {info.AudioChannels}ch {info.SampleRate}Hz");
    }
    else
    {
        Console.WriteLine("No audio stream");
    }
}
```

### Example 2: Conditional processing based on codec

```csharp
using FFMpegFluent;

async Task ProcessIfCompatible(string filePath)
{
    MediaInfo info = await MediaInfo.ProbeAsync(filePath);

    if (info.VideoCodec is "h264" or "hevc" &&
        info.Width <= 1920 &&
        info.Height <= 1080 &&
        info.FrameRate <= 30.0)
    {
        // Proceed with a pipeline that expects HD H.264/H.265 content
        Console.WriteLine("File is compatible for processing.");
    }
    else
    {
        Console.WriteLine("File does not meet compatibility requirements.");
    }
}
```

## Notes

- **Thread safety**: `MediaInfo` instances are immutable and safe to share across threads without synchronization. The static `ProbeAsync` method is thread-safe and may be called concurrently from multiple threads; each invocation spawns an independent `ffprobe` process.
- **Edge cases**: For files containing multiple streams of the same type, the properties reflect the *primary* stream (typically the first video or audio stream encountered). Additional streams are not represented in this type.
- **Variable frame rate**: If the video stream has a variable frame rate, `FrameRate` may return `0.0` or an average value depending on the FFmpeg version and container metadata. Do not rely on `FrameRate` for frame-accurate seeking with VFR content.
- **Zero values**: A value of `0` for numeric properties or `null` for codec strings does not necessarily indicate an error; it means the probe did not report that attribute. Always null-check `VideoCodec` and `AudioCodec` before using them to determine stream presence.
- **FFmpeg dependency**: `ProbeAsync` requires a functional FFmpeg installation discoverable on the system PATH. If FFmpeg is missing, an `InvalidOperationException` is thrown. Ensure the deployment environment includes `ffprobe`.
