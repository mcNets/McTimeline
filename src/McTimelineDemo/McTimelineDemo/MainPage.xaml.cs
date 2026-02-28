using McTimeline;

namespace McTimelineDemo;

public sealed partial class MainPage : Page
{
    private bool _isConfigPanelExpanded = true;

    public MainViewModel ViewModel { get; } = new MainViewModel();

    public MainPage()
    {
        this.InitializeComponent();
        TimelineControl.ItemClicked += TimelineControl_ItemClicked;
        TimelineControl.SeriesClicked += TimelineControl_SeriesClicked;
        UpdateConfigPanelState();
        Task.Run(async () => await ViewModel.AddNewSeries());
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

    private void ConfigToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isConfigPanelExpanded = !_isConfigPanelExpanded;
        UpdateConfigPanelState();
    }

    private void UpdateConfigPanelState()
    {
        if (_isConfigPanelExpanded)
        {
            ConfigPanelContainer.Visibility = Visibility.Visible;
            ConfigColumn.Width = GridLength.Auto;
            ConfigToggleButton.Content = "⟪";
        }
        else
        {
            ConfigPanelContainer.Visibility = Visibility.Collapsed;
            ConfigColumn.Width = new GridLength(0);
            ConfigToggleButton.Content = "⟫";
        }
    }
}
