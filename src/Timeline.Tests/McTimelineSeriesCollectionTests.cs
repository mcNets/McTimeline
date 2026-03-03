using FluentAssertions;
using McTimeline;
using System.ComponentModel;

namespace Timeline.Tests;

public class McTimelineSeriesCollectionTests {
    private static McTimelineSeries MakeSeries(string title, DateTime start, DateTime end) {
        var s = new McTimelineSeries(title);
        s.Add("k", title, "", start, end);
        return s;
    }

    [Fact]
    public void DefaultConstructor_ShouldBeEmpty() {
        var collection = new McTimelineSeriesCollection();

        collection.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithSeries_ShouldInitializeWithGivenSeries() {
        var series = new List<McTimelineSeries> {
            new("S1"),
            new("S2")
        };

        var collection = new McTimelineSeriesCollection(series);

        collection.Should().HaveCount(2);
    }

    [Fact]
    public void Name_ShouldFirePropertyChangedWhenChanged() {
        var collection = new McTimelineSeriesCollection();
        var changed = false;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(McTimelineSeriesCollection.Name)) changed = true;
        };

        collection.Name = "NewName";

        changed.Should().BeTrue();
    }

    [Fact]
    public void Name_ShouldNotFirePropertyChangedWhenSetToSameValue() {
        var collection = new McTimelineSeriesCollection { Name = "Same" };
        var count = 0;
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, _) => count++;

        collection.Name = "Same";

        count.Should().Be(0);
    }

    [Fact]
    public void MinDateFromSeries_ShouldReturnMinValueForEmptyCollection() {
        var collection = new McTimelineSeriesCollection();

        collection.MinDateFromSeries().Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void MinDateFromSeries_ShouldReturnEarliestStartDate() {
        var collection = new McTimelineSeriesCollection();
        collection.Add(MakeSeries("S1", new DateTime(2023, 3, 1), new DateTime(2023, 3, 10)));
        collection.Add(MakeSeries("S2", new DateTime(2023, 1, 1), new DateTime(2023, 1, 10)));

        collection.MinDateFromSeries().Should().Be(new DateTime(2023, 1, 1));
    }

    [Fact]
    public void MaxDateFromSeries_ShouldReturnMinValueForEmptyCollection() {
        var collection = new McTimelineSeriesCollection();

        collection.MaxDateFromSeries().Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void MaxDateFromSeries_ShouldReturnLatestEndDate() {
        var collection = new McTimelineSeriesCollection();
        collection.Add(MakeSeries("S1", new DateTime(2023, 1, 1), new DateTime(2023, 1, 10)));
        collection.Add(MakeSeries("S2", new DateTime(2023, 2, 1), new DateTime(2023, 6, 15)));

        collection.MaxDateFromSeries().Should().Be(new DateTime(2023, 6, 15));
    }

    [Fact]
    public void Dispose_ShouldClearAllSeries() {
        var collection = new McTimelineSeriesCollection();
        collection.Add(new McTimelineSeries("S1"));
        collection.Add(new McTimelineSeries("S2"));

        collection.Dispose();

        collection.Should().BeEmpty();
    }

    [Fact]
    public void Dispose_CalledTwice_ShouldNotThrow() {
        var collection = new McTimelineSeriesCollection();
        collection.Add(new McTimelineSeries("S1"));

        collection.Dispose();
        var act = collection.Dispose;

        act.Should().NotThrow();
    }

    [Fact]
    public void ToString_ShouldIncludeNameAndCount() {
        var collection = new McTimelineSeriesCollection { Name = "MyCollection" };
        collection.Add(new McTimelineSeries("S1"));
        collection.Add(new McTimelineSeries("S2"));

        collection.ToString().Should().Be("MyCollection/(2)");
    }
}
