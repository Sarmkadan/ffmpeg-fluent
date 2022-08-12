# VideoOptionsExtensions

Provides extension methods for configuring video-related options in FFmpeg pipelines. These methods allow fluent configuration of video codec, bitrate, resolution, frame rate, and quality settings while maintaining immutability of the underlying `VideoOptions` instance.

## API

### `WithCodecAndBitrate`

Configures the video stream with a specified codec and bitrate. The codec must be supported by the FFmpeg build in use.

- **Parameters**
  - `codec` (string): The video codec identifier (e.g., `"libx264"`, `"h264_nvenc"`).
  - `bitrate` (long): The target bitrate in bits per second (e.g., `5000000` for 5 Mbps).

- **Return Value**
  Returns a new `VideoOptions` instance with the updated codec and bitrate settings.

- **Exceptions**
  Throws `ArgumentNullException` if `codec` is `null`.
  Throws `ArgumentOutOfRangeException` if `bitrate` is negative.

---

### `WithResolutionAndFrameRate`

Sets the resolution and frame rate for the video stream. Both parameters must be positive values.

- **Parameters**
  - `width` (int): The width of the video in pixels.
  - `height` (int): The height of the video in pixels.
  - `frameRate` (double): The target frame rate in frames per second (e.g., `30.0`, `60.0`).

- **Return Value**
  Returns a new `VideoOptions` instance with the updated resolution and frame rate.

- **Exceptions**
  Throws `ArgumentOutOfRangeException` if `width`, `height`, or `frameRate` is not a positive number.

---

### `WithHighQuality`

Applies high-quality preset settings to the video stream. This typically includes slower encoding with better compression efficiency.

- **Return Value**
  Returns a new `VideoOptions` instance with quality-focused settings applied.

- **Exceptions**
  None.

---
### `DisableVideo`

Removes the video stream from the FFmpeg pipeline entirely. This is useful for audio-only outputs or when video processing is not required.

- **Return Value**
  Returns a new `VideoOptions` instance with video disabled.

- **Exceptions**
  None.

## Usage
