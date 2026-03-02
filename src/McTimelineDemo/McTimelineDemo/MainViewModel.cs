using McTimeline;

namespace McTimelineDemo;

public partial class MainViewModel : ObservableObject {
    private int _numeradorSeries = 0;

    public MainViewModel() {
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

    /*
     * SelectedItem
     */

    [ObservableProperty]
    public partial McTimelineItem? SelectedItem { get; set; } = null;

    [ObservableProperty]
    public partial string SelectedItemSeries { get; set; } = "";

    [ObservableProperty]
    public partial string SelectedItemButton { get; set; } = "";

    /*
     * SelectedSeries
     */

    [ObservableProperty]
    public partial string SelectedSeriesButton { get; set; } = "";

    [ObservableProperty]
    public partial string SelectedSeriesTitle { get; set; } = "";

    public void UpdateLastClicked(McTimelineItemClickedEventArgs args) {
        SelectedItem = new McTimelineItem(
            args.Item.IdKey,
            args.Item.Title,
            args.Item.Description,
            args.Item.Start,
            args.Item.End);
        SelectedItemSeries = $"Serie {args.SeriesIndex}";
        SelectedItemButton = args.Button.ToString() + " Button";
    }

    public void UpdateLastSeriesClicked(McTimelineSeriesClickedEventArgs args) {
        SelectedSeriesTitle = args.Series.Title ?? "";
        SelectedSeriesButton = args.Button.ToString() + " Button";
    }

    [RelayCommand]
    public async Task AddNewSeries() {
        var itemsCount = 500;
        for (int i = 0; i < 4; i++) {
            Series.Add(new McTimelineSeries($"Serie {_numeradorSeries++}",
                [.. MockTimelineItemsSeries.Generate(itemsCount, DataInici, DataFinal)]));
        }
    }
}
