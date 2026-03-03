using FluentAssertions;
using McTimeline;
using McTimeline.Viewport;

namespace Timeline.Tests;

public class McTimelineViewportTests {
    [Fact]
    public void OnSizeChanged_ShouldUpdateAxes() {
        var viewport = new McTimelineViewport();
        var size = new Windows.Foundation.Size(200, 100);

        viewport.OnSizeChanged(size);

        viewport.TimeAxis.ViewportPixels.Should().Be(200);
        // VerticalAxis.ViewportPixels is now set based on legend canvas height
    }

    [Fact]
    public void OnScrollChanged_ShouldUpdateOffsets() {
        var viewport = new McTimelineViewport();
        viewport.SeriesAxis.SeriesHeight = 10;
        viewport.TimeAxis.ContentHours = 100;
        viewport.SeriesAxis.ContentUnits = 100;

        viewport.OnScrollChanged(10, 20);

        viewport.TimeAxis.OffsetHours.Should().Be(1); // 10 / 10
        viewport.SeriesAxis.OffsetUnits.Should().Be(2); // 20 / 10
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

        viewport.SetSeriesRange(0, 50);

        viewport.SeriesAxis.ContentUnits.Should().Be(50);
    }

    [Fact]
    public void IsSeriesVisible_ShouldReturnTrueForVisibleSeries() {
        var viewport = new McTimelineViewport();
        viewport.SeriesAxis.SeriesHeight = 20;
        viewport.SeriesAxis.ContentUnits = 100;
        viewport.SeriesAxis.ViewportPixels = 100;
        viewport.SeriesAxis.OffsetUnits = 0;

        var isVisible = viewport.IsSeriesVisible(2); // y = 2 * 20 = 40

        isVisible.Should().BeTrue();
    }

    [Fact]
    public void IsSeriesVisible_ShouldReturnFalseForInvisibleSeries() {
        var viewport = new McTimelineViewport();
        viewport.SeriesAxis.SeriesHeight = 20;
        viewport.SeriesAxis.ContentUnits = 100;
        viewport.SeriesAxis.ViewportPixels = 50;
        viewport.SeriesAxis.OffsetUnits = 0;

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
        viewport.SeriesAxis.SeriesHeight = 30;
        viewport.TimeAxis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        viewport.TimeAxis.PixelsPerHour = 10;
        viewport.TimeAxis.OffsetHours = 0;
        var item = new McTimelineItem("1", "Test", "", new DateTime(2023, 1, 1, 1, 0, 0), new DateTime(2023, 1, 1, 2, 0, 0));

        var position = viewport.GetItemPosition(item, 1);

        position.X.Should().Be(10); // 1 hour * 10 px
        position.Y.Should().Be(30); // seriesIndex 1 * 30
        position.Width.Should().Be(10); // 1 hour * 10 px
    }

    [Fact]
    public void VisibleTimeRange_ShouldReflectCurrentScrollState() {
        var viewport = new McTimelineViewport();
        viewport.TimeAxis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        viewport.TimeAxis.PixelsPerHour = 10;
        viewport.TimeAxis.ViewportPixels = 240; // 24h visible
        viewport.TimeAxis.OffsetHours = 24; // scrolled 1 day

        var (start, end) = viewport.VisibleTimeRange;

        start.Should().Be(new DateTime(2023, 1, 2));
        end.Should().Be(new DateTime(2023, 1, 3));
    }

    [Fact]
    public void VisibleSeriesRange_ShouldReflectCurrentScrollState() {
        var viewport = new McTimelineViewport();
        viewport.SeriesAxis.ContentUnits = 20;
        viewport.SeriesAxis.SeriesHeight = 10;
        viewport.SeriesAxis.ViewportPixels = 50; // 5 units visible
        viewport.SeriesAxis.OffsetUnits = 4;

        var (top, bottom) = viewport.VisibleSeriesRange;

        top.Should().Be(4);
        bottom.Should().Be(9); // 4 + 5
    }

