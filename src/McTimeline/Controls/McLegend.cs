namespace McTimeline.Controls;

/// <summary>
/// Simple legend control composed of a border and centered text with overridable styles.
/// </summary>
public sealed class McLegend : Control {
    public McLegend() {
        DefaultStyleKey = typeof(McLegend);
    }

    /// <summary>
    /// Gets or sets the text displayed by the legend.
    /// </summary>
    public string LegendText {
        get => (string)GetValue(LegendTextProperty);
        set => SetValue(LegendTextProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="LegendText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty LegendTextProperty = 
        DependencyProperty.Register(nameof(LegendText),
                                    typeof(string),
                                    typeof(McLegend),
                                    new PropertyMetadata(string.Empty));
}
