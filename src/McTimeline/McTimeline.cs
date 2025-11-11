using Windows.Foundation;
using System.Collections.Specialized;

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
    private ColumnDefinition? _legendColumn;
    private ColumnDefinition? _timeScaleLegendColumn;
    private GridLength _legendColumnWidth;
    private GridLength _timeScaleLegendColumnWidth;

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

        if (_legendBorder?.Parent is Grid legendHost && legendHost.ColumnDefinitions.Count > 0) {
            _legendColumn = legendHost.ColumnDefinitions[0];
            _legendColumnWidth = _legendColumn.Width;
        }

        if (_timeScaleGrid?.ColumnDefinitions.Count > 0) {
            _timeScaleLegendColumn = _timeScaleGrid.ColumnDefinitions[0];
            _timeScaleLegendColumnWidth = _timeScaleLegendColumn.Width;
        }

        UpdateLegendVisibility();
    }

    protected override Size MeasureOverride(Size availableSize) {
        _container?.Measure(availableSize);
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize) {
        return base.ArrangeOverride(finalSize);
    }

    private void UpdateLegendVisibility() {
        bool isVisible = IsLegendVisible;

        if (_legendBorder is not null) {
            _legendBorder.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        if (_legendColumn is not null) {
            _legendColumn.Width = isVisible ? _legendColumnWidth : new GridLength(0);
        }

        if (_timeScaleLegendColumn is not null) {
            _timeScaleLegendColumn.Width = isVisible ? _timeScaleLegendColumnWidth : new GridLength(0);
        }
    }

    private void OnSeriesCollectionChanged(McTimelineSeriesCollection oldValue, McTimelineSeriesCollection newValue) {
        if (oldValue != null) {
            oldValue.CollectionChanged -= OnSeriesCollectionChanged;
            foreach (var series in oldValue) {
                series.Items.CollectionChanged -= OnSeriesItemsChanged;
            }
        }
        if (newValue != null) {
            newValue.CollectionChanged += OnSeriesCollectionChanged;
            foreach (var series in newValue) {
                series.Items.CollectionChanged += OnSeriesItemsChanged;
            }
        }
        InvalidateTimeline();
    }

    private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.OldItems != null) {
            foreach (McTimelineSeries series in e.OldItems) {
                series.Items.CollectionChanged -= OnSeriesItemsChanged;
            }
        }
        if (e.NewItems != null) {
            foreach (McTimelineSeries series in e.NewItems) {
                series.Items.CollectionChanged += OnSeriesItemsChanged;
            }
        }
        InvalidateTimeline();
    }

    private void OnSeriesItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        InvalidateTimeline();
    }

    private void InvalidateTimeline() {
        // Placeholder for repaint logic
        // Will be implemented later
    }
}
