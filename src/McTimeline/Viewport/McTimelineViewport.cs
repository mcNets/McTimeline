using System;
using Windows.Foundation;

namespace McTimeline.Viewport;

/// <summary>
/// Manages the viewport for both time (horizontal) and vertical axes in the McTimeline control.
/// Centralizes event handling for size changes, scrolling, and range updates.
/// Provides methods for generating ticks, checking series visibility, and calculating item positions.
/// </summary>
public sealed class McTimelineViewport {
    private int _visibleSeriesStartIndex;
    private int _visibleSeriesEndIndex = -1;

    /// <summary>
    /// Gets the time axis for horizontal positioning and time-based calculations.
    /// </summary>
    public McVirtualTimeAxis TimeAxis { get; } = new();

    /// <summary>
    /// Gets the vertical axis for vertical positioning and visibility calculations.
    /// </summary>
    public McVirtualSeriesAxis SeriesAxis { get; } = new();

    /// <summary>
    /// Gets or sets the height of each series in pixels, used for vertical positioning and visibility checks.
    /// </summary>
    public double SeriesHeight {
        get => SeriesAxis.SeriesHeight;
        set {
            SeriesAxis.SeriesHeight = value;
            UpdateVisibleSeriesRange();
        }
    }

    /// <summary>
    /// Gets the first visible series index within the viewport.
    /// Returns 0 when no series are available.
    /// </summary>
    public int VisibleSeriesStartIndex => _visibleSeriesStartIndex;

    /// <summary>
    /// Gets the last visible series index within the viewport.
    /// Returns -1 when no series are available.
    /// </summary>
    public int VisibleSeriesEndIndex => _visibleSeriesEndIndex;

    /// <summary>
    /// Occurs when the visible series range changes.
    /// </summary>
    public event EventHandler? VisibleSeriesRangeChanged;

    /// <summary>
    /// Updates the viewport sizes when the canvas or container size changes.
    /// </summary>
    /// <param name="newSize">The new size of the viewport.</param>
    public void OnSizeChanged(Size newSize) {
        TimeAxis.ViewportPixels = newSize.Width;
        SeriesAxis.ViewportPixels = newSize.Height;
        UpdateVisibleSeriesRange();
    }

    /// <summary>
    /// Updates the scroll offsets when the user scrolls horizontally or vertically.
    /// </summary>
    /// <param name="horizontalOffset">The horizontal scroll offset in pixels.</param>
    /// <param name="verticalOffset">The vertical scroll offset in pixels.</param>
    public void OnScrollChanged(double horizontalOffset, double verticalOffset) {
        TimeAxis.OffsetHours = horizontalOffset / TimeAxis.PixelsPerHour;
        SeriesAxis.OffsetUnits = verticalOffset / SeriesAxis.SeriesHeight;
        UpdateVisibleSeriesRange();
    }

    /// <summary>
    /// Sets the time range for the timeline.
    /// </summary>
    /// <param name="minDate">The minimum date.</param>
    /// <param name="maxDate">The maximum date.</param>
    public void SetTimeRange(DateTime minDate, DateTime maxDate) {
        TimeAxis.SetRange(minDate, maxDate);
    }

    /// <summary>
    /// Sets the vertical range in units.
    /// </summary>
    /// <param name="minUnits">The minimum units.</param>
    /// <param name="maxUnits">The maximum units.</param>
    public void SetSeriesRange(double minUnits, double maxUnits) {
        SeriesAxis.SetRange(minUnits, maxUnits);
        UpdateVisibleSeriesRange();
    }

    /// <summary>
    /// Gets the visible time range as a tuple of start and end dates.
    /// </summary>
    public (DateTime Start, DateTime End) VisibleTimeRange => TimeAxis.VisibleDateRange;

    /// <summary>
    /// Gets the visible vertical range as a tuple of start and end units.
    /// </summary>
    public (double Start, double End) VisibleSeriesRange => SeriesAxis.VisibleUnitsRange;

    /// <summary>
    /// Forces a recalculation of the visible series index range.
    /// Call this after external changes to the axis that the viewport does not track directly.
    /// </summary>
    public void RefreshVisibleSeriesRange() => UpdateVisibleSeriesRange();

    private void UpdateVisibleSeriesRange() {
        var (topUnits, bottomUnits) = SeriesAxis.VisibleUnitsRange;
        int minIndex = (int)Math.Floor(SeriesAxis.MinUnits);
        int totalSeries = (int)Math.Max(0, Math.Floor(SeriesAxis.ContentUnits + 1e-6));
        int maxIndex = totalSeries > 0 ? minIndex + totalSeries - 1 : minIndex - 1;

        int newStart = totalSeries == 0
            ? minIndex
            : (int)Math.Clamp(Math.Floor(topUnits), minIndex, maxIndex);

        int newEnd = totalSeries == 0
            ? minIndex - 1
            : (int)Math.Clamp((int)Math.Ceiling(bottomUnits) - 1, minIndex, maxIndex);

        if (newStart != _visibleSeriesStartIndex || newEnd != _visibleSeriesEndIndex) {
            _visibleSeriesStartIndex = newStart;
            _visibleSeriesEndIndex = newEnd;
            VisibleSeriesRangeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Determines whether a series at the specified index is visible in the viewport.
    /// </summary>
    /// <param name="seriesIndex">The index of the series.</param>
    /// <returns>true if the series is visible; otherwise, false.</returns>
    public bool IsSeriesVisible(int seriesIndex) {
        return SeriesAxis.Intersects(seriesIndex, 1);
    }

    /// <summary>
    /// Generates tick positions and labels for the days scale.
    /// </summary>
    /// <returns>An enumerable of tuples containing the X position and label for each day tick.</returns>
    public IEnumerable<(double X, string Label)> GetDayTicks() {
        var (left, right) = TimeAxis.VisibleHoursRange;
        DateTime start = TimeAxis.HoursToDate(Math.Floor(left / 24) * 24);
        for (DateTime d = start; d <= TimeAxis.HoursToDate(right); d = d.AddDays(1)) {
            double x = TimeAxis.TimeToScreen(d);
            yield return (x, d.ToString("dd/MM"));
        }
    }

    /// <summary>
    /// Generates tick positions and labels for the hours scale.
    /// </summary>
    /// <returns>An enumerable of tuples containing the X position and label for each hour tick.</returns>
    public IEnumerable<(double X, string Label)> GetHourTicks() {
        var (left, right) = TimeAxis.VisibleHoursRange;
        for (double h = Math.Ceiling(left); h <= right; h += 1) {
            double x = TimeAxis.HoursToScreen(h);
            yield return (x, $"{h % 24}:00");
        }
    }

    /// <summary>
    /// Calculates the position and size of a timeline item on the screen.
    /// </summary>
    /// <param name="item">The timeline item.</param>
    /// <param name="seriesIndex">The index of the series containing the item.</param>
    /// <returns>A tuple containing the X position, Y position, and width of the item.</returns>
    public (double X, double Y, double Width) GetItemPosition(McTimelineItem item, int seriesIndex) {
        double x = TimeAxis.TimeToScreen(item.Start);
        double y = SeriesAxis.UnitsToScreen(seriesIndex);
        double width = TimeAxis.DurationToPixels(item.End - item.Start);
        return (x, y, width);
    }
}