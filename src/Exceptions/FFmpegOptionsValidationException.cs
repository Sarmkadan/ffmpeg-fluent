#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegFluent;

/// <summary>
/// Exception thrown when encoding options are inconsistent or out of range and FFmpeg would not be launched.
/// Carries every problem found, not just the first.
/// </summary>
public class FFmpegOptionsValidationException : Exception
{
    /// <summary>
    /// Gets the human-readable validation problems, one entry per rule violation.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegOptionsValidationException"/> class.
    /// </summary>
    /// <param name="errors">The validation problems that were found.</param>
    public FFmpegOptionsValidationException(IReadOnlyList<string> errors)
        : base(FormatMessage(errors))
    {
        Errors = errors.ToArray();
    }

    private static string FormatMessage(IReadOnlyList<string> errors)
    {
        return $"FFmpeg options are invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(e => $"  - {e}"))}";
    }
}
