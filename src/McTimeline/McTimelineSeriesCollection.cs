using System.Collections.ObjectModel;
using System.ComponentModel;

namespace McTimeline;

public partial class McTimelineSeriesCollection : ObservableCollection<McTimelineSeries>, IDisposable {
    private bool _disposed;
    private string _name = string.Empty;

    public McTimelineSeriesCollection() {
    }

    public McTimelineSeriesCollection(IEnumerable<McTimelineSeries> series) : base(series) {
    }

    public string Name {
        get => _name;
        set {
            if (_name != value) {
                _name = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }

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

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

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
