using System.Collections.Specialized;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using McTimeline.Viewport;
using McTimeline.Controls;
using McTimeline.Pools;

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

        foreach (var key in _visibleLegendItems.Keys) {
            if (key < startIndex || key > endIndex) {
                if (_visibleLegendItems.TryGetValue(key, out var element)) {
                    _legendCanvas.Children.Remove(element);
                    _visibleLegendItems.Remove(key);
                    _legendItemPool.RecycleLegendItem(element);
                }
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
}