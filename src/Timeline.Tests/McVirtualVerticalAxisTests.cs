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

    [Fact]
    public void SetRange_ShouldThrowWhenMaxNotGreaterThanMin() {
        var axis = new McVirtualSeriesAxis();

        var act = () => axis.SetRange(5, 5);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetRange_ShouldSetMinUnitsAndContentUnits() {
        var axis = new McVirtualSeriesAxis();

        axis.SetRange(2, 12);

        axis.MinUnits.Should().Be(2);
        axis.ContentUnits.Should().Be(10);
    }

    [Fact]
    public void MaxOffsetSteps_ShouldCeilFractionalMaxOffsetUnits() {
        // ViewportUnits = 45/10 = 4.5 → MaxOffsetUnits = 10 - 4.5 = 5.5
        // MaxOffsetSteps = ceil(5.5 - epsilon) = 6
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 10,
            SeriesHeight = 10,
            ViewportPixels = 45
        };

        axis.MaxOffsetSteps.Should().Be(6);
    }

    [Fact]
    public void MaxOffsetSteps_ShouldBeZeroWhenContentFitsViewport() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 5,
            SeriesHeight = 10,
            ViewportPixels = 100 // ViewportUnits = 10 > ContentUnits = 5
        };

        axis.MaxOffsetSteps.Should().Be(0);
    }

    [Fact]
    public void UnitsToPixels_ShouldConvertCorrectly() {
        var axis = new McVirtualSeriesAxis { SeriesHeight = 15 };

        var px = axis.UnitsToPixels(4);

        px.Should().Be(60); // 4 * 15
    }

    [Fact]
    public void ViewportUnits_ShouldReturnPixelsDividedBySeriesHeight() {
        var axis = new McVirtualSeriesAxis {
            ViewportPixels = 150,
            SeriesHeight = 30
        };

        axis.ViewportUnits.Should().Be(5);
    }

    [Fact]
    public void VisibleUnitsRange_ShouldReturnCurrentViewRange() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 20,
            SeriesHeight = 10,
            ViewportPixels = 50,   // ViewportUnits = 5
            OffsetUnits = 3
        };

        var (top, bottom) = axis.VisibleUnitsRange;

        top.Should().Be(3);
        bottom.Should().Be(8); // 3 + 5
    }

    [Fact]
    public void ScrollNormalized_GetShouldReturnNormalizedPosition() {
        // ContentUnits=10, SeriesHeight=10, ViewportPixels=50 → ViewportUnits=5, MaxOffsetUnits=5, MaxOffsetSteps=5
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 10,
            SeriesHeight = 10,
            ViewportPixels = 50,
            OffsetUnits = 5   // at max
        };

        axis.ScrollNormalized.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void ScrollNormalized_SetShouldUpdateOffset() {
        var axis = new McVirtualSeriesAxis {
            ContentUnits = 10,
            SeriesHeight = 10,
            ViewportPixels = 50  // MaxOffsetSteps = 5
        };

        axis.ScrollNormalized = 0.0;

        axis.OffsetUnits.Should().Be(0);
    }

    [Fact]
    public void SeriesHeight_ShouldBeClampedToPositiveMinimum() {
        var axis = new McVirtualSeriesAxis();

        axis.SeriesHeight = -5;

        axis.SeriesHeight.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ContentUnits_ShouldBeClampedToZero() {
        var axis = new McVirtualSeriesAxis();

        axis.ContentUnits = -10;

        axis.ContentUnits.Should().Be(0);
    }
}