namespace FFmpegFluent;

/// <summary>
/// Represents a filter graph builder for FFmpeg's -filter_complex option.
/// </summary>
public class FilterGraph
{
    private readonly List<string> _filters = [];
    private readonly List<(string inputLabel, string filter, string outputLabel)> _chains = [];

    /// <summary>
    /// Gets whether the filter graph is empty (no filters or chains defined).
    /// </summary>
    public bool IsEmpty => _filters.Count == 0 && _chains.Count == 0;

    /// <summary>
    /// Adds a raw filter string to the graph.
    /// </summary>
    /// <param name="filter">The raw filter string (e.g., "scale=1280:720").</param>
    /// <returns>The FilterGraph instance for method chaining.</returns>
    public FilterGraph AddFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            throw new ArgumentException("Filter string cannot be null or whitespace.", nameof(filter));
        }

        _filters.Add(filter.Trim());
        return this;
    }

    /// <summary>
    /// Adds a named filter with arguments to the graph.
    /// </summary>
    /// <param name="name">The name of the filter (e.g., "scale", "fps").</param>
    /// <param name="args">Key-value pairs representing filter arguments.</param>
    /// <returns>The FilterGraph instance for method chaining.</returns>
    public FilterGraph AddFilter(string name, params (string key, string value)[] args)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Filter name cannot be null or whitespace.", nameof(name));
        }

        if (args == null || args.Length == 0)
        {
            throw new ArgumentException("At least one argument must be provided.", nameof(args));
        }

        var argString = string.Join(":", args.Select(a => $"{a.key}={a.value}"));
        _filters.Add($"{name}={argString}");
        return this;
    }

    /// <summary>
    /// Creates a chain between an input label, filter, and output label.
    /// </summary>
    /// <param name="inputLabel">The label of the input stream.</param>
    /// <param name="filter">The filter to apply (can be a raw filter string or filter name).</param>
    /// <param name="outputLabel">The label for the output stream.</param>
    /// <returns>The FilterGraph instance for method chaining.</returns>
    public FilterGraph Chain(string inputLabel, string filter, string outputLabel)
    {
        if (string.IsNullOrWhiteSpace(inputLabel))
        {
            throw new ArgumentException("Input label cannot be null or whitespace.", nameof(inputLabel));
        }

        if (string.IsNullOrWhiteSpace(filter))
        {
            throw new ArgumentException("Filter cannot be null or whitespace.", nameof(filter));
        }

        if (string.IsNullOrWhiteSpace(outputLabel))
        {
            throw new ArgumentException("Output label cannot be null or whitespace.", nameof(outputLabel));
        }

        _chains.Add((inputLabel.Trim(), filter.Trim(), outputLabel.Trim()));
        return this;
    }

    /// <summary>
    /// Builds the complete filter_complex string.
    /// </summary>
    /// <returns>The complete filter_complex string.</returns>
    public string Build()
    {
        if (IsEmpty)
        {
            return string.Empty;
        }

        var parts = new List<string>();

        // Add standalone filters
        if (_filters.Count > 0)
        {
            parts.AddRange(_filters);
        }

        // Add chains
        foreach (var (inputLabel, filter, outputLabel) in _chains)
        {
            parts.Add($"[{inputLabel}]{filter}[{outputLabel}]");
        }

        return string.Join("; ", parts);
    }
}
