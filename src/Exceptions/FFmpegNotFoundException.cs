#nullable enable

using System;

namespace FFmpegFluent;

/// <summary>
/// Exception thrown when FFmpeg or FFprobe executable cannot be located.
/// </summary>
public class FFmpegNotFoundException : Exception
{
    /// <summary>
    /// Gets the list of locations that were searched.
    /// </summary>
    public string[] SearchedLocations { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegNotFoundException"/> class.
    /// </summary>
    /// <param name="executable">The name of the executable that was not found.</param>
    /// <param name="searchedLocations">The list of locations that were searched.</param>
    public FFmpegNotFoundException(string executable, string[] searchedLocations)
        : base($"Could not locate '{executable}'. Searched locations: {string.Join(", ", searchedLocations)}")
    {
        SearchedLocations = searchedLocations;
    }
}
