namespace McTimeline.Pools;

public interface IMcLegendItemPool {
    void Clear();
    void Dispose();
    FrameworkElement GetLegendItem(string seriesName);
    void RecycleLegendItem(FrameworkElement element);
}
