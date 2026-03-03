using FluentAssertions;
using McTimeline;

namespace Timeline.Tests;

public class McTimelineSeriesTests {
    [Fact]
    public void Constructor_WithTitle_ShouldSetTitle() {
        var series = new McTimelineSeries("My Series");

        series.Title.Should().Be("My Series");
        series.Items.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithTitleAndItems_ShouldInitializeItems() {
        var items = new List<McTimelineItem> {
            new("k1", "A", "", new DateTime(2023, 1, 1), new DateTime(2023, 1, 2)),
            new("k2", "B", "", new DateTime(2023, 1, 3), new DateTime(2023, 1, 4))
        };

        var series = new McTimelineSeries("Series", items);

        series.Items.Should().HaveCount(2);
    }

    [Fact]
    public void AddByParts_ShouldAddItemToCollection() {
        var series = new McTimelineSeries("S");
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 1, 2);

        series.Add("key", "Title", "Desc", start, end);

        series.Items.Should().HaveCount(1);
        series.Items[0].IdKey.Should().Be("key");
        series.Items[0].Title.Should().Be("Title");
        series.Items[0].Start.Should().Be(start);
        series.Items[0].End.Should().Be(end);
    }

    [Fact]
    public void AddItem_ShouldAddItemToCollection() {
        var series = new McTimelineSeries("S");
        var item = new McTimelineItem("k", "T", "", new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));

        series.Add(item);

        series.Items.Should().Contain(item);
    }

    [Fact]
    public void Remove_ShouldRemoveItemFromCollection() {
        var series = new McTimelineSeries("S");
        var item = new McTimelineItem("k", "T", "", new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        series.Add(item);

        series.Remove(item);

        series.Items.Should().BeEmpty();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems() {
        var series = new McTimelineSeries("S");
        series.Add("1", "A", "", new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        series.Add("2", "B", "", new DateTime(2023, 2, 1), new DateTime(2023, 2, 2));

        series.Clear();

        series.Items.Should().BeEmpty();
    }

    [Fact]
    public void Sort_ShouldOrderItemsByStartDateAscending() {
        var series = new McTimelineSeries("S");
        var later = new McTimelineItem("2", "Later", "", new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));
        var earlier = new McTimelineItem("1", "Earlier", "", new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        series.Add(later);
        series.Add(earlier);

        series.Sort();

        series.Items[0].Should().Be(earlier);
        series.Items[1].Should().Be(later);
    }

    [Fact]
    public void SortDescending_ShouldOrderItemsByStartDateDescending() {
        var series = new McTimelineSeries("S");
        var earlier = new McTimelineItem("1", "Earlier", "", new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        var later = new McTimelineItem("2", "Later", "", new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));
        series.Add(earlier);
        series.Add(later);

        series.SortDescending();

        series.Items[0].Should().Be(later);
        series.Items[1].Should().Be(earlier);
    }

    [Fact]
    public void MinDate_ShouldReturnEarliestStartDate() {
        var series = new McTimelineSeries("S");
        series.Add("1", "A", "", new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));
        series.Add("2", "B", "", new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));

        series.MinDate.Should().Be(new DateTime(2023, 1, 1));
    }

    [Fact]
    public void MaxDate_ShouldReturnLatestEndDate() {
        var series = new McTimelineSeries("S");
        series.Add("1", "A", "", new DateTime(2023, 1, 1), new DateTime(2023, 1, 10));
        series.Add("2", "B", "", new DateTime(2023, 2, 1), new DateTime(2023, 2, 28));

        series.MaxDate.Should().Be(new DateTime(2023, 2, 28));
    }

    [Fact]
    public void MinDate_ShouldReturnMinValueWhenEmpty() {
        var series = new McTimelineSeries("S");

        series.MinDate.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void MaxDate_ShouldReturnMinValueWhenEmpty() {
        var series = new McTimelineSeries("S");

        series.MaxDate.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void PropertyChanged_ShouldFireWhenTitleChanges() {
        var series = new McTimelineSeries("Original");
        var changed = false;
        series.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(McTimelineSeries.Title)) changed = true;
        };

        series.Title = "Updated";

        changed.Should().BeTrue();
    }

    [Fact]
    public void PropertyChanged_ShouldNotFireWhenTitleSetToSameValue() {
        var series = new McTimelineSeries("Same");
        var count = 0;
        series.PropertyChanged += (_, _) => count++;

        series.Title = "Same";

        count.Should().Be(0);
    }

    [Fact]
    public void ToString_ShouldReturnTitle() {
        var series = new McTimelineSeries("My Series");

        series.ToString().Should().Be("My Series");
    }

    [Fact]
    public void GetEnumerator_ShouldIterateThroughAllItems() {
        var series = new McTimelineSeries("S");
        series.Add("1", "A", "", new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));
        series.Add("2", "B", "", new DateTime(2023, 2, 1), new DateTime(2023, 2, 2));

        var items = series.ToList();

        items.Should().HaveCount(2);
    }

    [Fact]
    public void ReadOnlyItems_ShouldReflectCurrentItems() {
        var series = new McTimelineSeries("S");
        series.Add("1", "A", "", new DateTime(2023, 1, 1), new DateTime(2023, 1, 2));

        series.ReadOnlyItems.Should().HaveCount(1);
    }
}
