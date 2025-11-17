namespace McTimeline;

public interface IMcLegendItemPool {
    void Clear();
    void Dispose();
    UIElement GetLegendItem(string seriesName);
    void RecycleLegendItem(UIElement element);
}
