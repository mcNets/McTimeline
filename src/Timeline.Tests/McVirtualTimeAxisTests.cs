using FluentAssertions;
using McTimeline;
using McTimeline.Viewport;

namespace Timeline.Tests;

public class McVirtualTimeAxisTests {
    [Fact]
    public void SetRange_ShouldSetMinAndMaxDates() {
        var axis = new McVirtualTimeAxis();
        var minDate = new DateTime(2023, 1, 1);
        var maxDate = new DateTime(2023, 12, 31);

        axis.SetRange(minDate, maxDate);

        axis.MinDate.Should().Be(minDate);
        axis.MaxDate.Should().Be(maxDate);
    }

    [Fact]
    public void DateToHours_ShouldConvertCorrectly() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        var date = new DateTime(2023, 1, 1, 12, 0, 0);

        var hours = axis.DateToHours(date);

        hours.Should().Be(12); // Assuming base is 2023-01-01 00:00
    }

    [Fact]
    public void HoursToDate_ShouldConvertCorrectly() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));

        var date = axis.HoursToDate(24);

        date.Should().Be(new DateTime(2023, 1, 2));
    }

    [Fact]
    public void TimeToScreen_ShouldConvertCorrectly() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        axis.PixelsPerHour = 10;
        axis.OffsetHours = 0;
        var date = new DateTime(2023, 1, 1, 1, 0, 0);

        var x = axis.TimeToScreen(date);

        x.Should().Be(10); // 1 hour * 10 px/hour
    }

    [Fact]
    public void ScreenToHours_ShouldConvertCorrectly() {
        var axis = new McVirtualTimeAxis();
        axis.PixelsPerHour = 10;
        axis.OffsetHours = 0;

        var hours = axis.ScreenToHours(50);

        hours.Should().Be(5); // 50 px / 10 px/hour
    }

    [Fact]
    public void OffsetHours_ShouldBeClamped() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        axis.ViewportPixels = 100;
        axis.PixelsPerHour = 10;

        axis.OffsetHours = 100; // Way beyond MaxOffsetHours

        axis.OffsetHours.Should().Be(axis.MaxOffsetHours);
    }

    [Fact]
    public void Intersects_ShouldReturnTrueForOverlappingRanges() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 3));
        axis.ViewportPixels = 100;
        axis.PixelsPerHour = 10;
        axis.OffsetHours = 0;

        var intersects = axis.Intersects(5, 5); // 5-10 hours, overlaps with 0-10

        intersects.Should().BeTrue();
    }

    [Fact]
    public void Intersects_ShouldReturnFalseForNonOverlappingRanges() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        axis.ViewportPixels = 100;
        axis.PixelsPerHour = 10;
        axis.OffsetHours = 0;

        var intersects = axis.Intersects(50, 5); // 50-55 hours, beyond viewport

        intersects.Should().BeFalse();
    }

    [Fact]
    public void ZoomToFit_ShouldAdjustPixelsPerHour() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        axis.ViewportPixels = 100;

        axis.ZoomToFit();

        axis.PixelsPerHour.Should().Be(100 / 24.0); // 24 hours in viewport
    }

    [Fact]
    public void SetRange_ShouldThrowWhenMaxNotGreaterThanMin() {
        var axis = new McVirtualTimeAxis();
        var date = new DateTime(2023, 1, 1);

        var act = () => axis.SetRange(date, date);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetRange_ShouldUpdateContentHours() {
        var axis = new McVirtualTimeAxis();
        var min = new DateTime(2023, 1, 1);
        var max = new DateTime(2023, 1, 3); // 48 hours

        axis.SetRange(min, max);

        axis.ContentHours.Should().Be(48);
    }

    [Fact]
    public void HoursToScreen_ShouldConvertCorrectly() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        axis.ViewportPixels = 100;
        axis.PixelsPerHour = 10;
        axis.OffsetHours = 2;

        var x = axis.HoursToScreen(5);

        x.Should().Be(30); // (5 - 2) * 10
    }

    [Fact]
    public void ScrollByPixels_ShouldUpdateOffset() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        axis.PixelsPerHour = 10;
        axis.ViewportPixels = 100;

        axis.ScrollByPixels(30);

        axis.OffsetHours.Should().Be(3); // 30 / 10
    }

    [Fact]
    public void ScrollByHours_ShouldUpdateOffset() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        axis.PixelsPerHour = 10;
        axis.ViewportPixels = 100;

        axis.ScrollByHours(5);

        axis.OffsetHours.Should().Be(5);
    }

    [Fact]
    public void ScrollToHours_ShouldSetOffset() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        axis.PixelsPerHour = 10;
        axis.ViewportPixels = 100;

        axis.ScrollToHours(7);

        axis.OffsetHours.Should().Be(7);
    }

    [Fact]
    public void ViewportHours_ShouldReturnPixelsDividedByPixelsPerHour() {
        var axis = new McVirtualTimeAxis();
        axis.ViewportPixels = 200;
        axis.PixelsPerHour = 10;

        axis.ViewportHours.Should().Be(20);
    }

    [Fact]
    public void MaxOffsetHours_ShouldBeZeroWhenContentFitsViewport() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2)); // 24h
        axis.PixelsPerHour = 1;
        axis.ViewportPixels = 100; // viewport = 100h > 24h content

        axis.MaxOffsetHours.Should().Be(0);
    }

    [Fact]
    public void MaxOffsetHours_ShouldBePositiveWhenContentExceedsViewport() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 3)); // 48h
        axis.PixelsPerHour = 10;
        axis.ViewportPixels = 100; // viewport = 10h

        axis.MaxOffsetHours.Should().Be(38); // 48 - 10
    }

    [Fact]
    public void VisibleHoursRange_ShouldReturnCurrentViewRange() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        axis.PixelsPerHour = 10;
        axis.ViewportPixels = 100; // 10h visible
        axis.OffsetHours = 5;

        var (left, right) = axis.VisibleHoursRange;

        left.Should().Be(5);
        right.Should().Be(15);
    }

    [Fact]
    public void VisibleDateRange_ShouldReturnDatesForCurrentView() {
        var axis = new McVirtualTimeAxis();
        var min = new DateTime(2023, 1, 1);
        axis.SetRange(min, new DateTime(2023, 1, 10));
        axis.PixelsPerHour = 10;
        axis.ViewportPixels = 240; // 24h visible
        axis.OffsetHours = 0;

        var (left, right) = axis.VisibleDateRange;

        left.Should().Be(min);
        right.Should().Be(new DateTime(2023, 1, 2));
    }

    [Fact]
    public void ScrollNormalized_GetShouldReturnNormalizedPosition() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 3)); // 48h
        axis.PixelsPerHour = 10;
        axis.ViewportPixels = 100; // 10h → MaxOffsetHours = 38

        axis.OffsetHours = 19; // half of 38

        axis.ScrollNormalized.Should().BeApproximately(0.5, 0.001);
    }

    [Fact]
    public void ScrollNormalized_SetShouldAdjustOffset() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 3)); // 48h
        axis.PixelsPerHour = 10;
        axis.ViewportPixels = 100; // MaxOffsetHours = 38

        axis.ScrollNormalized = 1.0;

        axis.OffsetHours.Should().BeApproximately(38, 0.001);
    }

    [Fact]
    public void ClampDate_ShouldReturnMinDateWhenBefore() {
        var axis = new McVirtualTimeAxis();
        var min = new DateTime(2023, 1, 1);
        axis.SetRange(min, new DateTime(2023, 1, 10));

        axis.ClampDate(new DateTime(2022, 6, 1)).Should().Be(min);
    }

    [Fact]
    public void ClampDate_ShouldReturnMaxDateWhenAfter() {
        var axis = new McVirtualTimeAxis();
        var max = new DateTime(2023, 1, 10);
        axis.SetRange(new DateTime(2023, 1, 1), max);

        axis.ClampDate(new DateTime(2024, 1, 1)).Should().Be(max);
    }

    [Fact]
    public void ClampDate_ShouldReturnDateWhenInRange() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        var date = new DateTime(2023, 1, 5);

        axis.ClampDate(date).Should().Be(date);
    }

    [Fact]
    public void DurationToPixels_ShouldConvertCorrectly() {
        var axis = new McVirtualTimeAxis();
        axis.PixelsPerHour = 10;

        var px = axis.DurationToPixels(TimeSpan.FromHours(3));

        px.Should().Be(30);
    }

    [Fact]
    public void IntersectsDateRange_ShouldReturnTrueWhenOverlapsGlobalAndVisibleRange() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        axis.PixelsPerHour = 1;
        axis.ViewportPixels = 10000; // large, shows all content
        axis.OffsetHours = 0;

        var result = axis.Intersects(new DateTime(2023, 1, 3), new DateTime(2023, 1, 5), 0);

        result.Should().BeTrue();
    }

    [Fact]
    public void IntersectsDateRange_ShouldReturnFalseWhenBeforeGlobalRange() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        axis.PixelsPerHour = 1;
        axis.ViewportPixels = 10000;

        var result = axis.Intersects(new DateTime(2022, 12, 1), new DateTime(2022, 12, 31), 0);

        result.Should().BeFalse();
    }

    [Fact]
    public void IntersectsDateRange_ShouldReturnFalseWhenAfterGlobalRange() {
        var axis = new McVirtualTimeAxis();
        axis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        axis.PixelsPerHour = 1;
        axis.ViewportPixels = 10000;

        var result = axis.Intersects(new DateTime(2023, 2, 1), new DateTime(2023, 2, 28), 0);

        result.Should().BeFalse();
    }

    [Fact]
    public void PixelsPerHour_ShouldBeClampedToPositiveMinimum() {
        var axis = new McVirtualTimeAxis();

        axis.PixelsPerHour = -5;

        axis.PixelsPerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ViewportPixels_ShouldBeClampedToZero() {
        var axis = new McVirtualTimeAxis();

        axis.ViewportPixels = -100;

        axis.ViewportPixels.Should().Be(0);
    }
}
