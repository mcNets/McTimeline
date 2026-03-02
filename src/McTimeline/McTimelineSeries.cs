using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace McTimeline;

/// <summary>
/// Represents a series of timeline items that can be displayed in a timeline control.
/// This class supports data binding, observable collections, and provides methods for managing timeline items.
/// </summary>
public partial class McTimelineSeries : INotifyPropertyChanged, IEnumerable<McTimelineItem> {
    /// <summary>
    /// Initializes a new instance of the <see cref="McTimelineSeries"/> class with the specified title.
    /// </summary>
    /// <param name="title">The title of the timeline series.</param>
    public McTimelineSeries(string title) {
        Title = title;
        Items = [];
        ReadOnlyItems = new ReadOnlyObservableCollection<McTimelineItem>(Items);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McTimelineSeries"/> class with the specified title and items.
    /// </summary>
    /// <param name="title">The title of the timeline series.</param>
    /// <param name="items">The initial list of timeline items to add to this series.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
    public McTimelineSeries(string title, List<McTimelineItem> items) {
        ArgumentNullException.ThrowIfNull(items);
        Title = title;
        Items = new ObservableCollection<McTimelineItem>(items);
        ReadOnlyItems = new ReadOnlyObservableCollection<McTimelineItem>(Items);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this series is visible in the timeline.
    /// </summary>
    public bool Visible { get; set; } = false;

    /// <summary>
    /// Gets or sets the title of this timeline series.
    /// Raises the <see cref="PropertyChanged"/> event when the value changes.
    /// </summary>
    public string? Title {
        get;
        set {
            if (field != value) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Returns a string that represents the current timeline series.
    /// </summary>
    /// <returns>The title of the timeline series.</returns>
    public override string ToString() => $"{Title}";

    /// <summary>
    /// Gets the observable collection of timeline items in this series.
    /// Use this collection to add, remove, or modify items.
    /// </summary>
    public ObservableCollection<McTimelineItem> Items { get; }

    /// <summary>
    /// Gets a read-only wrapper around the <see cref="Items"/> collection.
    /// </summary>
    public ReadOnlyObservableCollection<McTimelineItem> ReadOnlyItems { get; }

    /// <summary>
    /// Gets the earliest start date of all items in this series.
    /// Returns <see cref="DateTime.MinValue"/> if the series contains no items.
    /// </summary>
    public DateTime MinDate {
        get {
            return Items.Count == 0 ? DateTime.MinValue : Items.Min(i => i.Start);
        }
    }

    /// <summary>
    /// Gets the latest end date of all items in this series.
    /// Returns <see cref="DateTime.MinValue"/> if the series contains no items.
    /// </summary>
    public DateTime MaxDate {
        get {
            return Items.Count == 0 ? DateTime.MinValue : Items.Max(i => i.End);
        }
    }

    /// <summary>
    /// Gets or sets the style to be applied to items in this series.
    /// Raises the <see cref="PropertyChanged"/> event when the value changes.
    /// </summary>
    public Style? SeriesStyle {
        get;
        set {
            if (!Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SeriesStyle)));
            }
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the timeline items.
    /// </summary>
    /// <returns>An enumerator for the items collection.</returns>
    public IEnumerator<McTimelineItem> GetEnumerator() => Items.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the timeline items.
    /// </summary>
    /// <returns>An enumerator for the items collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Adds a new timeline item to this series with the specified properties.
    /// </summary>
    /// <param name="key">The unique key for the timeline item.</param>
    /// <param name="title">The title of the timeline item.</param>
    /// <param name="description">The description of the timeline item.</param>
    /// <param name="start">The start date and time of the timeline item.</param>
    /// <param name="end">The end date and time of the timeline item.</param>
    public void Add(string key, string title, string description, DateTime start, DateTime end) {
        Items.Add(new McTimelineItem(key, title, description, start, end));
    }

    /// <summary>
    /// Adds an existing timeline item to this series.
    /// </summary>
    /// <param name="item">The timeline item to add.</param>
    public void Add(McTimelineItem item) {
        Items.Add(item);
    }

    /// <summary>
    /// Removes the specified timeline item from this series.
    /// </summary>
    /// <param name="item">The timeline item to remove.</param>
    public void Remove(McTimelineItem item) => Items.Remove(item);

    /// <summary>
    /// Removes all timeline items from this series.
    /// </summary>
    public void Clear() {
        Items.Clear();
    }

    /// <summary>
    /// Sorts the timeline items in ascending order by their start date.
    /// </summary>
    public void Sort() => ApplyOrdering(Items.OrderBy(i => i.Start));

    /// <summary>
    /// Sorts the timeline items in descending order by their start date.
    /// </summary>
    public void SortDescending() => ApplyOrdering(Items.OrderByDescending(i => i.Start));

    /// <summary>
    /// Applies the specified ordering to the items collection by moving items to their correct positions.
    /// This preserves the observable collection's change notifications.
    /// </summary>
    /// <param name="orderedItems">The ordered sequence of items to apply.</param>
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

