# AudioOptions

`AudioOptions` is a fluent configuration class used to specify audio encoding parameters when processing media with `ffmpeg-fluent`. It provides a chainable API for setting properties such as codec, bitrate, sample rate, channels, volume, and whether to exclude audio entirely. The configured options are compiled into command-line arguments via `BuildArgs()` for use with FFmpeg.

## API

### `AudioOptions Codec`
**Purpose**: Sets the audio codec for encoding or decoding.
**Parameters**:
- `codec` (`string`): The FFmpeg codec name (e.g., `"aac"`, `"libmp3lame"`).
**Returns**: The current `AudioOptions` instance for method chaining.
**Throws**: `ArgumentNullException` if `codec` is `null` or whitespace.

### `AudioOptions Bitrate`
**Purpose**: Sets the target bitrate for the audio stream in bits per second.
**Parameters**:
- `bitrate` (`int`): The bitrate value (e.g., `128000` for 128 kbps).
**Returns**: The current `AudioOptions` instance for method chaining.
**Throws**: `ArgumentOutOfRangeException` if `bitrate` is less than or equal to `0`.

### `AudioOptions SampleRate`
**Purpose**: Sets the sample rate for the audio stream in Hz.
**Parameters**:
- `sampleRate` (`int`): The sample rate (e.g., `44100` for 44.1 kHz).
**Returns**: The current `AudioOptions` instance for method chaining.
**Throws**: `ArgumentOutOfRangeException` if `sampleRate` is less than or equal to `0`.

### `AudioOptions Channels`
**Purpose**: Sets the number of audio channels.
**Parameters**:
- `channels` (`int`): The channel count (e.g., `2` for stereo).
**Returns**: The current `AudioOptions` instance for method chaining.
**Throws**: `ArgumentOutOfRangeException` if `channels` is less than `1`.

### `AudioOptions Volume`
**Purpose**: Adjusts the audio volume as a multiplier (e.g., `0.5` for 50% volume).
**Parameters**:
- `volume` (`double`): The volume multiplier. Must be between `0.0` and `1.0`.
**Returns**: The current `AudioOptions` instance for method chaining.
**Throws**: `ArgumentOutOfRangeException` if `volume` is outside the `[0.0, 1.0]` range.

### `AudioOptions NoAudio`
**Purpose**: Disables audio processing entirely.
**Parameters**: None.
**Returns**: The current `AudioOptions` instance for method chaining.
**Notes**: Overrides all other audio settings when set.

### `IEnumerable<string> BuildArgs()`
**Purpose**: Compiles the configured audio options into FFmpeg command-line arguments.
**Parameters**: None.
**Returns**: A sequence of FFmpeg arguments (e.g., `"-c:a aac"`, `"-b:a 128k"`).
**Notes**:
- Returns an empty sequence if `NoAudio` is set.
- Arguments are ordered logically (codec before bitrate, etc.).
- Does not validate codec compatibility with FFmpeg.

## Usage

### Example 1: Basic Audio Encoding
