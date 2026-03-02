namespace McTimelineDemo.Controls;

public sealed partial class SelectedSeriesControl : UserControl {
    public SelectedSeriesControl() {
        this.InitializeComponent();
    }

    public string Series {
        get { return (string)GetValue(SeriesProperty); }
        set { SetValue(SeriesProperty, value); }
    }

    public static readonly DependencyProperty SeriesProperty =
        DependencyProperty.Register(nameof(Series), typeof(string), typeof(SelectedSeriesControl), new PropertyMetadata(string.Empty));

    public string ButtonName {
        get { return (string)GetValue(ButtonNameProperty); }
        set { SetValue(ButtonNameProperty, value); }
    }

    public static readonly DependencyProperty ButtonNameProperty =
        DependencyProperty.Register(nameof(ButtonName), typeof(string), typeof(SelectedSeriesControl), new PropertyMetadata(string.Empty));

}
