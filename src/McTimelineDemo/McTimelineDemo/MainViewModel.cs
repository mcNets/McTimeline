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

    [RelayCommand]
    public async Task AddNewSeries()
    {
        var itemsCount = 500;
        Series.Add(new McTimelineSeries($"Serie {_numeradorSeries++}", [.. MockTimelineItemsSeries.Generate(itemsCount, DataInici, DataFinal)]));
    }
   

}
