namespace McTimeline;

/// <summary>
/// 1D axis based on time (DateTime). 
/// World units are hours (double).
/// Scale = pixels per hour.
/// Combines the functionality of virtual space management and time axis in a single class.
/// </summary>
public sealed class McVirtualTimeAxis {
    private double _offsetHours;          // world units (hours, left of the viewport)
    private double _pixelsPerHour = 10.0; // screen px per world unit (>0)
    private double _viewportPx;           // width of the viewport in pixels
    private double _contentHours;         // total length of the content in hours

    /// <summary>
    /// Gets the earliest date allowed for the operation or entity.
    /// </summary>
    public DateTime MinDate { get; private set; }

    /// <summary>
    /// Gets the maximum allowable date value for the associated operation or range.
    /// </summary>
    public DateTime MaxDate { get; private set; }

    /// <summary>
    /// Gets or sets the offset value, in hours, used for time calculations.
    /// </summary>
    /// <remarks>The value is constrained to be between 0 and the maximum allowed offset hours. Setting a
    /// value outside this range will automatically clamp it to the nearest valid value.</remarks>
    public double OffsetHours {
        get => _offsetHours;
        set => _offsetHours = Math.Clamp(value, 0, MaxOffsetHours);
    }

    /// <summary>
    /// Gets or sets the number of pixels that represent one hour on the timeline scale.
    /// </summary>
    /// <remarks>Setting this property to a value less than or equal to zero will automatically adjust it to a
    /// minimal positive value. Changing the scale may affect related properties such as offset and maximum offset
    /// hours.</remarks>
    public double PixelsPerHour {
        get => _pixelsPerHour;
        set {
            _pixelsPerHour = Math.Max(1e-6, value); // avoids 0 or negatives
            _offsetHours = Math.Clamp(_offsetHours, 0, MaxOffsetHours);
        }
    }

    /// <summary>
    /// Gets or sets the width of the viewport (screen) in pixels.
    /// </summary>
    /// <remarks>Setting this property to a negative value will automatically clamp it to zero. The value
    /// represents the horizontal size of the visible area in pixels and may affect layout or rendering
    /// calculations.</remarks>
    public double ViewportPixels {
        get => _viewportPx;
        set {
            _viewportPx = Math.Max(0, value);
            _offsetHours = Math.Clamp(_offsetHours, 0, MaxOffsetHours);
        }
    }

    /// <summary>
    /// Gets or sets the total number of content hours. The value is constrained to be non-negative.
    /// </summary>
    public double ContentHours {
        get => _contentHours;
        set {
            _contentHours = Math.Max(0, value);
            _offsetHours = Math.Clamp(_offsetHours, 0, MaxOffsetHours);
        }
    }

    /// <summary>
    /// Gets the total number of hours currently visible in the viewport.
    /// </summary>
    public double ViewportHours => _viewportPx / _pixelsPerHour;

    /// <summary>
    /// Gets the maximum number of hours by which the content can be offset within the viewport.
    /// </summary>
    /// <remarks>The value represents the largest allowable horizontal scroll offset, in hours, based on the
    /// difference between the total content hours and the viewport hours. If the content fits entirely within the
    /// viewport, the value is zero.</remarks>
    public double MaxOffsetHours => Math.Max(0, _contentHours - ViewportHours);

    /// <summary>
    /// Converts a time value, in hours, to its corresponding position in screen pixels.
    /// </summary>
    /// <param name="hours">The time value, in hours, to convert to screen coordinates. Represents the number of hours to be mapped.</param>
    /// <remarks>The conversion is based on the current offset and scaling factor. This method is useful for
    /// mapping time-based data to a visual timeline or calendar view.</remarks>
    /// <returns>A double value representing the pixel position on the screen that corresponds to the specified number of hours.</returns>
    public double HoursToScreen(double hours) => (hours - _offsetHours) * _pixelsPerHour;

    /// <summary>
    /// Converts a horizontal screen coordinate to its corresponding hour value on the timeline.
    /// </summary>
    /// <param name="screenX">The horizontal position, in pixels, to convert to hours. Typically represents a point on the timeline's visual
    /// axis.</param>
    /// <returns>A double value representing the hour corresponding to the specified screen coordinate.</returns>
    public double ScreenToHours(double screenX) => screenX / _pixelsPerHour + _offsetHours;

