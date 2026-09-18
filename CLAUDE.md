# CLAUDE.md

FFmpegFluent: a dependency-free C# fluent DSL over the `ffmpeg`/`ffprobe` CLI (net10.0 class library, package `FFmpegFluent`, namespace `FFmpegFluent`).

## Build

```bash
dotnet build              # requires .NET SDK 10.x (system dotnet at /usr/local/bin/dotnet; /root/.dotnet is SDK 8 and fails with NETSDK1045)
dotnet pack               # NuGet package (Version in ffmpeg-fluent.csproj)
```

Single project `ffmpeg-fluent.csproj` at repo root, no solution file, no NuGet dependencies (BCL only). `Nullable` and `ImplicitUsings` enabled, `GenerateDocumentationFile` on - every public member needs XML docs or the build warns.

`build/` holds a committed prebuilt DLL/PDB/XML; `bin/` and `obj/` are gitignored.

## Tests

No test project or test runner. Ad-hoc tests are static classes inside `src/` that print to console:

- `src/ArgumentEscaperTests.cs` - `ArgumentEscaperTests.RunAll()`
- `src/FFmpegCommandValidationTests.cs` - `FFmpegCommandValidationTests.RunTests()`

They are compiled into the library but not invoked anywhere. `Program.cs` at the root is a manual smoke check (`Main` exercising `AudioOptions`/`VideoOptions`); the project is a library, so it is not executed either. To run any of these, reference the assembly from a throwaway console project and call the methods.

## Lint / format

None configured (no `.editorconfig`, no analyzers). Follow existing style: file-scoped namespaces, 4-space indent, `#nullable enable`, XML doc comments on all public API.

## Layout

- `src/` - all library code, flat, one public type per file.
- `docs/ARCHITECTURE.md` - component breakdown, data flow, design trade-offs, known limitations. Read this first.
- `docs/<TypeName>.md` - per-type reference, one file per public type. Update when a public API changes.
- `README.md` - per-type usage examples.
- Repo root contains junk files with names like `.Volume(0.5);` and `Example usage` (artifacts of a past aider session). They are tracked in git; ignore them and do not create more.

Entry points for readers:
- `src/FFmpegCommand.cs` - core composable command (`FFmpegCommand.Create(...)`, `AddInput`, `AddOutput`, `BuildCommandLine()`, `RunAsync()`).
- `src/InputFile.cs`, `src/OutputFile.cs`, `src/FilterGraph.cs`, `src/AudioOptions.cs`, `src/VideoOptions.cs` - building blocks.
- `src/*Preset.cs` (`ConcatPreset`, `GifPreset`, `ThumbnailPreset`, `ExtractAudioPreset`, `TrimPreset`, `SpeedPreset`, `SubtitlePreset`, `NormalizeAudioPreset`), `src/WatermarkHelper.cs` - standalone job presets, each owning its own process pipeline.
- `src/MediaInfo.cs` - `ffprobe` JSON probing.
- `src/FFmpegProgress.cs` - regex parse of ffmpeg stderr status lines into `IProgress<FFmpegProgress>`.
- `src/FFmpegLocator.cs` / `IFFmpegLocator.cs` - resolving the ffmpeg binary path.
- `src/ArgumentEscaper.cs` - OS-specific path quoting.

## Conventions

- Companion-file pattern: for a type `X`, sugar lives in `XExtensions.cs` (fluent factories/helpers), JSON round-trip in `XJsonExtensions.cs` (`ToJson`/`FromJson` via `System.Text.Json`), and non-throwing checks in `XValidation.cs` (`Validate()` returning a list of problem strings). Keep core types small; add new convenience API as extension methods.
- Fluent methods return `this` (or the extended instance) for chaining.
- Presets are independent of `FFmpegCommand`; they throw `InvalidOperationException` on non-zero exit, while `FFmpegCommand.RunAsync` throws `FFmpegException` (exit code, stderr, command line).
- `FFmpegCommand` fields `_inputs`, `_outputs`, `_filterGraph` are `internal` on purpose so `FFmpegCommandValidation` can inspect them.
- Arguments are rendered as one space-joined string; only input/output paths are quoted (`ArgumentEscaper`). Option values and filter strings are passed raw.
- Numeric parsing/formatting uses `CultureInfo.InvariantCulture`.
- Every `Create`/`RunAsync`/`ProbeAsync` accepts an optional `ffmpegPath`/`ffprobePath` and a `CancellationToken`.
- Commit messages use conventional prefixes (`fix:`, `chore:`, `docs:`).
