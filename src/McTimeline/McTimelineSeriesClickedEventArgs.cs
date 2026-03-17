namespace McTimeline;

/// <summary>
/// Event data for timeline series click notifications.
/// </summary>
public sealed class McTimelineSeriesClickedEventArgs(McTimelineSeries series, int seriesIndex, McTimelinePointerButton button) : EventArgs {

    /// <summary>
    /// Series that was clicked.
    /// </summary>
    public McTimelineSeries Series { get; } = series;

    /// <summary>
    /// Index of the clicked series.
    /// </summary>
    public int SeriesIndex { get; } = seriesIndex;

    /// <summary>
    /// Mouse button used for the click.
    /// </summary>
    public McTimelinePointerButton Button { get; } = button;
}
