# IPreset

The `IPreset` interface is implemented by presets that can contribute their inputs, outputs and options to an existing `FFmpegCommand`. Pass any implementation to `FFmpegCommand.ApplyPreset` to compose it with other command configuration.

## API

### `void Apply(FFmpegCommand command)`
- **Purpose:** Applies the preset's configuration to the given command.
- **Parameters:** `command` – The command to configure.
- **Return:** None.
- **Throws:** Implementation-specific; for example, `ConcatPreset` throws `InvalidOperationException` when it has fewer than two inputs or no output path.

## Implementations

- `TrimPreset`
- `GifPreset`
- `ThumbnailPreset`
- `ConcatPreset`
- `ExtractAudioPreset`

## Usage

```csharp
var command = FFmpegCommand.Create()
    .ApplyPreset(new TrimPreset("input.mp4", "output-trimmed.mp4")
        .From(TimeSpan.FromSeconds(5))
        .Duration(TimeSpan.FromSeconds(10))
        .StreamCopy());

await command.RunAsync();
```

## Notes

- `Apply` only configures the command; it does not run anything. Call `RunAsync` on the command afterwards.
- The preset's own `RunAsync`, `Run` and `BuildArguments` methods are unchanged and remain independent of `Apply`.
