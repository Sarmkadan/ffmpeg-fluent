# MediaInfoExtensions

Extension methods for extracting common media information from `MediaInfo` objects in a fluent manner. These helpers simplify access to frequently used properties like resolution, stream type checks, and bitrate calculations without requiring direct parsing of the underlying `MediaInfo` structure.

## API

### `GetResolution(MediaInfo mediaInfo)`
Extracts the resolution of the media file as a string in the format `"WxH"` (e.g., `"1920x1080"`). The resolution is derived from the first video stream found in the media file.

- **Parameters**:
  - `mediaInfo`: The `MediaInfo` object containing media metadata.
- **Return value**:
  - A string representing the resolution in `"WxH"` format, or `null` if no video stream is present.
- **Exceptions**:
  - Throws `ArgumentNullException` if `mediaInfo` is `null`.

---

### `IsVideoOnly(MediaInfo mediaInfo)`
Determines whether the media file contains only video streams and no audio or subtitle streams.

- **Parameters**:
  - `mediaInfo`: The `MediaInfo` object containing media metadata.
- **Return value**:
  - `true` if the file contains only video streams; otherwise, `false`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `mediaInfo` is `null`.

---

### `AverageBitRatePerSecond(MediaInfo mediaInfo)`
Calculates the average bitrate of the media file in bits per second. The bitrate is derived from the overall file size and duration, if available.

- **Parameters**:
  - `mediaInfo`: The `MediaInfo` object containing media metadata.
- **Return value**:
  - A `double` representing the average bitrate in bits per second. Returns `0.0` if the duration or file size is unavailable or invalid.
- **Exceptions**:
  - Throws `ArgumentNullException` if `mediaInfo` is `null`.

---
### `ToSummaryString(MediaInfo mediaInfo)`
Generates a concise summary string of the media file's key properties, including resolution, duration, and bitrate.

- **Parameters**:
  - `mediaInfo`: The `MediaInfo` object containing media metadata.
- **Return value**:
  - A string summarizing the media file's properties, or `null` if `mediaInfo` is `null` or lacks required data.
- **Exceptions**:
  - Throws `ArgumentNullException` if `mediaInfo` is `null`.

## Usage

```csharp
using FFmpeg.Fluent;
using MediaToolkit.Model;

// Example 1: Extracting resolution and checking if the file is video-only
var mediaInfo = await FFmpeg.GetMediaInfoAsync("input.mp4");
var resolution = MediaInfoExtensions.GetResolution(mediaInfo);
var isVideoOnly = MediaInfoExtensions.IsVideoOnly(mediaInfo);

Console.WriteLine($"Resolution: {resolution}, Is Video Only: {isVideoOnly}");
// Output: Resolution: 1920x1080, Is Video Only: false
```

```csharp
using FFmpeg.Fluent;
using MediaToolkit.Model;

// Example 2: Calculating average bitrate and generating a summary
var mediaInfo = await FFmpeg.GetMediaInfoAsync("input.mkv");
var bitrate = MediaInfoExtensions.AverageBitRatePerSecond(mediaInfo);
var summary = MediaInfoExtensions.ToSummaryString(mediaInfo);

Console.WriteLine($"Bitrate: {bitrate} bps");
Console.WriteLine($"Summary: {summary}");
// Output:
// Bitrate: 5000000 bps
// Summary: 1920x1080, 00:02:30, 5.0 Mbps
```

## Notes

- **Edge Cases**:
  - If `GetResolution` encounters a video stream with missing or invalid width/height, it returns `null`.
  - `AverageBitRatePerSecond` returns `0.0` if the duration or file size is missing or zero, avoiding division by zero.
  - `ToSummaryString` omits properties with missing or invalid data (e.g., duration or resolution) rather than throwing.

- **Thread Safety**:
  - All methods are thread-safe as they do not modify shared state and only read from the `MediaInfo` object. However, the `MediaInfo` object itself must be thread-safe if accessed concurrently by multiple threads.
