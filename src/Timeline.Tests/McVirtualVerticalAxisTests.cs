using FluentAssertions;
using McTimeline;
using McTimeline.Viewport;

namespace Timeline.Tests;

public class McVirtualSeriesAxisTests {
    [Fact]
    public void OffsetUnits_ShouldBeClampedAndFloored() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 100,
            ViewportPixels = 50,
            SeriesHeight = 10,

            OffsetUnits = 200 // Beyond MaxOffsetUnits
        };

        // MaxOffsetUnits = 100 - (50/10) = 95
        // OffsetUnits should be clamped to 95 and floored to 95
        axis.OffsetUnits.Should().Be(Math.Floor(axis.MaxOffsetUnits));
    }

    [Fact]
    public void ScrollToUnits_ShouldSetOffsetCorrectly() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 100,
            ViewportPixels = 50,
            SeriesHeight = 10,
        };

        axis.ScrollToUnits(2);
        axis.OffsetUnits.Should().Be(2);
    }

    [Fact]
    public void UnitsToScreen_ShouldConvertCorrectly() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 100,
            SeriesHeight = 5,
            OffsetUnits = 10
        };

        var y = axis.UnitsToScreen(20);

        y.Should().Be(50); // (20 - 10) * 5
    }

    [Fact]
    public void ScreenToUnits_ShouldConvertCorrectly() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 100,
            SeriesHeight = 5,
            OffsetUnits = 10
        };

        var units = axis.ScreenToUnits(50);

        units.Should().Be(20); // 50 / 5 + 10
    }

    [Fact]
    public void ScrollByPixels_ShouldUpdateOffsetAndFloor() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 100,
            SeriesHeight = 10,
            OffsetUnits = 0
        };

        axis.ScrollByPixels(55); // 55 / 10 = 5.5 units

        axis.OffsetUnits.Should().Be(5); // Floored to 5
    }

    [Fact]
    public void ScrollByUnits_ShouldUpdateOffsetAndFloor() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 100,
            SeriesHeight = 10,
            OffsetUnits = 0
        };

        axis.ScrollByUnits(5.7);

        axis.OffsetUnits.Should().Be(5); // Floored to 5
    }

    [Fact]
    public void Intersects_ShouldReturnTrueForVisibleRange() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 100,
            ViewportPixels = 50,
            SeriesHeight = 1,
            OffsetUnits = 0
        };

        var intersects = axis.Intersects(10, 10); // Units 10-20

        intersects.Should().BeTrue();
    }

    [Fact]
    public void Intersects_ShouldReturnFalseForInvisibleRange() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 100,
            ViewportPixels = 50,
            SeriesHeight = 1,
            OffsetUnits = 0
        };

        var intersects = axis.Intersects(60, 10); // Units 60-70, beyond viewport

        intersects.Should().BeFalse();
    }

    [Fact]
    public void ZoomToFit_ShouldAdjustSeriesHeight() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 100,
            ViewportPixels = 50
        };

        axis.ZoomToFit();

        axis.SeriesHeight.Should().Be(0.5); // 50 / 100
    }
}