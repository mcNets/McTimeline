namespace McTimeline.Pools;

public class McSeriesItemPool : IMcSeriesItemPool, IDisposable {
    const int INITIAL_POOL_SIZE = 10;

    private readonly Queue<McTimelineBar> _availableItems = new();

    public McSeriesItemPool(Style? style, int initialPoolSize = INITIAL_POOL_SIZE) {
        ItemStyle = style;
        
        for (int i = 0; i < initialPoolSize; i++) {
            _availableItems.Enqueue(new McTimelineBar());
        }
    }

    public Style? ItemStyle { get; set; }

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

    public void RecycleSeriesItem(FrameworkElement element) {
        if (element is McTimelineBar item) {
            item.ItemText = string.Empty;
            _availableItems.Enqueue(item);
        }
    }

    public void Clear() {
        _availableItems.Clear();
    }

    public void Dispose() {
        Clear();
        GC.SuppressFinalize(this);
    }

}