    /// <summary>
    /// Scrolls the view horizontally by the specified number of pixels.
    /// </summary>
    /// <param name="deltaPx">The number of pixels to scroll the view. Positive values scroll to the right; negative values scroll to the
    /// left.</param>
    public void ScrollByPixels(double deltaPx) => OffsetHours = _offsetHours + deltaPx / _pixelsPerHour;

    /// <summary>
    /// Adjusts the current scroll position by the specified number of hours.
    /// </summary>
    /// <param name="deltaHours">The number of hours to scroll. Positive values scroll forward; negative values scroll backward.</param>
    public void ScrollByHours(double deltaHours) => OffsetHours = _offsetHours + deltaHours;

    /// <summary>
    /// Scrolls the view to the specified hour position.
    /// </summary>
    /// <param name="hoursLeft">The hour value to scroll to. Represents the number of hours from the start of the view. Must be a non-negative
    /// value.</param>
    public void ScrollToHours(double hoursLeft) => OffsetHours = hoursLeft;

    /// <summary>
    /// Gets the range of hours currently visible in the viewport, represented as a tuple of left and right boundaries.
    /// </summary>
    /// <remarks>The returned range reflects the current horizontal scroll and zoom state. The left boundary
    /// indicates the starting hour, and the right boundary indicates the ending hour of the visible region.</remarks>
    public (double Left, double Right) VisibleHoursRange => (_offsetHours, _offsetHours + ViewportHours);

    /// <summary>
    /// Gets the range of dates currently visible in the viewport, represented as a tuple of left and right boundaries.
    /// </summary>
    /// <remarks>The returned range reflects the current horizontal scroll and zoom state. The left boundary
    /// indicates the starting date, and the right boundary indicates the ending date of the visible region.</remarks>
    public (DateTime Left, DateTime Right) VisibleDateRange => (HoursToDate(VisibleHoursRange.Left), HoursToDate(VisibleHoursRange.Right));

    /// <summary>
    /// Determines whether the specified time range, optionally expanded by a buffer, intersects with the visible hours
    /// range.
    /// </summary>
    /// <param name="hours">The starting hour of the time range to test for intersection, expressed in hours.</param>
    /// <param name="widthHours">The width of the time range, in hours, extending from the starting hour.</param>
    /// <param name="bufferHours">An optional buffer, in hours, to expand the visible hours range for intersection testing. The default is 0.</param>
    /// <returns>true if the specified time range intersects with the visible hours range (including any buffer); otherwise,
    /// false.</returns>
    public bool Intersects(double hours, double widthHours, double bufferHours = 0) {
        var (L, R) = VisibleHoursRange;
        var left = hours;
        var right = hours + widthHours;
        return (right > L - bufferHours) && (left < R + bufferHours);
    }

    /// <summary>
    /// Gets or sets the normalized scroll position as a value between 0 and 1.
    /// </summary>
    /// <remarks>Setting this property adjusts the scroll offset proportionally based on the maximum offset
    /// range. The value is clamped to the range [0, 1] when read. This property is useful for scenarios where scroll
    /// position needs to be represented or controlled in a normalized form, such as for UI sliders or progress
    /// indicators.</remarks>
    public double ScrollNormalized {
        get {
            var denom = Math.Max(MaxOffsetHours, 1e-6);
            return Math.Clamp(_offsetHours / denom, 0, 1);
        }
        set => OffsetHours = value * MaxOffsetHours;
    }

    /// <summary>
    /// Sets the minimum and maximum date range for the content, updating related properties accordingly.
    /// </summary>
    /// <remarks>After setting the range, related properties such as content duration and offset are updated
    /// to reflect the new boundaries.</remarks>
    /// <param name="min">The minimum date of the range. Represents the earliest allowed date for the content.</param>
    /// <param name="max">The maximum date of the range. Must be later than <paramref name="min"/>. Represents the latest allowed date for
    /// the content.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="max"/> is less than or equal to <paramref name="min"/>.</exception>
    public void SetRange(DateTime min, DateTime max) {
        if (max <= min) {
            throw new ArgumentException("MaxDate must be > MinDate");
        }
        MinDate = min;
        MaxDate = max;
        ContentHours = (max - min).TotalHours;
        // re-clamp in case the offset is out of bounds
        OffsetHours = OffsetHours;
    }

