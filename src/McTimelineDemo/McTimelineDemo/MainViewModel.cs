using System;
using McTimeline;

namespace McTimelineDemo;

public partial class MainViewModel : ObservableObject
{
    private int _numeradorSeries = 0;

    [ObservableProperty]
    public partial McTimelineSeriesCollection Series { get; set; } = new();

    [RelayCommand]
    public async Task GenerarNovaCollecio()
    {
        var dataInici = DateTime.Now.AddMonths(-6);
        var dataFinal = DateTime.Now;
        var itemsCount = 500;

        var series = new McTimelineSeriesCollection();
        for (int i = 0; i < 6; i++)
        {
            series.Add(new McTimelineSeries($"Serie {_numeradorSeries++}", [.. MockTimelineItemsSeries.Generate(itemsCount, dataInici, dataFinal)]));
        }
        Series = series;
    }
}
