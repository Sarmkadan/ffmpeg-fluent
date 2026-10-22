# ffmpeg-fluent

## Getting Started

### Installation

```bash
dotnet add package FFmpegFluent
```

The library shells out to `ffmpeg` and `ffprobe`, so both must be installed and reachable on your
machine (see [Locating ffmpeg](#locating-ffmpeg)). Everything else is BCL only.

### Minimal transcode

```csharp
using FFmpegFluent;

await FFmpegCommand.Create()
    .AddInput("input.mov")
    .AddOutput("output.mp4", o => o.Overwrite())
    .RunAsync();
```

`Overwrite()` passes `-y`. Without it, ffmpeg stops to ask before replacing an existing
`output.mp4` and waits on stdin, which the library never answers, so `RunAsync` hangs.

### Transcode with video and audio options

```csharp
using FFmpegFluent;

await FFmpegCommand.Create()
    .AddInput("input.mov")
    .AddOutput("output.mp4", o => o
        .Overwrite()
        .WithVideo(v => v
            .Codec("libx264")
            .Resolution(1280, 720)   // emits -vf scale=1280:720 (does not keep aspect ratio)
            .Crf(23)                 // 0-51 for libx264/libx265, lower is higher quality
            .Preset("medium"))
        .WithAudio(a => a
            .Codec("aac")
            .Bitrate("128k")))
    .RunAsync();
```

### Running asynchronously with cancellation

`RunAsync` returns `Task<int>` with the process exit code. A non-zero exit code throws
`FFmpegException` (with `ExitCode`, `StdErr` and the command line). Cancelling the token kills the
ffmpeg process tree and throws `OperationCanceledException`.

```csharp
using FFmpegFluent;

using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

try
{
    await FFmpegCommand.Create()
        .AddInput("input.mov")
        .AddOutput("output.mp4", o => o
            .Overwrite()
            .WithVideo(v => v.Crf(23).Preset("fast")))
        .RunAsync(ct: cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Transcode cancelled.");
}
catch (FFmpegException ex)
{
    Console.WriteLine($"ffmpeg exited with {ex.ExitCode}:{Environment.NewLine}{ex.StdErr}");
}
```

To cap the run time without a token, use `RunAsync(TimeSpan.FromMinutes(10), ct)`. This throws
`TimeoutException` when the limit elapses.

### Locating ffmpeg

`FFmpegCommand.Create()` uses `FFmpegLocator.Instance`, a process-wide singleton. The first time
the path is needed (for example, inside `RunAsync`), it resolves `ffmpeg` in this order:

1. **Explicit path**: the first argument to `new FFmpegLocator(ffmpegPath, ffprobePath)`. Only
   applies when you construct the locator yourself.
2. **`FFMPEG_PATH` environment variable**: must be the path to the `ffmpeg` executable file, not a
   directory.
3. **`PATH`**: each entry is checked for `ffmpeg` (`ffmpeg.exe` on Windows).

The locator does not probe common install directories such as `/usr/local/bin`,
`/opt/homebrew/bin`, or `C:\Program Files`. If your install is not on `PATH`, use step 1 or 2.

`ffprobe` is resolved separately, in this order: the directory of the explicit `ffmpeg` path, then
the directory of `FFMPEG_PATH`, then `PATH`.

The default lookup checks that the file exists, then runs `ffmpeg -version` to verify it. The
resolved path and version are cached after the first successful lookup, so changing `FFMPEG_PATH`
later in the same process has no effect. It also does not fall through: if `FFMPEG_PATH` points at
a broken binary, the lookup fails there and `PATH` is never tried.

### Platform notes

**Linux.** Executables are `ffmpeg` and `ffprobe`. Package installs such as
`sudo apt install ffmpeg` place them in `/usr/bin`, which is normally on `PATH`, so no
configuration is needed.

**macOS.** Same executable names and `PATH` behavior as Linux. Homebrew installs to
`/opt/homebrew/bin` on Apple Silicon and `/usr/local/bin` on Intel. Those directories are searched
only if they are on the `PATH` of the process running your code. Apps started from Finder or an IDE
may not inherit your shell's `PATH`; set `FFMPEG_PATH` in that case.

**Windows.** Executables are `ffmpeg.exe` and `ffprobe.exe`. `FFMPEG_PATH` must include the `.exe`
extension. **Known issue:** the `PATH` probe does not find `ffmpeg.exe` or `ffprobe.exe` on a
standard Windows install, because it appends each `PATHEXT` extension to a name that already ends
in `.exe` (for example `ffmpeg.exe.EXE`). Until this is fixed, use `FFMPEG_PATH` or an explicit
path on Windows.

### Custom paths

Set the environment variable for a shell session or a single run:

```bash
export FFMPEG_PATH=/opt/ffmpeg/bin/ffmpeg
dotnet run
```

On Windows PowerShell:

```powershell
$env:FFMPEG_PATH = "C:\ffmpeg\bin\ffmpeg.exe"
dotnet run
```

Or pass the paths in code. The constructor throws `ArgumentException` if either path is null or
whitespace, and `FileNotFoundException` if either file does not exist. Keep `ffprobe` in the same
directory as `ffmpeg`: the second argument is only checked for existence, and `ffprobe` is resolved
from the `ffmpeg` directory.

```csharp
using FFmpegFluent;

var locator = new FFmpegLocator("/opt/ffmpeg/bin/ffmpeg", "/opt/ffmpeg/bin/ffprobe");

await FFmpegCommand.Create(locator)
    .AddInput("input.mov")
    .AddOutput("output.mp4", o => o.Overwrite())
    .RunAsync();
```

### When ffmpeg is not found

`FFmpegPath`, `FFprobePath`, and `RunAsync` throw `FFmpegNotFoundException`. Its
`SearchedLocations` property lists what was tried. This type derives from `Exception`, not
`InvalidOperationException`, despite what some `IFFmpegLocator` XML docs say.

`TryLocate` returns `false` instead of throwing. It only checks that the `ffmpeg` file exists, and
it does not look up `ffprobe`. `LocateAsync` throws the same exception, but it also runs
`-version` on each candidate and moves on when one fails. `FFmpegCommand` does not use
`LocateAsync`.

```csharp
using FFmpegFluent;

var locator = new FFmpegLocator();
if (!locator.TryLocate(out var path))
{
    Console.Error.WriteLine("ffmpeg not found. Install it, or set FFMPEG_PATH.");
    return;
}

Console.WriteLine($"Using ffmpeg at {path}");
```

To see the full list of searched locations, catch the exception:

```csharp
using FFmpegFluent;

try
{
    await FFmpegCommand.Create()
        .AddInput("input.mov")
        .AddOutput("output.mp4", o => o.Overwrite())
        .RunAsync();
}
catch (FFmpegNotFoundException ex)
{
    Console.Error.WriteLine(ex.Message);
    foreach (var location in ex.SearchedLocations)
    {
        Console.Error.WriteLine($"  searched: {location}");
    }
}
```

### Troubleshooting

- **Wrong architecture** (for example, `Exec format error`, or an x64 binary on an ARM64 machine):
  the run fails with `InvalidOperationException: Failed to verify FFmpeg executable at '...'`, with
  the OS error as the inner exception. Check the machine with `uname -m` and the binary with
  `file "$(which ffmpeg)"` on Linux or macOS, then install a build that matches.
- **Missing execute permission** (`Permission denied` in the inner exception): the file exists, so
  lookup succeeds, but running it fails with the same `InvalidOperationException`. Fix it with
  `chmod +x /path/to/ffmpeg`.
- **Docker images**:
  - If the image has no ffmpeg, you get `FFmpegNotFoundException`. Install it in the image, for
    example `apt-get install -y ffmpeg` on Debian or Ubuntu, or `apk add ffmpeg` on Alpine.
  - A host binary mounted into the container often fails with a wrong architecture, missing shared
    libraries, or a glibc/musl mismatch. Prefer the package installed inside the image.
  - If ffmpeg is installed outside `PATH`, set `FFMPEG_PATH` in the image:

    ```dockerfile
    RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*
    ENV FFMPEG_PATH=/usr/bin/ffmpeg
    ```

- **A broken `FFMPEG_PATH` blocks the fallback**: the default lookup does not try `PATH` after a
  failing candidate. Unset `FFMPEG_PATH` to fall back to `PATH`.
- **`ffprobe` not found while `ffmpeg` is**: keep `ffprobe` in the same directory as `ffmpeg`, or
  on `PATH`. The explicit `ffprobe` argument does not affect lookup.

## Architecture

FFmpegFluent is a fluent C# DSL that shells out to the `ffmpeg`/`ffprobe` CLI - no native
bindings, no dependencies beyond the BCL. It has two layers: a composable core
(`FFmpegCommand` + `InputFile`/`OutputFile`/`FilterGraph` with progress reporting via
`FFmpegProgress`) and self-contained presets for common jobs (concat, GIF, thumbnails,
audio extraction, watermarking, media probing). See
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full component breakdown, data flow,
and known limitations.

## HardwareAccelOptions
The `HardwareAccelOptions` type allows you to configure hardware acceleration options for FFmpeg. It provides a way to specify the hardware acceleration kind, device, and input arguments.

## FFmpegProgress
The `FFmpegProgress` type represents the progress information reported by FFmpeg during a conversion. It exposes the amount of processed time, frames per second, bitrate, frame count, and speed multiplier, and can be parsed from FFmpeg’s progress output.

Example usage:

## FFmpegException
The `FFmpegException` type represents a failure raised when an FFmpeg process exits with an error. It exposes the process exit code, the captured standard error output, and the full command line that was executed, making failed invocations easy to diagnose and log.

Example usage:
```csharp
var preset = new SubtitlePreset("input.mp4", "output-with-subtitles.mp4")
    .WithSubtitle("subtitles.srt");

try
{
    await preset.RunAsync();
}
catch (FFmpegException ex)
{
    Console.WriteLine($"FFmpeg failed with exit code {ex.ExitCode}.");
    Console.WriteLine($"Command line: {ex.CommandLine}");
    Console.WriteLine($"Standard error:{Environment.NewLine}{ex.StdErr}");
}
```

## ExtractAudioPresetExtensions
The `ExtractAudioPresetExtensions` type provides a set of convenience methods to create common audio extraction presets. These presets can be used to extract audio from a video file and save it in a specific format.

Example usage:

## ConcatPresetExtensions
The `ConcatPresetExtensions` type offers fluent extension methods to build a `ConcatPreset` for concatenating multiple inputs, optionally applying hardware‑accelerated re‑encoding, quality presets, and lead‑in delays. It also provides an asynchronous helper to run the preset and retrieve the generated output file path.

## OutputFileExtensions
The `OutputFileExtensions` type provides fluent extension methods to configure output file properties like container format, video/audio codecs, bitrates, frame rate, resolution, metadata, and overwrite behavior. These methods enable building an `OutputFile` configuration using a fluent API.

Example usage:

## SubtitlePreset
The `SubtitlePreset` type burns a subtitle file (e.g., `.srt` or `.ass`) into a video using FFmpeg's `subtitles` video filter. It takes the input/output paths plus the subtitle file, escapes the subtitle path for safe inclusion in the filter argument, and can build an `FFmpegCommand`, produce raw ffmpeg arguments, or run the conversion directly.

Example usage:
```csharp
var preset = new SubtitlePreset("input.mp4", "output-with-subtitles.mp4")
    .WithSubtitle("subtitles.srt");

// Burn the subtitles into the output video.
await preset.RunAsync();

// Or inspect the generated command/arguments instead of running immediately.
FFmpegCommand command = preset.Build();
string[] arguments = preset.BuildArguments();
```

## NormalizeAudioPreset
The `NormalizeAudioPreset` type normalizes the audio loudness of a media file using FFmpeg's `loudnorm` filter. It lets you configure the target integrated loudness (LUFS), true peak (dBTP), and loudness range (LRA), optionally enable normalization and metadata printing, and supports two-pass workflows by supplying measurement data from a first pass via `WithMeasurementInput`. Like the other presets, it can produce raw ffmpeg arguments via `BuildArguments()` or run the conversion directly with `RunAsync()`, and `ToString()` returns a human-readable summary of the configured options.

Example usage:
```csharp
var preset = new NormalizeAudioPreset("input.mp4", "output-normalized.mp4")
    .WithTargetIntegrated(-16.0)
    .WithTargetTruePeak(-1.5)
    .WithTargetLra(11.0)
    .WithNormalize(true)
    .WithPrintMetadata(true);

// Inspect the generated ffmpeg arguments instead of running immediately.
string[] arguments = preset.BuildArguments();

// Run the loudness normalization.
await preset.RunAsync();
```

## TrimPreset
The `TrimPreset` type extracts a time-coded segment from a media file by seeking to a start position (`From`) and stopping at an end position (`To`) or after a fixed length (`Duration`). When the trimmed output must match the source exactly it can be re-encoded via `WithReencode`, while `StreamCopy` performs a fast lossless cut without re-encoding. Like the other presets, it can build an `FFmpegCommand` via `Build()`, produce raw ffmpeg arguments via `BuildArguments()`, or run the trim directly with `Run()`.

Example usage:
```csharp
var preset = new TrimPreset("input.mp4", "output-trimmed.mp4")
    .From(TimeSpan.FromSeconds(10))
    .To(TimeSpan.FromSeconds(40))
    .StreamCopy();

// Inspect the generated ffmpeg arguments instead of running immediately.
string[] arguments = preset.BuildArguments();

// Perform the trim.
preset.Run();
```

## SpeedPreset
The `SpeedPreset` type changes the playback speed of a video file using FFmpeg's `setpts` and `atempo` filters, with optional pitch preservation via the `rubberband` filter. It allows specifying a speed factor, choosing whether to preserve audio pitch, and configuring re-encoding codecs.

Example usage:
```csharp
var preset = new SpeedPreset("input.mp4", "output-fast.mp4")
    .WithSpeed(2.0)
    .PreservePitch(true)
    .WithReencode("libx264", "aac");

// Inspect the generated ffmpeg arguments instead of running immediately.
string[] arguments = preset.BuildArguments();

// Perform the speed adjustment.
preset.Run();
```