    /// <summary>
    /// Returns a date that is constrained to fall within the range defined by MinDate and MaxDate.
    /// </summary>
    /// <param name="dt">The date to be clamped to the valid range. If the value is earlier than MinDate, MinDate is returned; if later
    /// than MaxDate, MaxDate is returned.</param>
    /// <returns>A DateTime value within the inclusive range of MinDate and MaxDate. If dt is within the range, dt is returned
    /// unchanged.</returns>
    public DateTime ClampDate(DateTime dt)
        => dt < MinDate ? MinDate : (dt > MaxDate ? MaxDate : dt);

    /// <summary>
    /// Calculates the total number of hours between the specified date and the minimum allowed date.
    /// </summary>
    /// <param name="dt">The date and time to convert to hours relative to the minimum date. If the value is earlier than the minimum
    /// date, it will be clamped to the minimum.</param>
    /// <returns>The number of hours between the specified date and the minimum allowed date. Returns 0 if the date is equal to
    /// or earlier than the minimum date.</returns>
    public double DateToHours(DateTime dt)
        => (ClampDate(dt) - MinDate).TotalHours;

    /// <summary>
    /// Converts the specified number of hours to a <see cref="DateTime"/> value by adding the hours to the minimum
    /// date.
    /// </summary>
    /// <param name="hours">The number of hours to add to the minimum date. Can be negative, zero, or positive.</param>
    /// <returns>A <see cref="DateTime"/> that represents the minimum date plus the specified number of hours.</returns>
    public DateTime HoursToDate(double hours)
        => MinDate.AddHours(hours);

    /// <summary>
    /// Converts a time duration to its equivalent length in pixels based on the current pixels-per-hour setting.
    /// </summary>
    /// <remarks>This method uses the value of PixelsPerHour to determine the pixel length for the given
    /// duration. The result may be fractional if the duration does not align with whole hours.</remarks>
    /// <param name="span">The time interval to convert to pixels. Represents the duration to be mapped.</param>
    /// <returns>A double value representing the number of pixels corresponding to the specified duration. The value is
    /// proportional to the total hours in the duration.</returns>
    public double DurationToPixels(TimeSpan span)
        => span.TotalHours * PixelsPerHour;

    /// <summary>
    /// Gets the range of world hours currently visible in the view.
    /// </summary>
    public (double LeftHours, double RightHours) VisibleWorldRange => VisibleHoursRange;

    /// <summary>
    /// Determines whether the specified time range, optionally expanded by a buffer in hours, intersects with the
    /// global date range represented by this instance.
    /// </summary>
    /// <remarks>If end is earlier than start, the values are automatically swapped. The time range is clamped
    /// to the global date range before intersection is tested. This method is useful for determining whether a time
    /// window, with optional tolerance, falls within or overlaps the global range.</remarks>
    /// <param name="start">The start of the time range to test for intersection.</param>
    /// <param name="end">The end of the time range to test for intersection.</param>
    /// <param name="bufferHours">The number of hours to expand the time range on both sides before testing for intersection. Must be zero or
    /// positive.</param>
    /// <returns>true if the buffered time range overlaps with the global date range; otherwise, false.</returns>
    public bool Intersects(DateTime start, DateTime end, double bufferHours) {
        if (end < start)
            (start, end) = (end, start);

        // if it does not overlap with the global range, outside
        if (end <= MinDate || start >= MaxDate)
            return false;

        var s = ClampDate(start);
        var e = ClampDate(end);
        var hours = (s - MinDate).TotalHours;
        var width = Math.Max(0.0, (e - s).TotalHours);

        return Intersects(hours, width, bufferHours);
    }

    /// <summary>
    /// Converts the specified date and time to its corresponding position on the screen in pixels.
    /// </summary>
    /// <param name="dt">The date and time value to convert to a screen position.</param>
    /// <returns>A double representing the pixel position on the screen that corresponds to the specified date and time.</returns>
    public double TimeToScreen(DateTime dt) {
        // Convert the date to hours and then to viewport pixels
        return HoursToScreen(DateToHours(dt));
    }

    /// <summary>
    /// Adjusts the pixels per hour to fit the entire content within the viewport.
    /// </summary>
    /// <remarks>This method sets PixelsPerHour so that the total content hours fit exactly within the viewport pixels.
    /// If ContentHours is zero or ViewportPixels is zero, no change is made.</remarks>
    public void ZoomToFit() {
        if (ContentHours > 0 && ViewportPixels > 0) {
            PixelsPerHour = ViewportPixels / ContentHours;
        }
    }
}
