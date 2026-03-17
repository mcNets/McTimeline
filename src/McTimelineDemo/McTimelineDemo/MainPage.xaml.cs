using McTimeline;
using System.Collections.Specialized;

namespace McTimelineDemo;

public sealed partial class MainPage : Page
{
    private enum SeriesVisualMode {
        None,
        Multicolor,
        Gradients
    }

    private bool _isConfigPanelExpanded = true;
    private SeriesVisualMode _visualMode;
    private readonly string[] _multicolorStyleKeys = [
        "TL_MulticolorSeriesStyle1",
        "TL_MulticolorSeriesStyle2",
        "TL_MulticolorSeriesStyle3",
        "TL_MulticolorSeriesStyle4"
    ];
    private readonly string[] _gradientStyleKeys = [
        "TL_GradientSeriesStyle1",
        "TL_GradientSeriesStyle2",
        "TL_GradientSeriesStyle3",
        "TL_GradientSeriesStyle4"
    ];

    public MainViewModel ViewModel { get; } = new MainViewModel();

    public MainPage()
    {
        this.InitializeComponent();
        Loaded += MainPage_Loaded;
        TimelineControl.ItemClicked += TimelineControl_ItemClicked;
        TimelineControl.SeriesClicked += TimelineControl_SeriesClicked;
        ViewModel.Series.CollectionChanged += Series_CollectionChanged;
        UpdateConfigPanelState();
        Task.Run(async () => await ViewModel.AddNewSeries());
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Apply the demo startup zoom after template/style initialization.
        ViewModel.PixelsPerHour = 30;
        TimelineControl.PixelsPerHour = 30;
        Loaded -= MainPage_Loaded;
    }

    private void Series_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_visualMode == SeriesVisualMode.Multicolor) {
            ApplyStyles(_multicolorStyleKeys);
        }
        else if (_visualMode == SeriesVisualMode.Gradients) {
            ApplyStyles(_gradientStyleKeys);
        }
    }

    private void TimelineControl_ItemClicked(object? sender, McTimelineItemClickedEventArgs e)
    {
        ViewModel.UpdateLastClicked(e);
    }

    private void TimelineControl_SeriesClicked(object? sender, McTimelineSeriesClickedEventArgs e)
    {
        ViewModel.UpdateLastSeriesClicked(e);
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        var theme = App.RootFrame.ActualTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        var srv = App.RootFrame.RequestedTheme = theme;
    }

    private void ZoomSeriesToFitButton_Click(object sender, RoutedEventArgs e)
    {
        TimelineControl.ZoomSeriesToFit();
    }

    private void MulticolorRequested_Click(object sender, RoutedEventArgs e)
    {
        _visualMode = SeriesVisualMode.Multicolor;
        ApplyStyles(_multicolorStyleKeys);
    }

    private void GradientsRequested_Click(object sender, RoutedEventArgs e)
    {
        _visualMode = SeriesVisualMode.Gradients;
        ApplyStyles(_gradientStyleKeys);
    }

    private void DefaultStyleRequested_Click(object sender, RoutedEventArgs e)
    {
        foreach (var series in ViewModel.Series) {
            series.SeriesStyle = null;
        }
    }

    private void ConfigToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isConfigPanelExpanded = !_isConfigPanelExpanded;
        UpdateConfigPanelState();
    }

    private void UpdateConfigPanelState()
    {
        if (_isConfigPanelExpanded) {
            ConfigPanelContainer.Visibility = Visibility.Visible;
            ConfigColumn.Width = GridLength.Auto;
            ConfigToggleButton.Content = "⟪";
        }
        else {
            ConfigPanelContainer.Visibility = Visibility.Collapsed;
            ConfigColumn.Width = new GridLength(0);
            ConfigToggleButton.Content = "⟫";
        }
    }

    private void ApplyStyles(string[] styleKeys)
    {
        var styles = ResolveStyles(styleKeys);
        if (styles.Count == 0) {
            return;
        }

        for (int i = 0; i < ViewModel.Series.Count; i++) {
            ViewModel.Series[i].SeriesStyle = styles[i % styles.Count];
        }
    }

    private List<Style> ResolveStyles(string[] styleKeys)
    {
        var styles = new List<Style>(styleKeys.Length);
        var resources = Application.Current?.Resources;
        if (resources == null) {
            return styles;
        }

        foreach (var key in styleKeys) {
            if (resources.TryGetValue(key, out var value) && value is Style style) {
                styles.Add(style);
            }
        }

        return styles;
    }
}
