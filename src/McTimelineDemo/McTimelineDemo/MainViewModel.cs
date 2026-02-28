using McTimeline;

namespace McTimelineDemo;

public partial class MainViewModel : ObservableObject
{
    private int _numeradorSeries = 0;

    public MainViewModel()
    {
        Series.Clear();
    }

    [ObservableProperty]
    public partial McTimelineSeriesCollection Series { get; set; } = new();

    [ObservableProperty]
    public partial double LegendWidth { get; set; } = 200;

    [ObservableProperty]
    public partial double ScaleHeight { get; set; } = 50;

    [ObservableProperty]
    public partial double SeriesHeight { get; set; } = 30;

    [ObservableProperty]
    public partial bool IsLegendVisible { get; set; } = true;

    [ObservableProperty]
    public partial DateTime DataInici { get; set; } = DateTime.Now.AddMonths(-6);

    [ObservableProperty]
    public partial DateTime DataFinal { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial TimelineItem? LastClickedItem { get; set; } = null;

    public string LastClickedItemId => LastClickedItem?.IdKey ?? "(none)";
    public string LastClickedItemTitle => LastClickedItem?.Title ?? "-";
    public string LastClickedItemDescription => LastClickedItem?.Description ?? "-";
    public string LastClickedItemRange => LastClickedItem == null ? "-" : $"{LastClickedItem.Start:g} - {LastClickedItem.End:g}";

    [ObservableProperty]
    public partial string LastClickedSeries { get; set; } = "-";

    [ObservableProperty]
    public partial string LastClickedButton { get; set; } = "-";

    [ObservableProperty]
    public partial string LastClickedSeriesTitle { get; set; } = "-";

    [ObservableProperty]
    public partial string LastClickedSeriesButton { get; set; } = "-";

    partial void OnLastClickedItemChanged(TimelineItem? value)
    {
        OnPropertyChanged(nameof(LastClickedItemId));
        OnPropertyChanged(nameof(LastClickedItemTitle));
        OnPropertyChanged(nameof(LastClickedItemDescription));
        OnPropertyChanged(nameof(LastClickedItemRange));
    }

    public void UpdateLastClicked(McTimelineItemClickedEventArgs args)
    {
        LastClickedItem = new TimelineItem(
            args.Item.IdKey,
            args.Item.Title,
            args.Item.Description,
            args.Item.Start,
            args.Item.End);
        LastClickedSeries = $"Serie {args.SeriesIndex}";
        LastClickedButton = args.Button.ToString();
    }

    public void UpdateLastSeriesClicked(McTimelineSeriesClickedEventArgs args)
    {
        LastClickedSeries = $"Serie {args.SeriesIndex}";
        LastClickedSeriesTitle = args.Series.Title ?? "-";
        LastClickedSeriesButton = args.Button.ToString();
    }

    [RelayCommand]
    public async Task AddNewSeries()
    {
        var itemsCount = 500;
        Series.Add(new McTimelineSeries($"Serie {_numeradorSeries++}", 
            [.. MockTimelineItemsSeries.Generate(itemsCount, DataInici, DataFinal)]));
    }
}
