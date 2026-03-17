namespace McTimeline;

/// <summary>
/// Event data for timeline item click notifications.
/// </summary>
public sealed class McTimelineItemClickedEventArgs(McTimelineItem item, int seriesIndex, McTimelinePointerButton button) : EventArgs {

    /// <summary>
    /// Timeline item that was clicked.
    /// </summary>
    public McTimelineItem Item { get; } = item;

    /// <summary>
    /// Series index containing the clicked item.
    /// </summary>
    public int SeriesIndex { get; } = seriesIndex;

    /// <summary>
    /// Mouse button used for the click.
    /// </summary>
    public McTimelinePointerButton Button { get; } = button;
}
