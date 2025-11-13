using Windows.Foundation;
using System.Collections.Specialized;

namespace McTimeline;

public sealed partial class McTimeline : Control {

    #region Private fields

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

    private readonly McVirtualTimeAxis _timeAxis;

    #endregion

    public McTimeline() {
        this.DefaultStyleKey = typeof(McTimeline);
        _timeAxis = new McVirtualTimeAxis();
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

        // Adjust legend column widths
        if (_legendBorder?.Parent is Grid legendHost && legendHost.ColumnDefinitions.Count > 0) {
            _legendColumn = legendHost.ColumnDefinitions[0];
            _legendColumnWidth = _legendColumn.Width;
        }

        if (_timeScaleGrid?.ColumnDefinitions.Count > 0) {
            _timeScaleLegendColumn = _timeScaleGrid.ColumnDefinitions[0];
            _timeScaleLegendColumnWidth = _timeScaleLegendColumn.Width;
        }

        // Subscribe to canvas size changes
        if (_timelineCanvas != null) {
            _timelineCanvas.SizeChanged += OnTimelineCanvasSizeChanged;
        }

        // Subscribe to scroll changes
        if (_timelineScroll != null) {
            _timelineScroll.ViewChanged += OnTimelineScrollViewChanged;
        }

        // Initialize time axis
        _timeAxis.SetRange(MinDate, MaxDate);
        _timeAxis.PixelsPerHour = PixelsPerHour;
        if (_timelineCanvas != null) {
            _timeAxis.ViewportPixels = _timelineCanvas.ActualWidth;
        }

        UpdateLegendVisibility();
    }

    private void OnTimelineCanvasSizeChanged(object sender, SizeChangedEventArgs e) {
        // Update viewport width when canvas size changes
        _timeAxis.ViewportPixels = e.NewSize.Width;
        InvalidateTimeline();
    }

    private void OnTimelineScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e) {
        // Update scroll offset when user scrolls
        if (_timelineScroll != null) {
            // Convert pixel offset to hours
            double offsetHours = _timelineScroll.HorizontalOffset / _timeAxis.PixelsPerHour;
            _timeAxis.OffsetHours = offsetHours;
            InvalidateTimeline();
        }
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

    /// <summary>
    /// Invalidates the current timeline, signaling that it should be refreshed or repainted.
    /// </summary>
    /// <remarks>Call this method when changes occur that require the timeline to be updated visually. This
    /// method does not immediately trigger a repaint; the actual update may be deferred depending on the
    /// implementation.</remarks>
    private void InvalidateTimeline() {
        // Placeholder for repaint logic
        // Will be implemented later
    }
}
