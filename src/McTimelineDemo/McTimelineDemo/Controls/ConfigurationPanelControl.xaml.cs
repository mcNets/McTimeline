namespace McTimelineDemo.Controls;

public sealed partial class ConfigurationPanelControl : UserControl {
    public ConfigurationPanelControl() {
        this.InitializeComponent();
    }

    public event EventHandler<RoutedEventArgs>? ZoomSeriesToFit;
    public event EventHandler<RoutedEventArgs>? DefaultStyleRequested;
    public event EventHandler<RoutedEventArgs>? MulticolorRequested;
    public event EventHandler<RoutedEventArgs>? GradientsRequested;

    private void ThemeButton_Click(object sender, RoutedEventArgs e) {
        var theme = App.RootFrame.ActualTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        var srv = App.RootFrame.RequestedTheme = theme;
    }

    public ICommand AddNewSeries {
        get { return (ICommand)GetValue(AddNewSeriesProperty); }
        set { SetValue(AddNewSeriesProperty, value); }
    }

    public static readonly DependencyProperty AddNewSeriesProperty =
        DependencyProperty.Register(nameof(AddNewSeries), typeof(ICommand), typeof(ConfigurationPanelControl), new PropertyMetadata(null));

    public bool IsLegendVisible {
        get { return (bool)GetValue(IsLegendVisibleProperty); }
        set { SetValue(IsLegendVisibleProperty, value); }
    }

    public static readonly DependencyProperty IsLegendVisibleProperty =
        DependencyProperty.Register(nameof(IsLegendVisible), typeof(bool), typeof(ConfigurationPanelControl), new PropertyMetadata(true));

    public bool IsTextVisible {
        get { return (bool)GetValue(IsTextVisibleProperty); }
        set { SetValue(IsTextVisibleProperty, value); }
    }

    public static readonly DependencyProperty IsTextVisibleProperty =
        DependencyProperty.Register(nameof(IsTextVisible), typeof(bool), typeof(ConfigurationPanelControl), new PropertyMetadata(true));

    public decimal LegendWidth {
        get { return (decimal)GetValue(LegendWidthProperty); }
        set { SetValue(LegendWidthProperty, value); }
    }

    public static readonly DependencyProperty LegendWidthProperty =
        DependencyProperty.Register(nameof(LegendWidth), typeof(decimal), typeof(ConfigurationPanelControl), new PropertyMetadata(200));

    public decimal SeriesHeight {
        get { return (decimal)GetValue(SeriesHeightProperty); }
        set { SetValue(SeriesHeightProperty, value); }
    }

    public static readonly DependencyProperty SeriesHeightProperty =
        DependencyProperty.Register(nameof(SeriesHeight), typeof(decimal), typeof(ConfigurationPanelControl), new PropertyMetadata(30));

    public decimal ScaleHeight {
        get { return (decimal)GetValue(ScaleHeightProperty); }
        set { SetValue(ScaleHeightProperty, value); }
    }

    public static readonly DependencyProperty ScaleHeightProperty =
        DependencyProperty.Register(nameof(ScaleHeight), typeof(decimal), typeof(ConfigurationPanelControl), new PropertyMetadata(100));

    public decimal PixelsPerHour {
        get { return (decimal)GetValue(PixelsPerHourProperty); }
        set { SetValue(PixelsPerHourProperty, value); }
    }

    public static readonly DependencyProperty PixelsPerHourProperty =
        DependencyProperty.Register(nameof(PixelsPerHour), typeof(decimal), typeof(ConfigurationPanelControl), new PropertyMetadata(30));

    private void ZoomSeriesToFit_Click(object sender, RoutedEventArgs e) {
        ZoomSeriesToFit?.Invoke(this, e);
    }

    private void StylingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (e.AddedItems.Count == 0 || e.AddedItems[0] is not ComboBoxItem item) {
            return;
        }
        switch (item.Content as string) {
            case "Default":    DefaultStyleRequested?.Invoke(this, new RoutedEventArgs()); break;
            case "Multicolor": MulticolorRequested?.Invoke(this, new RoutedEventArgs()); break;
            case "Gradients":  GradientsRequested?.Invoke(this, new RoutedEventArgs()); break;
        }
    }
}
