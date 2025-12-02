using McTimeline.Controls;

namespace McTimeline.Pools;

/// <summary>
/// Provides a pool of reusable <see cref="McTimelineBar"/> controls to optimize performance
/// by reducing the overhead of creating and destroying UI elements.
/// </summary>
public class McSeriesItemPool : IMcSeriesItemPool, IDisposable {
    /// <summary>
    /// The default number of items to pre-allocate in the pool.
    /// </summary>
    const int INITIAL_POOL_SIZE = 10;

    private readonly Queue<McTimelineBar> _availableItems = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="McSeriesItemPool"/> class.
    /// </summary>
    /// <param name="style">The style to apply to all timeline bar items.</param>
    /// <param name="initialPoolSize">The initial number of items to create in the pool. Defaults to <see cref="INITIAL_POOL_SIZE"/>.</param>
    public McSeriesItemPool(Style? style, int initialPoolSize = INITIAL_POOL_SIZE) {
        ItemStyle = style;
        
        for (int i = 0; i < initialPoolSize; i++) {
            _availableItems.Enqueue(new McTimelineBar());
        }
    }

    /// <summary>
    /// Gets or sets the style applied to timeline bar items when retrieved from the pool.
    /// </summary>
    public Style? ItemStyle { get; set; }

    /// <summary>
    /// Retrieves a timeline bar item from the pool, or creates a new one if the pool is empty.
    /// </summary>
    /// <param name="seriesName">The name of the series (currently unused but available for future extensions).</param>
    /// <returns>A <see cref="McTimelineBar"/> control ready for use.</returns>
    public FrameworkElement GetSeriesItem(string seriesName) {
        McTimelineBar item;
        if (_availableItems.Count > 0) {
            item = _availableItems.Dequeue();
        }
        else {
            item = new McTimelineBar();
        }
        item.Style = ItemStyle;
        return item;
    }

    /// <summary>
    /// Returns a timeline bar item to the pool for reuse.
    /// </summary>
    /// <param name="element">The framework element to recycle. Must be a <see cref="McTimelineBar"/>.</param>
    public void RecycleSeriesItem(FrameworkElement element) {
        if (element is McTimelineBar item) {
            item.ItemText = string.Empty;
            _availableItems.Enqueue(item);
        }
    }

    /// <summary>
    /// Clears all items from the pool.
    /// </summary>
    public void Clear() {
        _availableItems.Clear();
    }

    /// <summary>
    /// Releases all resources used by the pool.
    /// </summary>
    public void Dispose() {
        Clear();
        GC.SuppressFinalize(this);
    }

}
