# Architecture

## Overview

FFmpegFluent is a typed, fluent C# DSL over the FFmpeg **command line** (not libav bindings).
Every feature ultimately does the same thing: build an argument string, spawn an
`ffmpeg`/`ffprobe` process via `System.Diagnostics.Process`, and interpret its exit code and
stderr. There is no P/Invoke, no native interop, and no NuGet dependency - the library targets
`net10.0` and uses only the BCL.

The library is a single project (`ffmpeg-fluent.csproj`, assembly `FFmpegFluent`), all sources
under `src/`, single namespace `FFmpegFluent`.

## Component breakdown

### 1. Core command builder

- **`FFmpegCommand`** - the general-purpose entry point. Created via
  `FFmpegCommand.Create(ffmpegPath)`; collects global options, `InputFile`s, `OutputFile`s and a
  `FilterGraph`, renders them with `BuildCommandLine()` (global options, then inputs, then
  `-filter_complex` if the graph is non-empty, then outputs), and executes with
  `RunAsync(IProgress<FFmpegProgress>?, CancellationToken)`. On non-zero exit it throws
  `FFmpegException` carrying the command line, exit code, and captured stderr.
- **`InputFile`** - per-input options: `Seek` (`-ss`), `Duration` (`-t`), `Loop`
  (`-stream_loop`, where `Loop(0)` means infinite via `-stream_loop -1`), and arbitrary
  `Option(key, value)`. Emits pre-`-i` options in a fixed order ending with `-i "path"`.
- **`OutputFile`** - holds a `VideoOptions` and an `AudioOptions` instance (configured through
  `WithVideo`/`WithAudio` callbacks) plus free-form options (`Format`, `Overwrite`, `Metadata`,
  `Option`). `BuildArgs()` yields its options, then video args, then audio args, then the path.
- **`VideoOptions` / `AudioOptions`** - codec, bitrate, resolution/frame rate (video), channels/
  sample rate/volume (audio), each rendering to a list of flag strings.
- **`FilterGraph`** - builds a `-filter_complex` value from raw filter strings, named filters
  with key/value args, and labeled chains (`[in]filter[out]`); segments are joined with `"; "`.
- **`FFmpegProgress`** - regex-parses ffmpeg's stderr status lines
  (`frame=... fps=... time=... bitrate=... speed=...`) into a typed record. `time=` is mandatory
  for a line to count as progress; the fractional seconds part is scaled by digit count
  (ffmpeg prints centiseconds). `FFmpegCommand.RunAsync` feeds every stderr line through
  `FFmpegProgress.TryParse` and reports hits to the caller's `IProgress<T>`.
- **`FFmpegException`** - exit code + stderr + command line, thrown by `FFmpegCommand` on failure.

### 2. Self-contained presets

These do **not** compose with `FFmpegCommand`; each is an independent builder with its own
`Process` invocation and its own `RunAsync`:

- **`ConcatPreset`** - concat demuxer workflow: writes a temporary list file
  (`file '...'` lines, single quotes escaped), runs `-f concat -safe 0 -i list`, either
  `-c copy` (default) or re-encode via `WithReencode`. Failure throws `InvalidOperationException`,
  not `FFmpegException`.
- **`ExtractAudioPreset`** - pull the audio stream out of a container.
- **`GifPreset`** - high-quality GIF via the two-pass palette idiom
  (`split → palettegen → paletteuse`) rendered as a single `-filter_complex`, with optional
  fps/width/time-range settings.
- **`ThumbnailPreset`** - frame extraction to an image.
- **`WatermarkHelper`** - overlays an image (input `1:v`) on a video (`0:v`) with position/
  margin/opacity/scale, re-encoding to H.264; audio is passed through from the source video
  (`-map 0:a?`).
- **`HardwareAccelOptions`** - value object describing `-hwaccel` kind/device/extra input args,
  consumed by preset extension methods.
- **`MediaInfo`** - the only `ffprobe` consumer: `ProbeAsync` runs
  `ffprobe -print_format json -show_format -show_streams`, parses the JSON with
  `System.Text.Json`, and exposes duration, bitrate, codecs, dimensions, frame rate, and audio
  properties. Has an internal 10-second timeout linked to the caller's token.

### 3. Companion files (per-type pattern)

