namespace McTimeline;

public sealed partial class McTimeline : Control {

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
                var legend = _legendItemPool.GetElement();
                legend.LegendText = SeriesCollection[index].Title ?? string.Empty;
                legend.Style = LegendItemStyle;
                element = legend;
                _visibleLegendItems[index] = element;
                _legendCanvas.Children.Add(element);
            }
            else {
                if (element is McLegend legend) {
                    legend.LegendText = SeriesCollection[index].Title ?? string.Empty;
                }
                element.Style = LegendItemStyle;
            }

            element.Width = width;
            element.Height = itemHeight;
            Canvas.SetTop(element, _viewport.SeriesAxis.UnitsToScreen(index));
        }
    }

    /// <summary>
    /// Removes legend items that fall outside the specified visible range.
    /// </summary>
    /// <param name="startIndex">The start index of the visible range.</param>
    /// <param name="endIndex">The end index of the visible range.</param>
    private void RemoveLegendItemsOutsideRange(int startIndex, int endIndex) {
        if (_legendCanvas == null) {
            return;
        }

        // Collect keys to remove without modifying collection during enumeration
        Span<int> keysBuffer = stackalloc int[_visibleLegendItems.Count];
        int count = 0;
        foreach (var key in _visibleLegendItems.Keys) {
            if (key < startIndex || key > endIndex) {
                keysBuffer[count++] = key;
            }
        }

        // Remove items
        for (int i = 0; i < count; i++) {
            int key = keysBuffer[i];
            if (_visibleLegendItems.TryGetValue(key, out var element)) {
                _legendCanvas.Children.Remove(element);
                _visibleLegendItems.Remove(key);
                if (element is McLegend legend) {
                    _legendItemPool.RecycleElement(legend);
                }
            }
        }
    }

    /// <summary>
    /// Clears all legend visuals from the canvas and recycles them to the pool.
    /// </summary>
    private void ClearLegendVisuals() {
        _legendCanvas?.Children.Clear();
        foreach (var element in _visibleLegendItems.Values) {
            if (element is McLegend legend) {
                _legendItemPool.RecycleElement(legend);
            }
        }
        _visibleLegendItems.Clear();
    }

    /// <summary>
    /// Draws the timeline items for all visible series within the viewport.
    /// </summary>
    private void DrawTimeline() {
        if (_timelineCanvas == null || SeriesCollection == null) {
            ClearTimelineVisuals();
            return;
        }

        int startSeriesIndex = _viewport.VisibleSeriesStartIndex;
        int endSeriesIndex = _viewport.VisibleSeriesEndIndex;

        if (startSeriesIndex > endSeriesIndex) {
            ClearTimelineVisuals();
            return;
        }

        var (visibleStart, visibleEnd) = _viewport.VisibleTimeRange;

        // Mark all existing items as potentially removable.
        // This implements a "Mark and Sweep" strategy for virtualization:
        // 1. Mark all current visual elements as candidates for removal (Tag = false).
        // 2. Iterate through the visible data range; if an element is reused, mark it as kept (Tag = true).
        // 3. Any element remaining with Tag = false is removed in RemoveInvisibleTimelineItems().
        for (int i = _timelineCanvas.Children.Count - 1; i >= 0; i--) {
            if (_timelineCanvas.Children[i] is FrameworkElement element) {
               element.Tag = false;
            }
        }

        // Draw items for each visible series
        for (int seriesIndex = startSeriesIndex; seriesIndex <= endSeriesIndex; seriesIndex++) {
            if (seriesIndex < 0 || seriesIndex >= SeriesCollection.Count) {
                continue;
            }

            var series = SeriesCollection[seriesIndex];
            if (series.Items == null) {
                continue;
            }

            foreach (var item in series.Items) {
                // Skip items outside the visible time range
                if (item.End < visibleStart || item.Start > visibleEnd) {
                    continue;
                }

                // Find existing element or create new one
                Controls.ITimelineBar? bar = FindTimelineBar(seriesIndex, item.IdKey);
                if (bar == null) {
                    var element = _seriesItemPool.GetElement();
                    bar = element as Controls.ITimelineBar;
                    if (bar != null) {
                        bar.SeriesIndex = seriesIndex;
                        bar.ItemKey = item.IdKey;
                        _timelineCanvas.Children.Add(element);
                    }
                }

                if (bar != null && bar is FrameworkElement barElement) {
                    barElement.Tag = true; // Mark as visible
                    UpdateTimelineItemVisual(barElement, bar, item, seriesIndex);
                }
            }
        }

        // Remove items that are no longer visible
        RemoveInvisibleTimelineItems();
    }

    /// <summary>
    /// Finds an existing timeline bar for a specific series and item.
    /// </summary>
    /// <param name="seriesIndex">The series index to search for.</param>
    /// <param name="itemKey">The item key to search for.</param>
    /// <returns>The found timeline bar, or null if not found.</returns>
    private Controls.ITimelineBar? FindTimelineBar(int seriesIndex, string itemKey) {
        if (_timelineCanvas == null) return null;

        for (int i = 0; i < _timelineCanvas.Children.Count; i++) {
            if (_timelineCanvas.Children[i] is Controls.ITimelineBar bar &&
                bar.SeriesIndex == seriesIndex &&
                bar.ItemKey == itemKey) {
                return bar;
            }
        }
        return null;
    }

    /// <summary>
    /// Updates the visual properties of a timeline item element.
    /// </summary>
    /// <param name="element">The framework element to update.</param>
    /// <param name="bar">The timeline bar interface.</param>
    /// <param name="item">The timeline item data.</param>
    /// <param name="seriesIndex">The index of the series containing the item.</param>
    private void UpdateTimelineItemVisual(FrameworkElement element, Controls.ITimelineBar bar, McTimelineItem item, int seriesIndex) {
        if (_timelineCanvas == null) {
            return;
        }

        var (x, y, width) = _viewport.GetItemPosition(item, seriesIndex);

        // Set common ITimelineBar properties
        bar.ItemToolTip = McTimeline.CreateItemToolTip(item);
        
        // Apply style
        element.Style = TimelineItemStyle;
        
        // Calculate height considering margins from style
        double totalHeight = _viewport.SeriesHeight;
        if (element.Margin.Top > 0 || element.Margin.Bottom > 0) {
            totalHeight = Math.Max(0, _viewport.SeriesHeight);
        }
        element.Height = totalHeight;
        element.Width = Math.Max(0, width);
        
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
    }

    /// <summary>
    /// Creates a tooltip for a timeline item.
    /// </summary>
    /// <param name="item">The timeline item.</param>
    /// <returns>The tooltip content as a string.</returns>
    private static string CreateItemToolTip(McTimelineItem item) {
        return $"{item.Title}\n{item.Description}\n{item.Start:g} - {item.End:g}";
    }

    /// <summary>
    /// Removes timeline items that are marked as not visible.
    /// </summary>
    private void RemoveInvisibleTimelineItems() {
        if (_timelineCanvas == null) {
            return;
        }

        for (int i = _timelineCanvas.Children.Count - 1; i >= 0; i--) {
            if (_timelineCanvas.Children[i] is FrameworkElement element && 
                element.Tag is bool visible && !visible) {
                _timelineCanvas.Children.RemoveAt(i);
                _seriesItemPool.RecycleElement(element);
            }
        }
    }

    /// <summary>
    /// Clears all timeline item visuals from the canvas.
    /// </summary>
    private void ClearTimelineVisuals() {
        if (_timelineCanvas == null) {
            return;
        }

        for (int i = _timelineCanvas.Children.Count - 1; i >= 0; i--) {
            if (_timelineCanvas.Children[i] is FrameworkElement element) {
                _seriesItemPool.RecycleElement(element);
            }
        }
        _timelineCanvas.Children.Clear();
    }
}