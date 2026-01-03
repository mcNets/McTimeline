namespace McTimeline.Controls;

/// <summary>
/// Defines the contract for timeline bar elements that can be displayed on the timeline.
/// Implement this interface to create custom timeline bar controls.
/// </summary>
public interface ITimelineBar {
    /// <summary>
    /// Gets or sets the text displayed on the timeline bar.
    /// </summary>
    string ItemText { get; set; }

    /// <summary>
    /// Gets or sets the series index this bar belongs to.
    /// Used for identifying which series row the bar should be rendered in.
    /// </summary>
    int SeriesIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the unique item key this bar represents.
    /// Used for matching visual elements to data items during virtualization.
    /// </summary>
    string ItemKey { get; set; }
    
    /// <summary>
    /// Gets or sets the tooltip content for the timeline bar.
    /// This can be any object, including strings or custom UI elements.
    /// </summary>
    object ItemToolTip { get; set; }
}
