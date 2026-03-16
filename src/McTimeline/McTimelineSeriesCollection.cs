using System.Collections.ObjectModel;
using System.ComponentModel;

namespace McTimeline;

/// <summary>
/// Represents a collection of timeline series that can be observed for changes.
/// </summary>
public partial class McTimelineSeriesCollection : ObservableCollection<McTimelineSeries>, IDisposable {
    #region Private fields
    private bool _disposed;
    private string _name = string.Empty;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="McTimelineSeriesCollection"/> class.
    /// </summary>
    public McTimelineSeriesCollection() {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McTimelineSeriesCollection"/> class with the specified series.
    /// </summary>
    /// <param name="series">The collection of series to initialize with.</param>
    public McTimelineSeriesCollection(IEnumerable<McTimelineSeries> series) : base(series) {
    }

    /// <summary>
    /// Gets or sets the name of the series collection.
    /// </summary>
    public string Name {
        get => _name;
        set {
            if (_name != value) {
                _name = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the collection showing the name and count.
    /// </summary>
    /// <returns>A string in the format "Name/(Count)".</returns>
    public override string ToString() => $"{Name}/({Count})";

    /// <summary>
    /// Calculates the earliest date from all series in the collection.
    /// </summary>
    /// <returns>The minimum date across all series, or <see cref="DateTime.MinValue"/> if the collection is empty.</returns>
    public DateTime MinDateFromSeries() {
        if (Count == 0) {
            return DateTime.MinValue;
        }

        var minDate = DateTime.MaxValue;
        foreach (var serie in this) {
            if (minDate == DateTime.MinValue || (serie.MinDate != DateTime.MinValue && serie.MinDate < minDate)) {
                minDate = serie.MinDate;
            }
        }
        return minDate;
    }

    /// <summary>
    /// Calculates the latest date from all series in the collection.
    /// </summary>
    /// <returns>The maximum date across all series, or <see cref="DateTime.MinValue"/> if the collection is empty.</returns>
    public DateTime MaxDateFromSeries() {
        if (Count == 0) {
            return DateTime.MinValue;
        }

        var maxDate = DateTime.MaxValue;
        foreach (var serie in this) {
            if (maxDate == DateTime.MaxValue || (serie.MaxDate != DateTime.MinValue && serie.MaxDate > maxDate)) {
                maxDate = serie.MaxDate;
            }
        }
        return maxDate;
    }

    /// <summary>
    /// Releases all resources used by the series collection.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the collection and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing) {
        if (_disposed) {
            return;
        }

        if (disposing) {
            Clear();
        }

        _disposed = true;
    }
}
