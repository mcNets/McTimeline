using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace McTimeline.Controls;

/// <summary>
/// Represents a visual bar element that can be displayed on the timeline.
/// This control is used to render individual timeline items with text and tooltip support.
/// </summary>
public sealed class McTimelineBar : Control {
    public McTimelineBar() {
        this.DefaultStyleKey = typeof(McTimelineBar);
    }

    /// <summary>
    /// Gets or sets the text displayed on the timeline bar.
    /// </summary>
    public string ItemText {
        get => (string)GetValue(ItemTextProperty);
        set => SetValue(ItemTextProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="ItemText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemTextProperty =
        DependencyProperty.Register(
            nameof(ItemText),
            typeof(string),
            typeof(McTimelineBar),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the tooltip content for the timeline bar.
    /// This can be any object, including strings or custom UI elements.
    /// </summary>
    public object ItemToolTip {
        get => GetValue(ItemToolTipProperty);
        set => SetValue(ItemToolTipProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="ItemToolTip"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemToolTipProperty =
        DependencyProperty.Register(
            nameof(ItemToolTip),
            typeof(object),
            typeof(McTimelineBar),
            new PropertyMetadata(null));
}
