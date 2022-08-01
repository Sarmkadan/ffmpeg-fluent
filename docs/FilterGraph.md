# FilterGraph
The `FilterGraph` type is a core component in the `ffmpeg-fluent` project, designed to facilitate the creation and management of filter graphs for media processing. It provides a fluent interface for adding filters, chaining them together, and building the final filter graph. This enables developers to easily construct complex media processing pipelines using a variety of filters.

## API
### AddFilter
Adds a new filter to the graph. There are two overloads:
- `AddFilter`: Adds a filter without specifying its name.
- `AddFilter(string name, params)`: Adds a filter with the specified name and parameters.
Both overloads return the `FilterGraph` instance itself, allowing for method chaining. They do not throw exceptions unless the filter cannot be added due to invalid parameters or an inconsistent state.

### Chain
Chains the current filter graph with another, effectively linking them together in a sequence. This method returns the `FilterGraph` instance, enabling further chaining or configuration. It may throw an exception if the chaining operation fails due to incompatible filter configurations.

### Build
Constructs the final filter graph based on the filters added and their configurations. This method returns a `string` representation of the filter graph, which can be used directly in media processing commands. It may throw an exception if the filter graph is invalid or cannot be built due to missing or conflicting configurations.

## Usage
The following examples demonstrate how to use the `FilterGraph` type to create and manage filter graphs:
```csharp
// Example 1: Simple filter graph creation
var graph = new FilterGraph()
    .AddFilter("scale", "640:480")
    .Chain()
    .Build();

// Example 2: More complex filter graph with multiple filters
var complexGraph = new FilterGraph()
    .AddFilter("scale", "1280:720")
    .AddFilter("crop", "640:480:0:0")
    .Chain()
    .AddFilter("drawtext", "text='Hello World':x=10:y=10")
    .Build();
```

## Notes
When working with `FilterGraph`, it's essential to consider the order in which filters are added and chained, as this affects the final output. Additionally, the thread-safety of `FilterGraph` instances should be considered, especially in multi-threaded environments, as the internal state of the graph could be modified concurrently. The `Build` method should be called once all filters have been added and configured to ensure a valid filter graph is generated. Edge cases, such as adding filters with incompatible parameters or chaining graphs in an invalid order, should be handled gracefully by checking for exceptions and validating the filter graph's state before attempting to build it.
