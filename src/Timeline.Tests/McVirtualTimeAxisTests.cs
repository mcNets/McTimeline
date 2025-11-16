using FluentAssertions;
using McTimeline;

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
}
