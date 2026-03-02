using McTimeline;

namespace McTimelineDemo.Controls;

/// <summary>
/// Interaction logic for ShortKeysControl.xaml
/// </summary>
public partial class SelectedItemControl : UserControl {
    public SelectedItemControl() {
        InitializeComponent();
        DataContext = this;
    }

    public McTimelineItem Item {
        get { return (McTimelineItem)GetValue(ItemProperty); }
        set { SetValue(ItemProperty, value); }
    }

    public static readonly DependencyProperty ItemProperty =
        DependencyProperty.Register(nameof(Item), typeof(McTimelineItem), typeof(SelectedItemControl), new PropertyMetadata(null, OnTimelineItemChange));

    private static void OnTimelineItemChange(DependencyObject dp, DependencyPropertyChangedEventArgs e) {
        if (dp is SelectedItemControl control) {
                var item = (McTimelineItem)e.NewValue;
                if (item != null) {
                    control.PART_Title.Text = item.Title;
                    control.PART_Description.Text = item.Description;
                    control.PART_Range.Text = $"{item.Start:yyyy-MM-dd} - {item.End:yyyy-MM-dd}";
                    control.PART_IdKey.Text = item.IdKey;
            }
        }
    }

    public string ButtonName {
        get { return (string)GetValue(ButtonNameProperty); }
        set { SetValue(ButtonNameProperty, value); }
    }

    public static readonly DependencyProperty ButtonNameProperty =
        DependencyProperty.Register(nameof(ButtonName), typeof(string), typeof(SelectedItemControl), new PropertyMetadata(string.Empty));
}
