namespace McTimeline;

public sealed partial class McTimeline {
    #region Timeline series

    /// <summary>
    /// Gets or sets the collection of timeline series.
    /// </summary>
    public McTimelineSeriesCollection SeriesCollection
    {
        get => (McTimelineSeriesCollection)GetValue(SeriesCollectionProperty);
        set => SetValue(SeriesCollectionProperty, value);
    }

    public static readonly DependencyProperty SeriesCollectionProperty =
        DependencyProperty.Register(
            nameof(SeriesCollection),
            typeof(McTimelineSeriesCollection),
            typeof(McTimeline),
            new PropertyMetadata(null, OnSeriesCollectionChanged));

    private static void OnSeriesCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is McTimeline timeline)
        {
            timeline.OnSeriesCollectionChanged((McTimelineSeriesCollection)e.OldValue, (McTimelineSeriesCollection)e.NewValue);
        }
    }

    #endregion

    #region Layout Dependency Properties

    /// <summary>
    /// Gets or sets a value indicating whether the legend column is visible.
    /// </summary>
    public bool IsLegendVisible {
        get => (bool)GetValue(IsLegendVisibleProperty);
        set => SetValue(IsLegendVisibleProperty, value);
    }

    public static readonly DependencyProperty IsLegendVisibleProperty =
        DependencyProperty.Register(
            nameof(IsLegendVisible),
            typeof(bool),
            typeof(McTimeline),
            new PropertyMetadata(true, OnIsLegendVisibleChanged));

    private static void OnIsLegendVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is McTimeline timeline) {
            timeline.UpdateLegendVisibility();
        }
    }

    public string LegendCaption {
        get { return (string)GetValue(LegendCaptionProperty); }
        set { SetValue(LegendCaptionProperty, value); }
    }

    public static readonly DependencyProperty LegendCaptionProperty =
        DependencyProperty.Register(nameof(LegendCaption),
                                    typeof(string),
                                    typeof(McTimeline),
                                    new PropertyMetadata(string.Empty));

    #endregion

    #region Style Dependency Properties

    /// <summary>
    /// Gets or sets the style for the time scale grid (top section).
    /// </summary>
    public Style? TimeScaleStyle {
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
    public Style? LegendStyle {
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
    /// Gets or sets the style for the legend caption TextBlock.
    /// </summary>
    public Style? LegendCaptionStyle {
        get => (Style?)GetValue(LegendCaptionStyleProperty);
        set => SetValue(LegendCaptionStyleProperty, value);
    }

    public static readonly DependencyProperty LegendCaptionStyleProperty =
        DependencyProperty.Register(
            nameof(LegendCaptionStyle),
            typeof(Style),
            typeof(McTimeline),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the style for each legend item border.
    /// </summary>
    public Style? LegendItemStyle {
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
    public Style? TimelineScrollStyle {
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
    public Style? TimelineCanvasStyle {
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
    public DataTemplate? LegendItemTemplate {
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
