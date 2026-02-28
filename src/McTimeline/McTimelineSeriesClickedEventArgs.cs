namespace McTimeline;

/// <summary>
/// Event data for timeline series click notifications.
/// </summary>
public sealed class McTimelineSeriesClickedEventArgs : EventArgs {
    public McTimelineSeriesClickedEventArgs(McTimelineSeries series, int seriesIndex, McTimelinePointerButton button) {
        Series = series;
        SeriesIndex = seriesIndex;
        Button = button;
    }

    /// <summary>
    /// Series that was clicked.
    /// </summary>
    public McTimelineSeries Series { get; }

    /// <summary>
    /// Index of the clicked series.
    /// </summary>
    public int SeriesIndex { get; }

    /// <summary>
    /// Mouse button used for the click.
    /// </summary>
    public McTimelinePointerButton Button { get; }
}
