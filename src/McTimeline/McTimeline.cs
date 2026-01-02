using System.Collections.Specialized;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.Foundation;

namespace McTimeline;

/// <summary>
/// Represents a timeline control that displays time-based series data with interactive navigation.
/// </summary>
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
    private Canvas? _legendCanvas;
    private ScrollBar? _hScroll;
    private ScrollBar? _vScroll;
    private ColumnDefinition? _legendColumn;
    private ColumnDefinition? _timeScaleLegendColumn;
    private GridLength _legendColumnWidth;
    private GridLength _timeScaleLegendColumnWidth;

    private readonly McTimelineViewport _viewport;
    private readonly Dictionary<int, FrameworkElement> _visibleLegendItems = new();
    private readonly McElementPool<McLegend> _legendItemPool;
    private readonly McElementPool<McTimelineBar> _seriesItemPool;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="McTimeline"/> class.
    /// </summary>
    public McTimeline() {
        this.DefaultStyleKey = typeof(McTimeline);
        _viewport = new McTimelineViewport();
        _legendItemPool = new McElementPool<McLegend>(LegendStyle);
        _seriesItemPool = new McElementPool<McTimelineBar>(TimelineItemStyle);
    }

    /// <summary>
    /// Called when the template is applied to the control.
    /// </summary>
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
        _hScroll = GetTemplateChild("PART_HScroll") as ScrollBar;
        _vScroll = GetTemplateChild("PART_VScroll") as ScrollBar;
        _legendCanvas = GetTemplateChild("PART_LegendCanvas") as Canvas;

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
            _timelineCanvas.PointerWheelChanged += OnCanvasPointerWheelChanged;
        }

        if (_legendCanvas != null) {
            _legendCanvas.SizeChanged += OnLegendCanvasSizeChanged;
        }

        // if (_legendCanvas != null) {
        //     _legendCanvas.SizeChanged += OnLegendCanvasSizeChanged;
        // }

        // Subscribe to scroll changes
        if (_timelineScroll != null) {
            _timelineScroll.ViewChanged += OnTimelineScrollViewChanged;
        }

        if (_hScroll != null) {
            _hScroll.ValueChanged += OnHScrollValueChanged;
        }

        if (_vScroll != null) {
            _vScroll.ValueChanged += OnVScrollValueChanged;
        }

        // Initialize time axis
        _viewport.TimeAxis.SetRange(MinDate, MaxDate);
        _viewport.TimeAxis.PixelsPerHour = PixelsPerHour;
        if (_timelineCanvas != null) {
            _viewport.TimeAxis.ViewportPixels = _timelineCanvas.ActualWidth;
        }

        // Initialize series axis
        _viewport.SeriesHeight = SeriesHeight;
        _viewport.SeriesAxis.ContentUnits = SeriesCollection?.Count ?? 0;
        if (_legendCanvas != null) {
            _viewport.SeriesAxis.ViewportPixels = _legendCanvas.ActualHeight;
        }
        _viewport.RefreshVisibleSeriesRange();

        UpdateHScrollBar();
        UpdateVScrollBar();

        UpdateLegendVisibility();
    }

    /// <summary>
    /// Handles the size changed event of the timeline canvas.
    /// Updates the viewport dimensions and refreshes the display.
    /// </summary>
    private void OnTimelineCanvasSizeChanged(object sender, SizeChangedEventArgs e) {
        // Update viewport size when canvas size changes
        _viewport.OnSizeChanged(e.NewSize);
        _viewport.RefreshVisibleSeriesRange();
        UpdateHScrollBar();
        UpdateVScrollBar();
        
        // Update clipping to prevent overflow
        if (_timelineCanvas != null) {
            _timelineCanvas.Clip = new RectangleGeometry {
                Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };
        }
        
        InvalidateTimeline();
    }

    private void OnLegendCanvasSizeChanged(object sender, SizeChangedEventArgs e) {
        // Update clipping for legend canvas
        if (_legendCanvas != null) {
            _legendCanvas.Clip = new RectangleGeometry {
                Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
            };
        }
    }

    /// <summary>
    /// Handles the scroll view changed event.
    /// Synchronizes the viewport with the current scroll position.
    /// </summary>
    private void OnTimelineScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e) {
        // Update scroll offsets when user scrolls
        if (_timelineScroll != null) {
            _viewport.OnScrollChanged(_timelineScroll.HorizontalOffset, _timelineScroll.VerticalOffset);
            InvalidateTimeline();
        }
    }

    /// <summary>
    /// Updates the visibility of the legend based on the <see cref="IsLegendVisible"/> property.
    /// </summary>
    private void UpdateLegendVisibility() {
        bool isVisible = IsLegendVisible;
        _legendBorder?.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        _legendColumn?.Width = isVisible ? _legendColumnWidth : new GridLength(0);
        _timeScaleLegendColumn?.Width = isVisible ? _timeScaleLegendColumnWidth : new GridLength(0);
    }

    /// <summary>
    /// Handles changes to the series collection when the collection is replaced.
    /// </summary>
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
        SyncSeriesAxisWithCollection();
        InvalidateTimeline();
    }

    /// <summary>
    /// Handles changes to the series collection when items are added or removed.
    /// </summary>
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
        SyncSeriesAxisWithCollection();
        InvalidateTimeline();
    }

    /// <summary>
    /// Handles changes to items within a series.
    /// </summary>
    private void OnSeriesItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        InvalidateTimeline();
    }

    /// <summary>
    /// Synchronizes the series axis with the current series collection count.
    /// </summary>
    private void SyncSeriesAxisWithCollection() {
        _viewport.SeriesAxis.ContentUnits = SeriesCollection?.Count ?? 0;
        _viewport.RefreshVisibleSeriesRange();
    }

    /// <summary>
    /// Invalidates the current timeline, signaling that it should be refreshed or repainted.
    /// </summary>
    /// <remarks>Call this method when changes occur that require the timeline to be updated visually. This
    /// method does not immediately trigger a repaint; the actual update may be deferred depending on the
    /// implementation.</remarks>
    private void InvalidateTimeline() {
        DrawLegend();
        DrawTimeline();
    }

    /// <summary>
    /// Handles the horizontal scrollbar value changed event.
    /// </summary>
    private void OnHScrollValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        double horizontalOffsetPx = e.NewValue * _viewport.TimeAxis.PixelsPerHour;
        double verticalUnits = _vScroll?.Value ?? 0;
        double verticalOffsetPx = verticalUnits * _viewport.SeriesAxis.SeriesHeight;
        _viewport.OnScrollChanged(horizontalOffsetPx, verticalOffsetPx);
        InvalidateTimeline();
    }

    /// <summary>
    /// Handles the vertical scrollbar value changed event.
    /// </summary>
    private void OnVScrollValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        double verticalOffsetPx = e.NewValue * _viewport.SeriesAxis.SeriesHeight;
        double horizontalHours = _hScroll?.Value ?? 0;
        double horizontalOffsetPx = horizontalHours * _viewport.TimeAxis.PixelsPerHour;
        _viewport.OnScrollChanged(horizontalOffsetPx, verticalOffsetPx);
        InvalidateTimeline();
    }

    /// <summary>
    /// Handles the mouse wheel event for zooming and scrolling.
    /// Control+Wheel: Horizontal zoom, Shift+Wheel: Horizontal scroll, Wheel: Vertical scroll.
    /// </summary>
    private void OnCanvasPointerWheelChanged(object? sender, PointerRoutedEventArgs e) {
        var delta = e.GetCurrentPoint(_timelineCanvas).Properties.MouseWheelDelta;
        if ((e.KeyModifiers & VirtualKeyModifiers.Control) != 0) {
            // Zoom horizontal
            double zoomFactor = delta > 0 ? 1.2 : 0.8;
            var oldPixelsPerHour = _viewport.TimeAxis.PixelsPerHour;
            _viewport.TimeAxis.PixelsPerHour = Math.Clamp(oldPixelsPerHour * zoomFactor, 5, 300);
            // Adjust offset to keep cursor position
            var pointerX = e.GetCurrentPoint(_timelineCanvas).Position.X;
            var hoursAtCursor = _viewport.TimeAxis.ScreenToHours(pointerX);
            _viewport.TimeAxis.OffsetHours = hoursAtCursor - (pointerX / _viewport.TimeAxis.PixelsPerHour);
            UpdateHScrollBar();
            InvalidateTimeline();
        }
        else if ((e.KeyModifiers & VirtualKeyModifiers.Shift) != 0) {
            // Scroll horizontal
            double scrollDelta = delta > 0 ? -50 : 50; // Adjust step
            _viewport.TimeAxis.ScrollByPixels(scrollDelta);
            UpdateHScrollBar();
            InvalidateTimeline();
        }
        else {
            // Scroll vertical
            double scrollDelta = delta > 0 ? -1 : 1; // Adjust step
            _viewport.SeriesAxis.OffsetUnits += scrollDelta;
            UpdateVScrollBar();
            InvalidateTimeline();
        }

        e.Handled = true;
    }

    /// <summary>
    /// Updates the horizontal scrollbar properties based on the current viewport state.
    /// </summary>
    private void UpdateHScrollBar() {
        if (_hScroll != null) {
            _hScroll.Minimum = 0;
            _hScroll.Maximum = _viewport.TimeAxis.MaxOffsetHours;
            _hScroll.Value = _viewport.TimeAxis.OffsetHours;
            _hScroll.ViewportSize = _viewport.TimeAxis.ViewportHours;
            _hScroll.LargeChange = _viewport.TimeAxis.ViewportHours;
            _hScroll.SmallChange = _viewport.TimeAxis.ViewportHours / 10;
        }
    }

    /// <summary>
    /// Updates the vertical scrollbar properties based on the current viewport state.
    /// </summary>
    private void UpdateVScrollBar() {
        if (_vScroll != null) {
            _vScroll.Minimum = 0;
            _vScroll.Maximum = _viewport.SeriesAxis.MaxOffsetSteps;
            _vScroll.Value = _viewport.SeriesAxis.OffsetUnits;
            _vScroll.ViewportSize = _viewport.SeriesAxis.ViewportUnits;
            _vScroll.LargeChange = 1;
            _vScroll.SmallChange = 1;
        }
    }

    /// <summary>
    /// Adjusts the series height to fit all series within the current viewport.
    /// </summary>
    public void ZoomSeriesToFit() {
        _viewport.ZoomSeriesToFit();
        SeriesHeight = _viewport.SeriesAxis.SeriesHeight;
        UpdateVScrollBar();
        InvalidateTimeline();
    }
}