Almost every type `X` has up to three siblings, generated per a fixed convention:

- `XExtensions.cs` - fluent convenience/factory extension methods.
- `XJsonExtensions.cs` - `ToJson`/`FromJson` round-tripping via `System.Text.Json`.
- `XValidation.cs` (only for `FFmpegCommand`, `FFmpegProgress`, `HardwareAccelOptions`,
  `ThumbnailPreset`) - `Validate()` returning a list of human-readable problems instead of
  throwing.

`FFmpegCommandValidation` reaches into `FFmpegCommand`'s `internal` fields (`_inputs`,
`_outputs`, `_filterGraph`) - that is why those fields are `internal` rather than `private`.

Per-type reference docs live in `docs/*.md` (one file per public type).

## Key design decisions and trade-offs

- **Shell out to the CLI instead of binding libav.** Rationale: zero native dependencies, works
  with whatever ffmpeg build is on PATH, trivially cross-platform. Trade-off: process-spawn
  overhead per operation, progress limited to what stderr exposes, and correctness depends on
  string rendering of arguments.
- **Arguments are rendered into a single space-joined string** (`Process.StartInfo.Arguments`),
  not `ArgumentList`. Input/output paths are quoted inline; values passed through `Option()` and
  filter strings are not. Trade-off: simple and inspectable (`BuildCommandLine()` returns exactly
  what runs), but option *values* containing spaces or quotes are the caller's responsibility.
- **Presets are independent of `FFmpegCommand`.** Each preset owns its full pipeline, so a preset
  can be read top-to-bottom and used standalone. Trade-off: process-handling code (start, drain
  stderr, check exit code) is duplicated across ~6 types with slightly different behavior - e.g.
  only `FFmpegCommand` throws `FFmpegException`; presets throw `InvalidOperationException`.
- **Errors are surfaced through stderr capture + exit code**, never parsed semantically. FFmpeg's
  exit codes are not fine-grained, so the raw stderr text is attached to the exception for the
  caller to inspect.
- **Progress via regex over stderr status lines** rather than `-progress` pipe output. Simpler
  (no extra fd), but format-coupled to ffmpeg's human-readable status line.

## Data flow

```
caller
  └─ fluent configuration (FFmpegCommand / preset)
       └─ Build*() -> argument string
            └─ Process.Start(ffmpeg, args)
                 ├─ stderr lines ──> FFmpegProgress.TryParse ──> IProgress<FFmpegProgress>
                 └─ exit code ──> 0: return / non-0: FFmpegException (or InvalidOperationException in presets)
```

`MediaInfo.ProbeAsync` is the reverse direction: `ffprobe` stdout (JSON) -> typed `MediaInfo`.

## Extension points

- `FFmpegCommand.GlobalOption` / `InputFile.Option` / `OutputFile.Option` - escape hatch for any
  flag the typed API does not model.
- `FilterGraph.AddFilter(string)` - raw filter strings for anything beyond named filters/chains.
- Extension-method files (`*Extensions.cs`) are the intended place for new convenience APIs -
  core types stay small, sugar lives in extensions.
- `ffmpegPath`/`ffprobePath` parameters on every `Create`/`RunAsync`/`ProbeAsync` - point at any
  ffmpeg build.

## Known limitations

- **No argument escaping beyond path quoting.** Option values or filter expressions containing
  spaces/quotes will be split incorrectly by the space-join in `BuildCommandLine()`.
- **No unit tests** in the repository.
- **`FFmpegCommand.RunAsync` redirects only stderr**; commands that write media to stdout
  (`-f ... pipe:1`) are not supported through it.
- **Inconsistent exception types** between `FFmpegCommand` (`FFmpegException`) and the presets
  (`InvalidOperationException`).
- **`GifPreset`/`ThumbnailPreset`/`WatermarkHelper` read or stream stderr only after exit or via
  events**; extremely verbose ffmpeg output combined with a blocked pipe is drained differently
  per preset (see each `RunAsync`).
- **`MediaInfo.ProbeAsync` hard-codes a 10 s timeout**, which may be too short for probing large
  files on slow storage.
- Progress parsing depends on ffmpeg's default stderr status-line format; `-loglevel quiet` or
  `-progress` mode produces no progress reports.
