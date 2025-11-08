using Windows.Foundation;

namespace McTimeline;

public sealed partial class McTimeline : Control {
    private Grid? _container;
    private Grid? _timeScaleGrid;
    private Canvas? _timeScaleDays;
    private Canvas? _timeScaleHours;
    private Border? _legendBorder;
    private ItemsRepeater? _seriesRepeater;
    private ScrollViewer? _timelineScroll;
    private Canvas? _timelineCanvas;

    public McTimeline() {
        this.DefaultStyleKey = typeof(McTimeline);
    }

    protected override void OnApplyTemplate() {
        base.OnApplyTemplate();

        _container = GetTemplateChild("PART_Container") as Grid;
        _timeScaleGrid = GetTemplateChild("PART_TimeScaleGrid") as Grid;
        _timeScaleDays = GetTemplateChild("PART_TimeScaleDays") as Canvas;
        _timeScaleHours = GetTemplateChild("PART_TimeScaleHours") as Canvas;
        _legendBorder = GetTemplateChild("PART_LegendBorder") as Border;
        _seriesRepeater = GetTemplateChild("PART_SeriesRepeater") as ItemsRepeater;
        _timelineScroll = GetTemplateChild("PART_TimelineScroll") as ScrollViewer;
        _timelineCanvas = GetTemplateChild("PART_TimelineCanvas") as Canvas;
    }

    protected override Size MeasureOverride(Size availableSize) {
        _container?.Measure(availableSize);
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize) {
        return base.ArrangeOverride(finalSize);
    }
}
