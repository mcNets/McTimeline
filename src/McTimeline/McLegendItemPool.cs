namespace McTimeline;

/// <summary>
/// Manages a pool of reusable UI elements for legend items to improve performance.
/// Each legend item consists of a UIElement (typically a Border containing a TextBlock).
/// </summary>
public sealed class McLegendItemPool : IDisposable, IMcLegendItemPool {
    private readonly Queue<UIElement> _availableItems = new();
    private readonly Style? _legendBorderStyle;
    private readonly Style? _legendTextStyle;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the McLegendItemPool class.
    /// </summary>
    /// <param name="legendBorderStyle">The style to apply to the Border elements.</param>
    /// <param name="legendTextStyle">The style to apply to the TextBlock elements.</param>
    public McLegendItemPool(Style? legendBorderStyle, Style? legendTextStyle) {
        _legendBorderStyle = legendBorderStyle;
        _legendTextStyle = legendTextStyle;
    }

    /// <summary>
    /// Gets a reusable UIElement for a legend item.
    /// If no items are available in the pool, creates a new one.
    /// </summary>
    /// <param name="text">The text to display in the legend item.</param>
    /// <returns>A UIElement configured for the legend item.</returns>
    public UIElement GetLegendItem(string text) {
        Border border;
        if (_availableItems.Count > 0) {
            border = (Border)_availableItems.Dequeue();
            border.Style = _legendBorderStyle;
            // Update the text
            if (border.Child is TextBlock textBlock) {
                textBlock.Text = text;
                textBlock.Style = _legendTextStyle; // Ensure style is applied
            }
        }
        else {
            // Create new
            border = new Border {
                Style = _legendBorderStyle
            };
            var textBlock = new TextBlock {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                Style = _legendTextStyle
            };
            border.Child = textBlock;
        }
        return border;
    }

    /// <summary>
    /// Returns a UIElement to the pool for reuse.
    /// </summary>
    /// <param name="element">The UIElement to recycle.</param>
    public void RecycleLegendItem(UIElement element) {
        // Clear any bindings or reset state if needed
        _availableItems.Enqueue(element);
    }

    /// <summary>
    /// Clears the pool, disposing of all cached elements.
    /// </summary>
    public void Clear() {
        _availableItems.Clear();
    }

    /// <summary>
    /// Releases all resources used by the McLegendItemPool.
    /// </summary>
    public void Dispose() {
        if (!_disposed) {
            Clear();
            _disposed = true;
        }
    }
}