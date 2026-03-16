namespace McTimeline;

/// <summary>
/// Default and minimum constant values used in the <see cref="McTimeline"/> project.
/// </summary>
internal static class McConstants {
    /// <summary>
    /// Default height in pixels for each series row.
    /// </summary>
    public const double SERIES_HEIGHT = 40.0;

    /// <summary>
    /// Minimum allowed series height to avoid division by zero.
    /// </summary>
    public const double MIN_SERIES_HEIGHT = 1e-6;

    /// <summary>
    /// Epsilon used when computing <c>MaxOffsetSteps</c> to avoid floating-point edge cases.
    /// </summary>
    public const double MAX_OFFSET_EPSILON = 1e-9;

    /// <summary>
    /// Minimum allowed zoom level in pixels per hour.
    /// </summary>
    public const double MIN_PIXELS_PER_HOUR = 10.0;

    /// <summary>
    /// Maximum allowed zoom level in pixels per hour.
    /// </summary>
    public const double MAX_PIXELS_PER_HOUR = 300.0;

    /// <summary>
    /// Zoom factor applied when zooming in (scroll wheel up with Ctrl).
    /// </summary>
    public const double ZOOM_IN_FACTOR = 1.2;

    /// <summary>
    /// Zoom factor applied when zooming out (scroll wheel down with Ctrl).
    /// </summary>
    public const double ZOOM_OUT_FACTOR = 0.8;

    /// <summary>
    /// Number of pixels to scroll per wheel tick (vertical scroll without modifier keys).
    /// </summary>
    public const double SCROLL_DELTA_PIXELS = 50.0;

    /// <summary>
    /// Divisor used to compute the horizontal scrollbar small-change step from the viewport hours.
    /// </summary>
    public const double HSCROLL_SMALL_CHANGE_DIVISOR = 10.0;

    /// <summary>
    /// Top padding in pixels for day labels in the time scale.
    /// </summary>
    public const double DAY_LABEL_TOP_PADDING = 3.0;

    /// <summary>
    /// Fraction of the time-scale canvas height used for hour tick marks.
    /// </summary>
    public const double TICK_HEIGHT_RATIO = 0.3;

    /// <summary>
    /// Initial number of elements to pre-create in each element pool.
    /// </summary>
    public const int INITIAL_POOL_SIZE = 50;
}
