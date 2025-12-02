namespace McTimeline.Pools;

public interface IMcSeriesItemPool {
    void Clear();
    void Dispose();
    FrameworkElement GetSeriesItem(string seriesName);
    void RecycleSeriesItem(FrameworkElement element);
}
