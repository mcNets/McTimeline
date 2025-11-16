using Windows.Foundation;
using System.Collections.Specialized;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Windows.System;

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

    #endregion

    public McTimeline() {
        this.DefaultStyleKey = typeof(McTimeline);
        _viewport = new McTimelineViewport();
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

        if (_legendCanvas != null) {
            _legendCanvas.SizeChanged += OnLegendCanvasSizeChanged;
        }

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

        // Initialize vertical axis
        _viewport.SeriesHeight = SeriesHeight;
        _viewport.VerticalAxis.ContentUnits = SeriesCollection?.Count ?? 0;
        if (_legendCanvas != null) {
            _viewport.VerticalAxis.ViewportPixels = _legendCanvas.ActualHeight;
        }

        UpdateHScroll();
        UpdateVScroll();

        UpdateLegendVisibility();
    }

    private void OnTimelineCanvasSizeChanged(object sender, SizeChangedEventArgs e) {
        // Update viewport size when canvas size changes
        _viewport.OnSizeChanged(e.NewSize);
        UpdateHScroll();
        UpdateVScroll();
        InvalidateTimeline();
    }

    private void OnLegendCanvasSizeChanged(object sender, SizeChangedEventArgs e) {
        // Update vertical axis viewport pixels when legend canvas size changes
        _viewport.VerticalAxis.ViewportPixels = e.NewSize.Height;
        UpdateVScroll();
        InvalidateTimeline();
    }

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
        _viewport.VerticalAxis.ContentUnits = SeriesCollection.Count;
        InvalidateTimeline();
    }

    private void OnSeriesItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        InvalidateTimeline();
    }

    /// <summary>
    /// Draws the legend for the series in the legend area.
    /// </summary>
    private void DrawLegend() {
        if (_legendCanvas != null) {
            _legendCanvas.Children.Clear();
            double y = 0;
            foreach (var series in SeriesCollection) {
                var textBlock = new TextBlock {
                    Text = series.Title,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, y, 0, 0)
                };
                _legendCanvas.Children.Add(textBlock);
                y += _viewport.SeriesHeight;
            }
        }
    }

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
        _viewport.OnScrollChanged(e.NewValue, _vScroll?.Value ?? 0);
        InvalidateTimeline();
    }

    private void OnVScrollValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        _viewport.OnScrollChanged(_hScroll?.Value ?? 0, e.NewValue);
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
            UpdateHScroll();
            InvalidateTimeline();
        } else {
            // Scroll horizontal
            double scrollDelta = delta > 0 ? -50 : 50; // Adjust step
            _viewport.TimeAxis.ScrollByPixels(scrollDelta);
            UpdateHScroll();
            InvalidateTimeline();
        }
        e.Handled = true;
    }

    private void UpdateHScroll() {
        if (_hScroll != null) {
            _hScroll.Minimum = 0;
            _hScroll.Maximum = _viewport.TimeAxis.MaxOffsetHours;
            _hScroll.Value = _viewport.TimeAxis.OffsetHours;
            _hScroll.ViewportSize = _viewport.TimeAxis.ViewportHours;
            _hScroll.LargeChange = _viewport.TimeAxis.ViewportHours;
            _hScroll.SmallChange = _viewport.TimeAxis.ViewportHours / 10;
        }
    }

    private void UpdateVScroll() {
        if (_vScroll != null) {
            _vScroll.Minimum = 0;
            _vScroll.Maximum = _viewport.VerticalAxis.MaxOffsetUnits;
            _vScroll.Value = _viewport.VerticalAxis.OffsetUnits;
            _vScroll.ViewportSize = _viewport.VerticalAxis.ViewportUnits;
            _vScroll.LargeChange = _viewport.VerticalAxis.ViewportUnits;
            _vScroll.SmallChange = _viewport.VerticalAxis.ViewportUnits / 10;
        }
    }
}
