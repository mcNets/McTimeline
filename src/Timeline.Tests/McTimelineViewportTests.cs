using FluentAssertions;
using McTimeline;

namespace Timeline.Tests;

public class McTimelineViewportTests {
    [Fact]
    public void OnSizeChanged_ShouldUpdateAxes() {
        var viewport = new McTimelineViewport();
        var size = new Windows.Foundation.Size(200, 100);

        viewport.OnSizeChanged(size);

        viewport.TimeAxis.ViewportPixels.Should().Be(200);
        viewport.VerticalAxis.ViewportPixels.Should().Be(100);
    }

    [Fact]
    public void OnScrollChanged_ShouldUpdateOffsets() {
        var viewport = new McTimelineViewport();
        viewport.TimeAxis.ContentHours = 100;
        viewport.VerticalAxis.ContentUnits = 100;

        viewport.OnScrollChanged(10, 20);

        viewport.TimeAxis.OffsetHours.Should().Be(1); // 10 / 10
        viewport.VerticalAxis.OffsetUnits.Should().Be(2); // 20 / 10
    }

    [Fact]
    public void SetTimeRange_ShouldUpdateTimeAxis() {
        var viewport = new McTimelineViewport();
        var minDate = new DateTime(2023, 1, 1);
        var maxDate = new DateTime(2023, 1, 2);

        viewport.SetTimeRange(minDate, maxDate);

        viewport.TimeAxis.MinDate.Should().Be(minDate);
        viewport.TimeAxis.MaxDate.Should().Be(maxDate);
    }

    [Fact]
    public void SetVerticalRange_ShouldUpdateVerticalAxis() {
        var viewport = new McTimelineViewport();

        viewport.SetVerticalRange(0, 50);

        viewport.VerticalAxis.ContentUnits.Should().Be(50);
    }

    [Fact]
    public void IsSeriesVisible_ShouldReturnTrueForVisibleSeries() {
        var viewport = new McTimelineViewport();
        viewport.SeriesHeight = 20;
        viewport.VerticalAxis.ContentUnits = 100;
        viewport.VerticalAxis.ViewportPixels = 100;
        viewport.VerticalAxis.PixelsPerUnit = 1;
        viewport.VerticalAxis.OffsetUnits = 0;

        var isVisible = viewport.IsSeriesVisible(2); // y = 2 * 20 = 40

        isVisible.Should().BeTrue();
    }

    [Fact]
    public void IsSeriesVisible_ShouldReturnFalseForInvisibleSeries() {
        var viewport = new McTimelineViewport();
        viewport.SeriesHeight = 20;
        viewport.VerticalAxis.ContentUnits = 100;
        viewport.VerticalAxis.ViewportPixels = 50;
        viewport.VerticalAxis.PixelsPerUnit = 1;
        viewport.VerticalAxis.OffsetUnits = 0;

        var isVisible = viewport.IsSeriesVisible(5); // y = 5 * 20 = 100, beyond viewport

        isVisible.Should().BeFalse();
    }

    [Fact]
    public void GetDayTicks_ShouldReturnTicks() {
        var viewport = new McTimelineViewport();
        viewport.TimeAxis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 3));
        viewport.TimeAxis.ViewportPixels = 100;
        viewport.TimeAxis.PixelsPerHour = 10;
        viewport.TimeAxis.OffsetHours = 0;

        var ticks = viewport.GetDayTicks().ToList();

        ticks.Should().NotBeEmpty();
        ticks.First().Label.Should().Be("01/01");
    }

    [Fact]
    public void GetHourTicks_ShouldReturnTicks() {
        var viewport = new McTimelineViewport();
        viewport.TimeAxis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        viewport.TimeAxis.ViewportPixels = 100;
        viewport.TimeAxis.PixelsPerHour = 10;
        viewport.TimeAxis.OffsetHours = 0;

        var ticks = viewport.GetHourTicks().ToList();

        ticks.Should().NotBeEmpty();
        ticks.First().Label.Should().Be("0:00");
    }

    [Fact]
    public void GetItemPosition_ShouldCalculateCorrectly() {
        var viewport = new McTimelineViewport();
        viewport.SeriesHeight = 30;
        viewport.TimeAxis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        viewport.TimeAxis.PixelsPerHour = 10;
        viewport.TimeAxis.OffsetHours = 0;
        var item = new McTimelineItem("1", "Test", "", new DateTime(2023, 1, 1, 1, 0, 0), new DateTime(2023, 1, 1, 2, 0, 0));

        var position = viewport.GetItemPosition(item, 1);

        position.X.Should().Be(10); // 1 hour * 10 px
        position.Y.Should().Be(30); // seriesIndex 1 * 30
        position.Width.Should().Be(10); // 1 hour * 10 px
    }
}