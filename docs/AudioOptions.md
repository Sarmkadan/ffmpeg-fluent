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
- `volume` (`double`): The volume multiplier. Must be non-negative; values above `1.0` boost the signal.
**Returns**: The current `AudioOptions` instance for method chaining.
**Throws**: `ArgumentOutOfRangeException` if `volume` is negative. `NaN` and infinity are rejected by `Validate()`.

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
- Does not validate the options; use `Validate()` for that.

### `IReadOnlyList<string> Validate()` / `bool IsValid()` / `void EnsureValid()`
**Purpose**: Checks the configuration without launching FFmpeg. `Validate()` returns every problem found; `EnsureValid()` throws `FFmpegOptionsValidationException` carrying the same list in its `Errors` property. `FFmpegCommand.RunAsync` runs this check for every output (video and audio) before the process starts.

Rules (ignored entirely when `NoAudio` is set):
- Codec: must be a known encoder name, case-insensitive (for example `aac`, `libmp3lame`, `libopus`, `flac`, `copy`, or any `pcm_*`/`adpcm_*` encoder). Unknown names are rejected.
- Sample rate: must be one of the standard rates. Without a codec the set is 8000-192000 Hz (`8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100, 48000, 64000, 88200, 96000, 176400, 192000`). Codec subsets:
  - `aac`, `libfdk_aac`: the MPEG-4 AAC table, which also includes `7350`.
  - `libmp3lame`: `8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100, 48000`.
  - `libopus`, `opus`: `8000, 12000, 16000, 24000, 48000`.
- Channels: 1-8.
- Volume: finite and non-negative (rejects `NaN` and infinity).
- Bitrate: `<number>[k|K|M|G]` with an optional decimal part (e.g. `128k`, `1.5M`, `128000`), greater than zero. Ambiguous forms such as `2m` or `2MB` are rejected.
- Bitrate range: `libmp3lame` 32k-320k, `aac`/`libfdk_aac` 32k-512k. Other codecs are not range-checked.

**Returns:**
`IReadOnlyList<string>` from `Validate()` (empty if valid); `bool` from `IsValid()`; `void` from `EnsureValid()`.

**Throws:**
`FFmpegOptionsValidationException` - From `EnsureValid()` and `RunAsync`, when any rule fails.

## Usage

### Example 1: Basic Audio Encoding
