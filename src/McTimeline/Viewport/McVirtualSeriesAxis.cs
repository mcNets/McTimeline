namespace McTimeline.Viewport;

/// <summary>
/// 1D vertical axis for managing vertical positioning and visibility.
/// World units are series indices.
/// Scale = pixels per series.
/// Combines the functionality of virtual space management and vertical axis in a single class.
/// </summary>
public sealed class McVirtualSeriesAxis {
    #region Private fields
    
    private double _offsetUnits;          // world units (series indices, above the viewport)
    private double _seriesHeight = 30.0;  // screen px per series (>0)
    private double _viewportPx;           // height of the viewport in pixels
    private double _contentUnits;         // total number of series

    #endregion

    /// <summary>
    /// Gets or sets the minimum unit value for the axis range.
    /// (Minimum number of series)
    /// </summary>
    public double MinUnits { get; set; } = 0;

    /// <summary>
    /// Gets or sets the offset value, in units (series indices), used for vertical positioning calculations.
    /// This represents how many series are scrolled above the visible viewport.
    /// </summary>
    /// <remarks>
    /// The offset determines the starting series index visible at the top of the viewport.
    /// For example, an offset of 5 means series 5, 6, 7, etc. are visible.
    /// The value is automatically clamped to the valid range [MinUnits, MinUnits + MaxOffsetSteps]
    /// and floored to ensure only whole series are displayed, preventing partial series visibility.
    /// Setting this property triggers a re-clamping of the offset if the viewport or content changes.
    /// </remarks>
    public double OffsetUnits {
        get => _offsetUnits;
        set => _offsetUnits = Math.Floor(Math.Clamp(value, MinUnits, MinUnits + MaxOffsetSteps));
    }

    /// <summary>
    /// Gets or sets the number of pixels that represent one series on the vertical scale.
    /// This defines the height of each series row in the timeline.
    /// </summary>
    /// <remarks>
    /// The series height determines how tall each series appears on screen.
    /// For example, a SeriesHeight of 30 means each series occupies 30 pixels vertically.
    /// Setting this property to a value less than or equal to zero will automatically adjust it to a
    /// minimal positive value (1e-6) to avoid division by zero.
    /// Changing the series height may affect the number of visible series (ViewportUnits) and
    /// will trigger a re-clamping of the offset to ensure valid scrolling bounds.
    /// This property is typically bound to the SeriesHeight dependency property of the McTimeline control.
    /// </remarks>
    public double SeriesHeight {
        get => _seriesHeight;
        set {
            _seriesHeight = Math.Max(1e-6, value); // avoids 0 or negatives
            ClampOffsetIntoRange();
        }
    }

    /// <summary>
    /// Gets or sets the height of the viewport (screen) in pixels.
    /// </summary>
    /// <remarks>Setting this property to a negative value will automatically clamp it to zero. The value
    /// represents the vertical size of the visible area in pixels and may affect layout or rendering
    /// calculations.</remarks>
    public double ViewportPixels {
        get => _viewportPx;
        set {
            _viewportPx = Math.Max(0, value);
            ClampOffsetIntoRange();
        }
    }

    /// <summary>
    /// Gets or sets the total number of content units. (Number of series)
    /// The value is constrained to be non-negative.
    /// </summary>
    public double ContentUnits {
        get => _contentUnits;
        set {
            _contentUnits = Math.Max(0, value);
            ClampOffsetIntoRange();
        }
    }

    /// <summary>
    /// Gets the total number of units currently visible in the viewport.
    /// </summary>
    public double ViewportUnits => _viewportPx / _seriesHeight;

    /// <summary>
    /// Gets the maximum number of units by which the content can be offset within the viewport.
    /// </summary>
    /// <remarks>The value represents the largest allowable vertical scroll offset, in units, based on the
    /// difference between the total content units and the viewport units. If the content fits entirely within the
    /// viewport, the value is zero.</remarks>
    public double MaxOffsetUnits => Math.Max(0, _contentUnits - ViewportUnits);

    /// <summary>
    /// Gets the maximum offset in whole-series steps, ensuring every series can be scrolled to the top.
    /// </summary>
    public double MaxOffsetSteps {
        get {
            var raw = MaxOffsetUnits;
            if (raw <= 0) {
                return 0;
            }
            const double epsilon = 1e-9;
            return Math.Ceiling(raw - epsilon);
        }
    }

