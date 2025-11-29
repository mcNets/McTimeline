using McTimeline.Controls;

namespace McTimeline.Pools;

/// <summary>
/// Manages a pool of reusable <see cref="McLegend"/> controls for legend items to improve performance.
/// </summary>
public sealed partial class McLegendItemPool : IDisposable, IMcLegendItemPool {
    private readonly Queue<McLegend> _availableItems = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the McLegendItemPool class.
    /// And pre-populates the pool with a set number of McLegend controls.
    /// </summary>
    /// <param name="legendStyle">The style to apply to the McLegend control.</param>
    public McLegendItemPool(Style? legendStyle) {
        LegendStyle = legendStyle;

        for (int i = 0; i < 5; i++) {
            _availableItems.Enqueue(CreateLegend());
        }
    }

    /// <summary>
    /// Gets or sets the style applied to each McLegend control.
    /// </summary>
    public Style? LegendStyle { get; set; }

    /// <summary>
    /// Gets a reusable FrameworkElement for a legend item.
    /// If no items are available in the pool, creates a new one.
    /// </summary>
    /// <param name="text">The text to display in the legend item.</param>
    /// <returns>A McLegend control configured for the legend item.</returns>
    public FrameworkElement GetLegendItem(string text) {
        var legend = _availableItems.Count > 0 ? _availableItems.Dequeue() : CreateLegend();
        legend.Style = LegendStyle;
        legend.LegendText = text;
        return legend;
    }

    /// <summary>
    /// Returns a FrameworkElement to the pool for reuse.
    /// </summary>
    /// <param name="element">The FrameworkElement to recycle.</param>
    public void RecycleLegendItem(FrameworkElement element) {
        if (element is McLegend legend) {
            legend.LegendText = string.Empty;
            _availableItems.Enqueue(legend);
        }
    }

    /// <summary>
    /// Creates a new McLegend control with the current LegendStyle.
    /// </summary>
    private McLegend CreateLegend() {
        return new McLegend {
            Style = LegendStyle
        };
    }

    /// <summary>
    /// Clears the pool, disposing of all cached elements.
    /// </summary>
    public void Clear() {
        _availableItems.Clear();
    }

    /// <summary>
    /// Releases all resources used by the McLegendItemPool.
    /// </summary>
    public void Dispose() {
        if (!_disposed) {
            Clear();
            _disposed = true;
        }
    }

}