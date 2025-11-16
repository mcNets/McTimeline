using FluentAssertions;
using McTimeline;

namespace Timeline.Tests;

public class McVirtualVerticalAxisTests {
    [Fact]
    public void OffsetUnits_ShouldBeClamped() {
        var axis = new McVirtualSeriesAxis();
        axis.ContentUnits = 100;
        axis.ViewportPixels = 50;
        axis.SeriesHeight = 10;

        axis.OffsetUnits = 200; // Beyond MaxOffsetUnits

        axis.OffsetUnits.Should().Be(axis.MaxOffsetUnits);
    }

    [Fact]
    public void UnitsToScreen_ShouldConvertCorrectly() {
        var axis = new McVirtualSeriesAxis();
        axis.ContentUnits = 100;
        axis.SeriesHeight = 5;
        axis.OffsetUnits = 10;

        var y = axis.UnitsToScreen(20);

        y.Should().Be(50); // (20 - 10) * 5
    }

    [Fact]
    public void ScreenToUnits_ShouldConvertCorrectly() {
        var axis = new McVirtualSeriesAxis();
        axis.ContentUnits = 100;
        axis.SeriesHeight = 5;
        axis.OffsetUnits = 10;

        var units = axis.ScreenToUnits(50);

        units.Should().Be(20); // 50 / 5 + 10
    }

    [Fact]
    public void ScrollByPixels_ShouldUpdateOffset() {
        var axis = new McVirtualSeriesAxis();
        axis.ContentUnits = 100;
        axis.SeriesHeight = 10;
        axis.OffsetUnits = 0;

        axis.ScrollByPixels(50);

        axis.OffsetUnits.Should().Be(5); // 50 / 10
    }

    [Fact]
    public void Intersects_ShouldReturnTrueForVisibleRange() {
        var axis = new McVirtualSeriesAxis();
        axis.ContentUnits = 100;
        axis.ViewportPixels = 50;
        axis.SeriesHeight = 1;
        axis.OffsetUnits = 0;

        var intersects = axis.Intersects(10, 10); // Units 10-20

        intersects.Should().BeTrue();
    }

    [Fact]
    public void Intersects_ShouldReturnFalseForInvisibleRange() {
        var axis = new McVirtualSeriesAxis();
        axis.ContentUnits = 100;
        axis.ViewportPixels = 50;
        axis.SeriesHeight = 1;
        axis.OffsetUnits = 0;

        var intersects = axis.Intersects(60, 10); // Units 60-70, beyond viewport

        intersects.Should().BeFalse();
    }

    [Fact]
    public void ZoomToFit_ShouldAdjustSeriesHeight() {
        var axis = new McVirtualSeriesAxis();
        axis.ContentUnits = 100;
        axis.ViewportPixels = 50;

        axis.ZoomToFit();

        axis.SeriesHeight.Should().Be(0.5); // 50 / 100
    }
}