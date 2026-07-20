using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FFmpegFluent;

/// <summary>
/// Extension methods for <see cref="FFmpegCommand"/>.
/// </summary>
public static class FFmpegCommandExtensions
{
    /// <summary>
    /// Returns a dry‑run description of the command.
    /// The result contains the full command line produced by <see cref="FFmpegCommand.BuildCommandLine"/>
    /// followed by a human‑readable multi‑line breakdown of global options, inputs, filter graph,
    /// and outputs. No process is started.
    /// </summary>
    /// <param name="command">The command to preview.</param>
    /// <returns>A string containing the preview.</returns>
    public static string Preview(this FFmpegCommand command)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));

        var sb = new StringBuilder();

        // Full command line
        sb.AppendLine("Command line:");
        sb.AppendLine(command.BuildCommandLine());
        sb.AppendLine();

        sb.AppendLine("Breakdown:");

        // Global options
        sb.AppendLine("Global options:");
        AppendEnumerable(sb, GetFieldOrProperty(command, "_globalOptions") ?? GetFieldOrProperty(command, "GlobalOptions"));

        // Inputs
        sb.AppendLine("Inputs:");
        AppendEnumerable(sb, GetFieldOrProperty(command, "_inputs") ?? GetFieldOrProperty(command, "Inputs"));

        // Filter graph
        sb.AppendLine("Filter graph:");
        AppendSingle(sb, GetFieldOrProperty(command, "_filterGraph") ?? GetFieldOrProperty(command, "FilterGraph"));

        // Outputs
        sb.AppendLine("Outputs:");
        AppendEnumerable(sb, GetFieldOrProperty(command, "_outputs") ?? GetFieldOrProperty(command, "Outputs"));

        return sb.ToString();
    }

    private static void AppendEnumerable(StringBuilder sb, object? value)
    {
        if (value is IEnumerable enumerable && !(value is string))
        {
            int i = 0;
            foreach (var item in enumerable)
            {
                sb.AppendLine($"  [{i}] {item}");
                i++;
            }
            if (i == 0)
            {
                sb.AppendLine("  (none)");
            }
        }
        else
        {
            AppendSingle(sb, value);
        }
    }

    private static void AppendSingle(StringBuilder sb, object? value)
    {
        if (value == null)
        {
            sb.AppendLine("  (none)");
        }
        else
        {
            sb.AppendLine($"  {value}");
        }
    }

    private static object? GetFieldOrProperty(object obj, string name)
    {
        var type = obj.GetType();
        var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) return field.GetValue(obj);

        var prop = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
        return prop?.GetValue(obj);
    }
}
