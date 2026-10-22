# FFmpegOptionsValidationException

Thrown when encoding options are invalid or inconsistent. It is raised before FFmpeg is launched, by `FFmpegCommand.RunAsync` and by `VideoOptionsValidation.EnsureValid`.

## API

### Errors
`IReadOnlyList<string> Errors` - Every problem found, one entry per violated rule. For `FFmpegCommand.RunAsync`, each entry is prefixed with `Output '<path>':`.

### Message
Lists all errors, one per line.

## Example

```csharp
try
{
    await FFmpegCommand.Create()
        .AddInput("in.mp4")
        .AddOutput("out.mp4", o => o.WithVideo(v => v.Codec("libx264").Crf(23).Bitrate("2M")))
        .RunAsync();
}
catch (FFmpegOptionsValidationException ex)
{
    foreach (var error in ex.Errors)
        Console.WriteLine(error);
}
```

See `VideoOptions.md` for the list of rules.
