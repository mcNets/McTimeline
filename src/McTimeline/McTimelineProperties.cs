namespace McTimeline;

public sealed partial class McTimeline
{
    #region Style Dependency Properties

    /// <summary>
    /// Gets or sets the style for the time scale grid (top section).
    /// </summary>
    public Style? TimeScaleStyle
    {
        get => (Style?)GetValue(TimeScaleStyleProperty);
        set => SetValue(TimeScaleStyleProperty, value);
    }

    public static readonly DependencyProperty TimeScaleStyleProperty =
        DependencyProperty.Register(
            nameof(TimeScaleStyle),
            typeof(Style),
            typeof(McTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the style for the legend border (left section).
    /// </summary>
    public Style? LegendStyle
    {
        get => (Style?)GetValue(LegendStyleProperty);
        set => SetValue(LegendStyleProperty, value);
    }

    public static readonly DependencyProperty LegendStyleProperty =
        DependencyProperty.Register(
            nameof(LegendStyle),
            typeof(Style),
            typeof(McTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the style for each legend item border.
    /// </summary>
    public Style? LegendItemStyle
    {
        get => (Style?)GetValue(LegendItemStyleProperty);
        set => SetValue(LegendItemStyleProperty, value);
    }

    public static readonly DependencyProperty LegendItemStyleProperty =
        DependencyProperty.Register(
            nameof(LegendItemStyle),
            typeof(Style),
            typeof(McTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the style for the timeline ScrollViewer.
    /// </summary>
    public Style? TimelineScrollStyle
    {
        get => (Style?)GetValue(TimelineScrollStyleProperty);
        set => SetValue(TimelineScrollStyleProperty, value);
    }

    public static readonly DependencyProperty TimelineScrollStyleProperty =
        DependencyProperty.Register(
            nameof(TimelineScrollStyle),
            typeof(Style),
            typeof(McTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the style for the timeline Canvas.
    /// </summary>
    public Style? TimelineCanvasStyle
    {
        get => (Style?)GetValue(TimelineCanvasStyleProperty);
        set => SetValue(TimelineCanvasStyleProperty, value);
    }

    public static readonly DependencyProperty TimelineCanvasStyleProperty =
        DependencyProperty.Register(
            nameof(TimelineCanvasStyle),
            typeof(Style),
            typeof(McTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the custom DataTemplate for legend items.
    /// </summary>
    public DataTemplate? LegendItemTemplate
    {
        get => (DataTemplate?)GetValue(LegendItemTemplateProperty);
        set => SetValue(LegendItemTemplateProperty, value);
    }

    public static readonly DependencyProperty LegendItemTemplateProperty =
        DependencyProperty.Register(
            nameof(LegendItemTemplate),
            typeof(DataTemplate),
            typeof(McTimeline),
            new PropertyMetadata(null));

    #endregion
}
