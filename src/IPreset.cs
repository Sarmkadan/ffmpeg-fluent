namespace FFmpegFluent;

/// <summary>
/// A reusable job configuration that contributes its inputs, outputs and options to an <see cref="FFmpegCommand"/>.
/// </summary>
public interface IPreset
{
    /// <summary>
    /// Applies this preset's configuration to the given command.
    /// </summary>
    /// <param name="command">The command to configure.</param>
    void Apply(FFmpegCommand command);
}
