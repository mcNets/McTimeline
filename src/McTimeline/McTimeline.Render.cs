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
                element = _legendItemPool.GetLegendItem(SeriesCollection[index].Title ?? string.Empty);
                element.Style = LegendItemStyle;
                _visibleLegendItems[index] = element;
                _legendCanvas.Children.Add(element);
            }
            else {
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
                _legendItemPool.RecycleLegendItem(element);
            }
        }
    }

    private void ClearLegendVisuals() {
        _legendCanvas?.Children.Clear();
        foreach (var element in _visibleLegendItems.Values) {
            _legendItemPool.RecycleLegendItem(element);
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

        // Mark all existing items as potentially removable
        for (int i = _timelineCanvas.Children.Count - 1; i >= 0; i--) {
            if (_timelineCanvas.Children[i] is McTimelineBar bar) {
                bar.Tag = false; // Mark as not visible
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
                McTimelineBar? bar = FindTimelineBar(seriesIndex, item.IdKey);
                if (bar == null) {
                    bar = _seriesItemPool.GetSeriesItem(series.Title ?? string.Empty) as McTimelineBar;
                    if (bar != null) {
                        bar.SeriesIndex = seriesIndex;
                        bar.ItemKey = item.IdKey;
                        _timelineCanvas.Children.Add(bar);
                    }
                }

                if (bar != null) {
                    bar.Tag = true; // Mark as visible
                    UpdateTimelineItemVisual(bar, item, seriesIndex);
                }
            }
        }

        // Remove items that are no longer visible
        RemoveInvisibleTimelineItems();
    }

    /// <summary>
    /// Finds an existing timeline bar for a specific series and item.
    /// </summary>
    private McTimelineBar? FindTimelineBar(int seriesIndex, string itemKey) {
        if (_timelineCanvas == null) return null;

        for (int i = 0; i < _timelineCanvas.Children.Count; i++) {
            if (_timelineCanvas.Children[i] is McTimelineBar bar &&
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
    /// <param name="item">The timeline item data.</param>
    /// <param name="seriesIndex">The index of the series containing the item.</param>
    private void UpdateTimelineItemVisual(FrameworkElement element, McTimelineItem item, int seriesIndex) {
        if (_timelineCanvas == null) {
            return;
        }

        var (x, y, width) = _viewport.GetItemPosition(item, seriesIndex);

        if (element is McTimelineBar bar) {
            //bar.ItemText = item.Title;
            bar.ItemToolTip = CreateItemToolTip(item);
            bar.Style = TimelineItemStyle;
            
            // Calculate height considering margins from style
            double totalHeight = _viewport.SeriesHeight;
            if (bar.Margin.Top > 0 || bar.Margin.Bottom > 0) {
                totalHeight = Math.Max(0, _viewport.SeriesHeight);
            }
            bar.Height = totalHeight;
        }

        element.Width = Math.Max(0, width);
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
    }

    /// <summary>
    /// Creates a tooltip for a timeline item.
    /// </summary>
    /// <param name="item">The timeline item.</param>
    /// <returns>The tooltip content as a string.</returns>
    private string CreateItemToolTip(McTimelineItem item) {
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
            if (_timelineCanvas.Children[i] is McTimelineBar bar && bar.Tag is bool visible && !visible) {
                _timelineCanvas.Children.RemoveAt(i);
                _seriesItemPool.RecycleSeriesItem(bar);
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
            if (_timelineCanvas.Children[i] is McTimelineBar bar) {
                _seriesItemPool.RecycleSeriesItem(bar);
            }
        }
        _timelineCanvas.Children.Clear();
    }
}