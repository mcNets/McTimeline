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
    private readonly McTimelineViewport _viewport;
    private Grid? _container;
    // Scale elements
    private Grid? _timeScaleGrid;
    private Canvas? _scaleDaysCanvas;
    private readonly Dictionary<DateTime, TextBlock> _visibleDays = [];
    private readonly McElementPool<TextBlock> _scaleDaysPool;
    private Canvas? _scaleHoursCanvas;
    private readonly List<FrameworkElement> _visibleHours = [];
    private readonly McElementPool<TextBlock> _scaleHoursPool;
    private readonly McElementPool<Border> _hourTickPool;
    private ColumnDefinition? _timeScaleLegendColumn;
    private GridLength _timeScaleLegendColumnWidth;

    private readonly Dictionary<int, FrameworkElement> _visibleLegendItems = [];
    private readonly McElementPool<McLegend> _legendItemPool; 
    private McElementPool<FrameworkElement> _seriesItemPool;
    private Border? _legendBorder;
    private ItemsRepeater? _seriesRepeater;
    private ScrollViewer? _timelineScroll;
    private Canvas? _timelineCanvas;
    private Canvas? _legendCanvas;
    private ScrollBar? _hScroll;
    private ScrollBar? _vScroll;
    private ColumnDefinition? _legendColumn;
    private GridLength _legendColumnWidth;
    #endregion

    /// <summary>
    /// Raised when a timeline item bar is clicked.
    /// </summary>
    public event EventHandler<McTimelineItemClickedEventArgs>? ItemClicked;

    /// <summary>
    /// Raised when a legend series is clicked.
    /// </summary>
    public event EventHandler<McTimelineSeriesClickedEventArgs>? SeriesClicked;

    /// <summary>
    /// Initializes a new instance of the <see cref="McTimeline"/> class.
    /// </summary>
    public McTimeline() {
        this.DefaultStyleKey = typeof(McTimeline);
        _viewport = new McTimelineViewport();
        _legendItemPool = new McElementPool<McLegend>(LegendStyle);
        _seriesItemPool = new McElementPool<FrameworkElement>(() => CreateTimelineBarInstance(), TimelineItemStyle);
        _scaleDaysPool = new McElementPool<TextBlock>(TimeScaleDaysStyle);
        _scaleHoursPool = new McElementPool<TextBlock>(TimeScaleDaysStyle);
        _hourTickPool = new McElementPool<Border>(TimeScaleTickStyle);
    }

    /// <summary>
    /// Called when the template is applied to the control.
    /// </summary>
    protected override void OnApplyTemplate() {
        base.OnApplyTemplate();

        _container = GetTemplateChild("PART_Container") as Grid;
        _timeScaleGrid = GetTemplateChild("PART_TimeScaleGrid") as Grid;
        _legendBorder = GetTemplateChild("PART_LegendBorder") as Border;
        _seriesRepeater = GetTemplateChild("PART_SeriesRepeater") as ItemsRepeater;
        _timelineScroll = GetTemplateChild("PART_TimelineScroll") as ScrollViewer;
        _hScroll = GetTemplateChild("PART_HScroll") as ScrollBar;
        _vScroll = GetTemplateChild("PART_VScroll") as ScrollBar;
        _scaleDaysCanvas = GetTemplateChild("PART_TimeScaleDays") as Canvas;
        _scaleHoursCanvas = GetTemplateChild("PART_TimeScaleHours") as Canvas;
        _timelineCanvas = GetTemplateChild("PART_TimelineCanvas") as Canvas;
        _legendCanvas = GetTemplateChild("PART_LegendCanvas") as Canvas;

        // Adjust legend column width
        if (_legendBorder?.Parent is Grid legendHost && legendHost.ColumnDefinitions.Count > 0) {
            _legendColumn = legendHost.ColumnDefinitions[0];
            _legendColumnWidth = _legendColumn.Width;
        }

        // Adjust time scale legend column width
        if (_timeScaleGrid?.ColumnDefinitions.Count > 0) {
            _timeScaleLegendColumn = _timeScaleGrid.ColumnDefinitions[0];
            _timeScaleLegendColumnWidth = _timeScaleLegendColumn.Width;
        }

        // Subscribe to canvas size changes
        _timelineCanvas?.SizeChanged += OnTimelineCanvasSizeChanged;
        _timelineCanvas?.PointerWheelChanged += OnCanvasPointerWheelChanged;

        _legendCanvas?.SizeChanged += OnLegendCanvasSizeChanged;

        _scaleDaysCanvas?.SizeChanged += OnTimeScaleSizeChanged;
        _scaleHoursCanvas?.SizeChanged += OnTimeScaleSizeChanged;

        // Subscribe to scroll changes
        _timelineScroll?.ViewChanged += OnTimelineScrollViewChanged;
        _hScroll?.ValueChanged += OnHScrollValueChanged;
        _vScroll?.ValueChanged += OnVScrollValueChanged;

        // Initialize time axis
        _viewport.TimeAxis.SetRange(MinDate, MaxDate);
        _viewport.TimeAxis.PixelsPerHour = Math.Clamp(PixelsPerHour, McConstants.MIN_PIXELS_PER_HOUR, McConstants.MAX_PIXELS_PER_HOUR);
        _viewport.TimeAxis.ViewportPixels = _timelineCanvas?.ActualWidth ?? 0;

        // Initialize series axis
        _viewport.SeriesHeight = SeriesHeight;
        _viewport.SeriesAxis.ContentUnits = SeriesCollection?.Count ?? 0;
        _viewport.SeriesAxis.ViewportPixels = _legendCanvas?.ActualHeight ?? 0;

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
        _timelineCanvas!.Clip = new RectangleGeometry { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
        _viewport.OnSizeChanged(e.NewSize);
        _viewport.RefreshVisibleSeriesRange();
        UpdateHScrollBar();
        UpdateVScrollBar();
        InvalidateTimeline();
    }

    /// <summary>
    /// Handles the size changed event of the time scale canvases.
    /// Updates the clipping region and redraws labels to match the new size.
    /// </summary>
    private void OnTimeScaleSizeChanged(object sender, SizeChangedEventArgs e) {
        if (sender == _scaleDaysCanvas) {
            _scaleDaysCanvas.Clip = new RectangleGeometry { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
            DrawDays();
        }
        else if (sender == _scaleHoursCanvas) {
            _scaleHoursCanvas.Clip = new RectangleGeometry { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
            DrawHours();
        }
    }

    /// <summary>
    /// Handles the size changed event of the legend canvas.
    /// Updates the clipping region to match the new size.
    /// </summary>
    private void OnLegendCanvasSizeChanged(object sender, SizeChangedEventArgs e) {
        _legendCanvas!.Clip = new RectangleGeometry { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
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
    /// Handles changes to the series collection when the collection is replaced.
    /// </summary>
    private void OnSeriesCollectionChanged(McTimelineSeriesCollection oldValue, McTimelineSeriesCollection newValue) {
        if (oldValue != null) {
            oldValue.CollectionChanged -= OnSeriesCollectionChanged;
            foreach (var series in oldValue) {
                series.PropertyChanged -= OnSeriesPropertyChanged;
                series.Items.CollectionChanged -= OnSeriesItemsChanged;
            }
        }
        if (newValue != null) {
            newValue.CollectionChanged += OnSeriesCollectionChanged;
            foreach (var series in newValue) {
                series.PropertyChanged += OnSeriesPropertyChanged;
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
                series.PropertyChanged -= OnSeriesPropertyChanged;
                series.Items.CollectionChanged -= OnSeriesItemsChanged;
            }
        }
        if (e.NewItems != null) {
            foreach (McTimelineSeries series in e.NewItems) {
                series.PropertyChanged += OnSeriesPropertyChanged;
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
    /// Handles visual property updates on a series (e.g. style changes).
    /// </summary>
    private void OnSeriesPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(McTimelineSeries.SeriesStyle) || string.IsNullOrEmpty(e.PropertyName)) {
            InvalidateTimeline();
        }
    }

    /// <summary>
    /// Synchronizes the series axis with the current series collection count.
    /// </summary>
    private void SyncSeriesAxisWithCollection() {
        _viewport.SeriesAxis.ContentUnits = SeriesCollection?.Count ?? 0;
        _viewport.RefreshVisibleSeriesRange();
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
    /// Invalidates the current timeline, signaling that it should be refreshed or repainted.
    /// </summary>
    /// <remarks>Call this method when changes occur that require the timeline to be updated visually. This
    /// method does not immediately trigger a repaint; the actual update may be deferred depending on the
    /// implementation.</remarks>
    private void InvalidateTimeline() {
        DrawDays();
        DrawHours();
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
        // Zoom horizontal
        if ((e.KeyModifiers & VirtualKeyModifiers.Control) != 0) {
            double zoomFactor = delta > 0 ? McConstants.ZOOM_IN_FACTOR : McConstants.ZOOM_OUT_FACTOR;
            var pointerX = e.GetCurrentPoint(_timelineCanvas).Position.X;
            // Keep the world hour under the cursor fixed while zooming.
            var hoursAtCursorBeforeZoom = _viewport.TimeAxis.ScreenToHours(pointerX);
            var oldPixelsPerHour = _viewport.TimeAxis.PixelsPerHour;
            PixelsPerHour = Math.Clamp(oldPixelsPerHour * zoomFactor, McConstants.MIN_PIXELS_PER_HOUR, McConstants.MAX_PIXELS_PER_HOUR);
            _viewport.TimeAxis.OffsetHours = hoursAtCursorBeforeZoom - (pointerX / _viewport.TimeAxis.PixelsPerHour);
            UpdateHScrollBar();
            InvalidateTimeline();
        }
        // Scroll horizontal
        else if ((e.KeyModifiers & VirtualKeyModifiers.Shift) != 0) {
            double scrollDelta = delta > 0 ? -1 : 1; // Adjust step
            _viewport.SeriesAxis.OffsetUnits += scrollDelta;
            UpdateVScrollBar();
            InvalidateTimeline();
        }
        // Scroll vertical
        else {
            double scrollDelta = delta > 0 ? -McConstants.SCROLL_DELTA_PIXELS : McConstants.SCROLL_DELTA_PIXELS;
            _viewport.TimeAxis.ScrollByPixels(scrollDelta);
            UpdateHScrollBar();
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
            _hScroll.SmallChange = _viewport.TimeAxis.ViewportHours / McConstants.HSCROLL_SMALL_CHANGE_DIVISOR;
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


    /// <summary>
    /// Handles the pointer pressed event on a timeline item.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be an <see cref="Controls.ITimelineBar"/>.</param>
    /// <param name="e">The pointer event arguments.</param>
    /// <remarks>This method resolves the clicked item and raises the <see cref="ItemClicked"/> event with details about the clicked item and pointer button.</remarks>
    private void OnTimelineItemPointerPressed(object sender, PointerRoutedEventArgs e) {
        if (sender is not Controls.ITimelineBar bar) {
            return;
        }

        if (!TryResolveClickedItem(bar, out McTimelineItem? clickedItem) || clickedItem == null) {
            return;
        }

        ItemClicked?.Invoke(this, new McTimelineItemClickedEventArgs(clickedItem, bar.SeriesIndex, GetPointerButton(e)));
    }

    /// <summary>
    /// Handles the pointer pressed event on a legend item.
    /// </summary>
    /// <param name="sender">The source of the event, expected to be a <see cref="FrameworkElement"/> representing the legend item.</param>
    /// <param name="e">The pointer event arguments.</param>
    /// <remarks>This method resolves the clicked series and raises the <see cref="SeriesClicked"/> event with details about the clicked series and pointer button.</remarks>
    private void OnLegendItemPointerPressed(object sender, PointerRoutedEventArgs e) {
        if (sender is not FrameworkElement legendElement) {
            return;
        }

        if (!TryResolveClickedSeries(legendElement, out int seriesIndex, out McTimelineSeries? clickedSeries) || clickedSeries == null) {
            return;
        }

        SeriesClicked?.Invoke(this, new McTimelineSeriesClickedEventArgs(clickedSeries, seriesIndex, GetPointerButton(e)));
    }

    /// <summary>
    /// Attempts to resolve the clicked timeline item based on the provided bar information.
    /// </summary>
    /// <param name="bar">The timeline bar that was clicked, containing series index and item key information.</param>
    /// <param name="item">When this method returns, contains the resolved <see cref="McTimelineItem"/> if the resolution was successful; otherwise, null.</param>
    /// <returns>True if the clicked item was successfully resolved; otherwise, false.</returns>
    private bool TryResolveClickedItem(Controls.ITimelineBar bar, out McTimelineItem? item) {
        item = null;

        if (SeriesCollection == null || bar.SeriesIndex < 0 || bar.SeriesIndex >= SeriesCollection.Count) {
            return false;
        }

        var series = SeriesCollection[bar.SeriesIndex];
        if (series?.Items == null) {
            return false;
        }

        for (int i = 0; i < series.Items.Count; i++) {
            if (series.Items[i].IdKey == bar.ItemKey) {
                item = series.Items[i];
                return true;
            }
        }

        return false;
    }

    
    /// <summary>
    /// Attempts to resolve the clicked series based on the provided legend element.
    /// </summary>
    /// <param name="legendElement">The legend element that was clicked.</param>
    /// <param name="seriesIndex">When this method returns, contains the index of the resolved series if the resolution was successful; otherwise, -1.</param>
    /// <param name="series">When this method returns, contains the resolved <see cref="McTimelineSeries"/> if the resolution was successful; otherwise, null.</param>
    /// <returns>True if the clicked series was successfully resolved; otherwise, false.</returns>
    private bool TryResolveClickedSeries(FrameworkElement legendElement, out int seriesIndex, out McTimelineSeries? series) {
        seriesIndex = -1;
        series = null;

        foreach (var pair in _visibleLegendItems) {
            if (!ReferenceEquals(pair.Value, legendElement)) {
                continue;
            }

            seriesIndex = pair.Key;
            break;
        }

        if (seriesIndex < 0 || SeriesCollection == null || seriesIndex >= SeriesCollection.Count) {
            return false;
        }

        series = SeriesCollection[seriesIndex];
        return series != null;
    }

    /// <summary>
    /// Determines which pointer button was pressed based on the provided pointer event arguments.
    /// </summary>
    /// <param name="e">The pointer event arguments containing information about the pointer state.</param>
    /// <returns>A <see cref="McTimelinePointerButton"/> value indicating which pointer button was pressed.</returns>
    /// <remarks>This method checks the pointer properties to determine which button was pressed, including left, right, middle, and extended buttons. If no known button is detected as pressed, it returns <see cref="McTimelinePointerButton.Unknown"/>.</remarks>
    private static McTimelinePointerButton GetPointerButton(PointerRoutedEventArgs e) {
        var props = e.GetCurrentPoint(null).Properties;
        if (props.IsLeftButtonPressed) {
            return McTimelinePointerButton.Left;
        }
        if (props.IsRightButtonPressed) {
            return McTimelinePointerButton.Right;
        }
        if (props.IsMiddleButtonPressed) {
            return McTimelinePointerButton.Middle;
        }
        if (props.IsXButton1Pressed) {
            return McTimelinePointerButton.X1;
        }
        if (props.IsXButton2Pressed) {
            return McTimelinePointerButton.X2;
        }

        return McTimelinePointerButton.Unknown;
    }
}