    /// <summary>
    /// Converts a unit value to its corresponding position in screen pixels.
    /// </summary>
    /// <param name="units">The unit value to convert to screen coordinates.</param>
    /// <returns>A double value representing the pixel position on the screen that corresponds to the specified number of units.</returns>
    public double UnitsToScreen(double units) => (units - _offsetUnits) * _seriesHeight;

    /// <summary>
    /// Converts a vertical screen coordinate to its corresponding unit value.
    /// </summary>
    /// <param name="screenY">The vertical position, in pixels, to convert to units.</param>
    /// <returns>A double value representing the unit corresponding to the specified screen coordinate.</returns>
    public double ScreenToUnits(double screenY) => screenY / _seriesHeight + _offsetUnits;

    /// <summary>
    /// Scrolls the view vertically by the specified number of pixels.
    /// </summary>
    /// <param name="deltaPx">The number of pixels to scroll the view. Positive values scroll down; negative values scroll up.</param>
    public void ScrollByPixels(double deltaPx) => OffsetUnits = _offsetUnits + deltaPx / _seriesHeight;

    /// <summary>
    /// Adjusts the current scroll position by the specified number of units.
    /// </summary>
    /// <param name="deltaUnits">The number of units to scroll. Positive values scroll down; negative values scroll up.</param>
    public void ScrollByUnits(double deltaUnits) => OffsetUnits = _offsetUnits + deltaUnits;

    /// <summary>
    /// Scrolls the view to the specified unit position.
    /// </summary>
    /// <param name="unitsTop">The unit value to scroll to. Represents the number of units from the top of the view. Must be a non-negative value.</param>
    public void ScrollToUnits(double unitsTop) => OffsetUnits = unitsTop;

    /// <summary>
    /// Gets the range of units currently visible in the viewport, represented as a tuple of top and bottom boundaries.
    /// </summary>
    public (double Top, double Bottom) VisibleUnitsRange => (_offsetUnits, _offsetUnits + ViewportUnits);

    /// <summary>
    /// Determines whether the specified unit range intersects with the visible units range.
    /// </summary>
    /// <param name="units">The starting unit of the range to test for intersection.</param>
    /// <param name="heightUnits">The height of the range, in units.</param>
    /// <param name="bufferUnits">An optional buffer, in units, to expand the visible units range for intersection testing. The default is 0.</param>
    /// <returns>true if the specified range intersects with the visible units range (including any buffer); otherwise, false.</returns>
    public bool Intersects(double units, double heightUnits, double bufferUnits = 0) {
        var (T, B) = VisibleUnitsRange;
        var top = units;
        var bottom = units + heightUnits;
        return (bottom > T - bufferUnits) && (top < B + bufferUnits);
    }

    /// <summary>
    /// Gets or sets the normalized scroll position as a value between 0 and 1.
    /// </summary>
    /// <remarks>Setting this property adjusts the scroll offset proportionally based on the maximum offset
    /// range. The value is clamped to the range [0, 1] when read.</remarks>
    public double ScrollNormalized {
        get {
            var denom = Math.Max(MaxOffsetSteps, 1e-6);
            return Math.Clamp(_offsetUnits / denom, 0, 1);
        }
        set => OffsetUnits = value * MaxOffsetSteps;
    }

    /// <summary>
    /// Sets the minimum and maximum unit range for the content.
    /// </summary>
    /// <param name="min">The minimum unit of the range.</param>
    /// <param name="max">The maximum unit of the range. Must be greater than <paramref name="min"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="max"/> is less than or equal to <paramref name="min"/>.</exception>
    public void SetRange(double min, double max) {
        if (max <= min) {
            throw new ArgumentException("Max must be > Min");
        }
        MinUnits = min;
        ContentUnits = max - min;
        // re-clamp in case the offset is out of bounds
        ClampOffsetIntoRange();
    }

    /// <summary>
    /// Converts a height in units to pixels.
    /// </summary>
    /// <param name="units">The height in units.</param>
    /// <returns>The height in pixels.</returns>
    public double UnitsToPixels(double units) => units * _seriesHeight;

    /// <summary>
    /// Adjusts the pixels per unit to fit the entire content within the viewport.
    /// </summary>
    public void ZoomToFit() {
        if (ContentUnits > 0 && ViewportPixels > 0) {
            SeriesHeight = ViewportPixels / ContentUnits;
        }
    }

    private void ClampOffsetIntoRange() {
        _offsetUnits = Math.Floor(Math.Clamp(_offsetUnits, MinUnits, MinUnits + MaxOffsetSteps));
    }
}