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
    public partial bool IsLegendVisible { get; set; } = true;

    [RelayCommand]
    public async Task AddNewSeries()
    {
        var dataInici = DateTime.Now.AddMonths(-6);
        var dataFinal = DateTime.Now;
        var itemsCount = 500;

        //var series = new McTimelineSeriesCollection();
        Series.Add(new McTimelineSeries($"Serie {_numeradorSeries++}", [.. MockTimelineItemsSeries.Generate(itemsCount, dataInici, dataFinal)]));
        //Series = series;
    }
   

}