    [Fact]
    public void VisibleSeriesIndices_ShouldUpdateAfterScrollChange() {
        var viewport = new McTimelineViewport();
        viewport.SeriesAxis.ContentUnits = 10;
        viewport.SeriesAxis.SeriesHeight = 30;
        viewport.SeriesAxis.ViewportPixels = 90; // shows 3 series

        viewport.OnScrollChanged(0, 0); // offset = 0 → series 0-2 visible

        viewport.VisibleSeriesStartIndex.Should().Be(0);
        viewport.VisibleSeriesEndIndex.Should().Be(2);

        viewport.OnScrollChanged(0, 90); // offset = 3 series → series 3-5 visible

        viewport.VisibleSeriesStartIndex.Should().Be(3);
        viewport.VisibleSeriesEndIndex.Should().Be(5);
    }

    [Fact]
    public void VisibleSeriesRangeChanged_ShouldFireWhenRangeChanges() {
        var viewport = new McTimelineViewport();
        viewport.SeriesAxis.ContentUnits = 10;
        viewport.SeriesAxis.SeriesHeight = 30;
        viewport.SeriesAxis.ViewportPixels = 90;

        var fired = false;
        viewport.VisibleSeriesRangeChanged += (_, _) => fired = true;

        viewport.OnScrollChanged(0, 30); // scroll down 1 series

        fired.Should().BeTrue();
    }

    [Fact]
    public void ZoomSeriesToFit_ShouldAdjustSeriesHeightAndUpdateRange() {
        var viewport = new McTimelineViewport();
        viewport.SetSeriesRange(0, 10);
        viewport.OnSizeChanged(new Windows.Foundation.Size(400, 200));

        viewport.ZoomSeriesToFit();

        viewport.SeriesAxis.SeriesHeight.Should().BeApproximately(20, 0.001); // 200 / 10
    }

    [Fact]
    public void SeriesHeight_SetterShouldUpdateSeriesAxisAndRefreshRange() {
        var viewport = new McTimelineViewport();
        viewport.SetSeriesRange(0, 10);
        viewport.OnSizeChanged(new Windows.Foundation.Size(400, 100));

        viewport.SeriesHeight = 25;

        viewport.SeriesAxis.SeriesHeight.Should().Be(25);
    }

    [Fact]
    public void GetDayTicks_ShouldUseCorrectLabelsWithOffset() {
        var viewport = new McTimelineViewport();
        viewport.TimeAxis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 5));
        viewport.TimeAxis.PixelsPerHour = 10;
        viewport.TimeAxis.ViewportPixels = 240; // 24h visible
        viewport.TimeAxis.OffsetHours = 24; // start at 2023-01-02

        var ticks = viewport.GetDayTicks().ToList();

        ticks.Should().NotBeEmpty();
        ticks.First().Label.Should().Be("02/01");
    }

    [Fact]
    public void GetHourTicks_ShouldUseModulo24LabelsWithOffset() {
        var viewport = new McTimelineViewport();
        viewport.TimeAxis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 5));
        viewport.TimeAxis.PixelsPerHour = 10;
        viewport.TimeAxis.ViewportPixels = 30;  // 3h visible
        viewport.TimeAxis.OffsetHours = 25;     // 25h in → hour label = 25 % 24 = 1

        var ticks = viewport.GetHourTicks().ToList();

        ticks.Should().NotBeEmpty();
        ticks.First().Label.Should().Be("1:00");
    }

    [Fact]
    public void GetItemPosition_WithNonZeroOffsets_ShouldAdjustPosition() {
        var viewport = new McTimelineViewport();
        viewport.SetSeriesRange(0, 10);
        viewport.SeriesAxis.SeriesHeight = 30;
        viewport.SeriesAxis.ViewportPixels = 90; // ViewportUnits=3, MaxOffsetSteps=7
        viewport.SeriesAxis.OffsetUnits = 2;     // scrolled down 2 series
        viewport.TimeAxis.SetRange(new DateTime(2023, 1, 1), new DateTime(2023, 1, 5));
        viewport.TimeAxis.ViewportPixels = 100;
        viewport.TimeAxis.PixelsPerHour = 10;
        viewport.TimeAxis.OffsetHours = 1;       // scrolled 1 hour
        var item = new McTimelineItem("1", "T", "", new DateTime(2023, 1, 1, 3, 0, 0), new DateTime(2023, 1, 1, 4, 0, 0));

        var position = viewport.GetItemPosition(item, 4);

        // X: (3h - 1h) * 10 = 20
        // Y: (4 - 2) * 30 = 60
        // Width: 1h * 10 = 10
        position.X.Should().Be(20);
        position.Y.Should().Be(60);
        position.Width.Should().Be(10);
    }
}