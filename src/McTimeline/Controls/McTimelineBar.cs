using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace McTimeline.Controls;

public sealed class McTimelineBar : Control {
    
    public McTimelineBar() {
        this.DefaultStyleKey = typeof(McTimelineBar);
    }

    public string ItemText {
        get => (string)GetValue(ItemTextProperty);
        set => SetValue(ItemTextProperty, value);
    }

    public static readonly DependencyProperty ItemTextProperty =
        DependencyProperty.Register(
            nameof(ItemText),
            typeof(string),
            typeof(McTimelineBar),
            new PropertyMetadata(string.Empty));

    public object ItemToolTip {
        get => GetValue(ItemToolTipProperty);
        set => SetValue(ItemToolTipProperty, value);
    }

    public static readonly DependencyProperty ItemToolTipProperty =
        DependencyProperty.Register(
            nameof(ItemToolTip),
            typeof(object),
            typeof(McTimelineBar),
            new PropertyMetadata(null));
}
