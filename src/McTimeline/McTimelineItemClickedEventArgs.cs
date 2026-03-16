namespace McTimeline;

/// <summary>
/// Event data for timeline item click notifications.
/// </summary>
public sealed class McTimelineItemClickedEventArgs : EventArgs {
    public McTimelineItemClickedEventArgs(McTimelineItem item, int seriesIndex, McTimelinePointerButton button) {
        Item = item;
        SeriesIndex = seriesIndex;
        Button = button;
    }

    /// <summary>
    /// Timeline item that was clicked.
    /// </summary>
    public McTimelineItem Item { get; }

    /// <summary>
    /// Series index containing the clicked item.
    /// </summary>
    public int SeriesIndex { get; }

    /// <summary>
    /// Mouse button used for the click.
    /// </summary>
    public McTimelinePointerButton Button { get; }
}
