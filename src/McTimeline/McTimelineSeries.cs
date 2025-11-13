using System.Collections;
using System.Collections.ObjectModel;

namespace McTimeline;

public class McTimelineSeries : IEnumerable<McTimelineItem> {
    public McTimelineSeries(string title) {
        Title = title;
        Items = new ObservableCollection<McTimelineItem>();
        ReadOnlyItems = new ReadOnlyObservableCollection<McTimelineItem>(Items);
    }

    public McTimelineSeries(string title, List<McTimelineItem> items) {
        ArgumentNullException.ThrowIfNull(items);
        Title = title;
        Items = new ObservableCollection<McTimelineItem>(items);
        ReadOnlyItems = new ReadOnlyObservableCollection<McTimelineItem>(Items);
    }

    public string? Title { get; set; }

    public ObservableCollection<McTimelineItem> Items { get; }

    public ReadOnlyObservableCollection<McTimelineItem> ReadOnlyItems { get; }


    public DateTime MinDate {
        get {
            return Items.Count == 0 ? DateTime.MinValue : Items.Min(i => i.Start);
        }
    }

    public DateTime MaxDate {
        get {
            return Items.Count == 0 ? DateTime.MinValue : Items.Max(i => i.End);
        }
    }

    public Style? SeriesStyle { get; set; }

    public IEnumerator<McTimelineItem> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(string key, string title, string description, DateTime start, DateTime end) {
        Items.Add(new McTimelineItem(key, title, description, start, end));
    }

    public void Add(McTimelineItem item) {
        Items.Add(item);
    }

    public void Remove(McTimelineItem item) => Items.Remove(item);

    public void Clear() {
        Items.Clear();
    }

    public void Sort() => ApplyOrdering(Items.OrderBy(i => i.Start));

    public void SortDescending() => ApplyOrdering(Items.OrderByDescending(i => i.Start));

    private void ApplyOrdering(IOrderedEnumerable<McTimelineItem> orderedItems) {
        var snapshot = orderedItems.ToList();
        for (int targetIndex = 0; targetIndex < snapshot.Count; targetIndex++) {
            var item = snapshot[targetIndex];
            int currentIndex = Items.IndexOf(item);
            if (currentIndex >= 0 && currentIndex != targetIndex) {
                Items.Move(currentIndex, targetIndex);
            }
        }
    }
}

