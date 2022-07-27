using System;
using System.Collections.Generic;
using System.Linq;

namespace FFmpegFluent;

public static class FFmpegCommandValidation
{
    /// <summary>
    /// Validates the FFmpegCommand instance and returns a list of human-readable problems.
    /// Checks for null/empty inputs and outputs, empty filter graphs, and invalid paths.
    /// </summary>
    /// <param name="value">The FFmpegCommand to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> Validate(this FFmpegCommand value)
    {
        if (value == null)
        {
            return new[] { "FFmpegCommand cannot be null." };
        }

        var problems = new List<string>();

        // Validate inputs
        if (value._inputs == null)
        {
            problems.Add("Input collection cannot be null.");
        }
        else if (value._inputs.Count == 0)
        {
            problems.Add("At least one input file must be specified.");
        }
        else
        {
            foreach (var input in value._inputs)
            {
                if (input == null)
                {
                    problems.Add("Input file cannot be null.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(input.Path))
                {
                    problems.Add("Input file path cannot be null or whitespace.");
                    break;
                }
            }
        }

        // Validate outputs
        if (value._outputs == null)
        {
            problems.Add("Output collection cannot be null.");
        }
        else if (value._outputs.Count == 0)
        {
            problems.Add("At least one output file must be specified.");
        }
        else
        {
            foreach (var output in value._outputs)
            {
                if (output == null)
                {
                    problems.Add("Output file cannot be null.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(output.Path))
                {
                    problems.Add("Output file path cannot be null or whitespace.");
                    break;
                }
            }
        }

        // Validate filter graph
        if (value._filterGraph == null)
        {
            problems.Add("Filter graph cannot be null.");
        }
        else if (value._filterGraph.IsEmpty)
        {
            // Empty filter graph is valid (not required)
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the FFmpegCommand instance is valid.
    /// </summary>
    /// <param name="value">The FFmpegCommand to validate</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this FFmpegCommand value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the FFmpegCommand instance is valid, throwing ArgumentException if not.
    /// </summary>
    /// <param name="value">The FFmpegCommand to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this FFmpegCommand value)
    {
        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"FFmpegCommand validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems.Select(p => $"  - {p}"))}");
        }
    }
}
