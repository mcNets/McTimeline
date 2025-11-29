using System.Collections.Specialized;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using McTimeline.Viewport;
using McTimeline.Controls;
using McTimeline.Pools;

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
    private Canvas? _legendCanvas;
    private ScrollBar? _hScroll;
    private ScrollBar? _vScroll;
    private ColumnDefinition? _legendColumn;
    private ColumnDefinition? _timeScaleLegendColumn;
    private GridLength _legendColumnWidth;
    private GridLength _timeScaleLegendColumnWidth;

    private readonly McTimelineViewport _viewport;
    private readonly Dictionary<int, FrameworkElement> _visibleLegendItems = new();
    private readonly IMcLegendItemPool _legendItemPool;

    #endregion

    public McTimeline() {
        this.DefaultStyleKey = typeof(McTimeline);
        _viewport = new McTimelineViewport();
        _legendItemPool = new McLegendItemPool(LegendStyle);
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

    private void OnTimelineCanvasSizeChanged(object sender, SizeChangedEventArgs e) {
        // Update viewport size when canvas size changes
        _viewport.OnSizeChanged(e.NewSize);
        _viewport.RefreshVisibleSeriesRange();
        UpdateHScrollBar();
        UpdateVScrollBar();
        InvalidateTimeline();
    }

    // private void OnLegendCanvasSizeChanged(object sender, SizeChangedEventArgs e) {
    //     // Update vertical axis viewport pixels when legend canvas size changes
    //     _viewport.SeriesAxis.ViewportPixels = e.NewSize.Height;
    //     _viewport.RefreshVisibleSeriesRange();
    //     UpdateVScrollBar();
    //     InvalidateTimeline();
    // }

    private void OnTimelineScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e) {
        // Update scroll offsets when user scrolls
        if (_timelineScroll != null) {
            _viewport.OnScrollChanged(_timelineScroll.HorizontalOffset, _timelineScroll.VerticalOffset);
            InvalidateTimeline();
        }
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
        SyncSeriesAxisWithCollection();
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
        SyncSeriesAxisWithCollection();
        InvalidateTimeline();
    }

    private void OnSeriesItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        InvalidateTimeline();
    }

    private void SyncSeriesAxisWithCollection() {
        _viewport.SeriesAxis.ContentUnits = SeriesCollection?.Count ?? 0;
        _viewport.RefreshVisibleSeriesRange();
    }

    /// <summary>
    /// Draws the legend for the series in the legend area.
    /// Only visible series within the viewport are rendered.
    /// </summary>
    private void DrawLegend() {
        if (_legendCanvas == null) {
            _visibleLegendItems.Clear();
            return;
        }

        if (SeriesCollection == null || SeriesCollection.Count == 0) {
            ClearLegendVisuals();
            return;
        }

        int startIndex = _viewport.VisibleSeriesStartIndex;
        int endIndex = _viewport.VisibleSeriesEndIndex;

        if (startIndex > endIndex) {
            ClearLegendVisuals();
            return;
        }

        RemoveLegendItemsOutsideRange(startIndex, endIndex);

        double width = _legendCanvas.ActualWidth;
        double itemHeight = _viewport.SeriesHeight;

        for (int index = startIndex; index <= endIndex; index++) {
            if (!_visibleLegendItems.TryGetValue(index, out FrameworkElement? element) || element == null) {
                //element = CreateLegendItem(SeriesCollection[index]);
                element = _legendItemPool.GetLegendItem(SeriesCollection[index].Title ?? string.Empty);
                element.Style = LegendItemStyle;
                _visibleLegendItems[index] = element;
                _legendCanvas.Children.Add(element);
            }
            else {
                //UpdateLegendItem(element, SeriesCollection[index]);
                element.Style = LegendItemStyle;
            }

            element.Width = width;
            element.Height = itemHeight;
            Canvas.SetTop(element, _viewport.SeriesAxis.UnitsToScreen(index));
        }
    }

    private void RemoveLegendItemsOutsideRange(int startIndex, int endIndex) {
        if (_legendCanvas == null) {
            return;
        }

        //List<int> keysToRemove = new();
        foreach (var key in _visibleLegendItems.Keys) {
            if (key < startIndex || key > endIndex) {
                //keysToRemove.Add(key);
                if (_visibleLegendItems.TryGetValue(key, out var element)) {
                    _legendCanvas.Children.Remove(element);
                    _visibleLegendItems.Remove(key);
                    _legendItemPool.RecycleLegendItem(element);
                }
            }
        }

        // foreach (int key in keysToRemove) {
        //     if (_visibleLegendItems.TryGetValue(key, out var element)) {
        //         _legendCanvas.Children.Remove(element);
        //         _visibleLegendItems.Remove(key);
        //     }
        // }
    }

    private void ClearLegendVisuals() {
        _legendCanvas?.Children.Clear();
        foreach (var element in _visibleLegendItems.Values) {
            _legendItemPool.RecycleLegendItem(element);
        }
        _visibleLegendItems.Clear();
    }

    private FrameworkElement CreateLegendItem(McTimelineSeries series) {
        // var border = new Border {
        //     Style = LegendItemStyle
        // };
        // border.Child = new TextBlock {
        //     Text = series.Title,
        //     VerticalAlignment = VerticalAlignment.Center,
        //     HorizontalAlignment = HorizontalAlignment.Center
        // };
        // return border;
        return _legendItemPool.GetLegendItem(series.Title ?? string.Empty);
    }

    // private static void UpdateLegendItem(Border border, McTimelineSeries series) {
    //     if (border.Child is TextBlock textBlock) {
    //         textBlock.Text = series.Title;
    //     }
    // }

    /// <summary>
    /// Invalidates the current timeline, signaling that it should be refreshed or repainted.
    /// </summary>
    /// <remarks>Call this method when changes occur that require the timeline to be updated visually. This
    /// method does not immediately trigger a repaint; the actual update may be deferred depending on the
    /// implementation.</remarks>
    private void InvalidateTimeline() {
        DrawLegend();
        // TODO: Implement timeline drawing
    }

    private void OnHScrollValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        double horizontalOffsetPx = e.NewValue * _viewport.TimeAxis.PixelsPerHour;
        double verticalUnits = _vScroll?.Value ?? 0;
        double verticalOffsetPx = verticalUnits * _viewport.SeriesAxis.SeriesHeight;
        _viewport.OnScrollChanged(horizontalOffsetPx, verticalOffsetPx);
        InvalidateTimeline();
    }

    private void OnVScrollValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        double verticalOffsetPx = e.NewValue * _viewport.SeriesAxis.SeriesHeight;
        double horizontalHours = _hScroll?.Value ?? 0;
        double horizontalOffsetPx = horizontalHours * _viewport.TimeAxis.PixelsPerHour;
        _viewport.OnScrollChanged(horizontalOffsetPx, verticalOffsetPx);
        InvalidateTimeline();
    }

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

    public void ZoomSeriesToFit() {
        _viewport.ZoomSeriesToFit();
        SeriesHeight = _viewport.SeriesAxis.SeriesHeight;
        UpdateVScrollBar();
        InvalidateTimeline();
    }
}
