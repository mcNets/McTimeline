using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Path = Microsoft.UI.Xaml.Shapes.Path;

namespace McTimeline;

public sealed partial class McTimeline : Control {
    private const double DefaultCollapsedLegendWidth = 32d;
    private Grid? _container;
    private Grid? _timeScaleGrid;
    private Canvas? _timeScaleDays;
    private Canvas? _timeScaleHours;
    private Border? _legendBorder;
    private ItemsRepeater? _seriesRepeater;
    private ScrollViewer? _timelineScroll;
    private Canvas? _timelineCanvas;
    private ColumnDefinition? _legendColumn;
    private ColumnDefinition? _timeScaleLegendColumn;
    private GridLength _legendColumnWidth;
    private GridLength _timeScaleLegendColumnWidth;
    private Button? _legendToggleButton;
    private Path? _legendToggleGlyph;

    public McTimeline() {
        this.DefaultStyleKey = typeof(McTimeline);
    }

    protected override void OnApplyTemplate() {
        base.OnApplyTemplate();

        DetachLegendToggleButton();

        _container = GetTemplateChild("PART_Container") as Grid;
        _timeScaleGrid = GetTemplateChild("PART_TimeScaleGrid") as Grid;
        _timeScaleDays = GetTemplateChild("PART_TimeScaleDays") as Canvas;
        _timeScaleHours = GetTemplateChild("PART_TimeScaleHours") as Canvas;
        _legendBorder = GetTemplateChild("PART_LegendBorder") as Border;
        _seriesRepeater = GetTemplateChild("PART_SeriesRepeater") as ItemsRepeater;
        _timelineScroll = GetTemplateChild("PART_TimelineScroll") as ScrollViewer;
        _timelineCanvas = GetTemplateChild("PART_TimelineCanvas") as Canvas;
        _legendToggleButton = GetTemplateChild("PART_LegendToggleButton") as Button;
        _legendToggleGlyph = GetTemplateChild("PART_LegendToggleGlyph") as Path;

        if (_legendBorder?.Parent is Grid legendHost && legendHost.ColumnDefinitions.Count > 0) {
            _legendColumn = legendHost.ColumnDefinitions[0];
            _legendColumnWidth = _legendColumn.Width;
            _legendColumn.MinWidth = 0;
        }

        if (_timeScaleGrid?.ColumnDefinitions.Count > 0) {
            _timeScaleLegendColumn = _timeScaleGrid.ColumnDefinitions[0];
            _timeScaleLegendColumnWidth = _timeScaleLegendColumn.Width;
            _timeScaleLegendColumn.MinWidth = 0;
        }

        AttachLegendToggleButton();
        UpdateLegendVisibility();
    }

    protected override Size MeasureOverride(Size availableSize) {
        _container?.Measure(availableSize);
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize) {
        return base.ArrangeOverride(finalSize);
    }

    private void UpdateLegendVisibility() {
        bool isVisible = IsLegendVisible;
        double collapsedWidth = GetCollapsedLegendWidth();

        if (_legendBorder is not null) {
            _legendBorder.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        if (_legendColumn is not null) {
            _legendColumn.Width = isVisible ? _legendColumnWidth : new GridLength(collapsedWidth, GridUnitType.Pixel);
            _legendColumn.MinWidth = isVisible ? 0 : collapsedWidth;
        }

        if (_timeScaleLegendColumn is not null) {
            _timeScaleLegendColumn.Width = isVisible ? _timeScaleLegendColumnWidth : new GridLength(collapsedWidth, GridUnitType.Pixel);
            _timeScaleLegendColumn.MinWidth = isVisible ? 0 : collapsedWidth;
        }

        UpdateLegendToggleVisual();
    }

    private void AttachLegendToggleButton() {
        if (_legendToggleButton is null) {
            return;
        }

        _legendToggleButton.Click += LegendToggleButton_Click;
        _legendToggleButton.SizeChanged += LegendToggleButton_SizeChanged;
    }

    private void DetachLegendToggleButton() {
        if (_legendToggleButton is null) {
            return;
        }

        _legendToggleButton.Click -= LegendToggleButton_Click;
        _legendToggleButton.SizeChanged -= LegendToggleButton_SizeChanged;
        _legendToggleButton = null;
        _legendToggleGlyph = null;
    }

    private void LegendToggleButton_Click(object sender, RoutedEventArgs e) {
        IsLegendVisible = !IsLegendVisible;
    }

    private void LegendToggleButton_SizeChanged(object sender, SizeChangedEventArgs e) {
        if (!IsLegendVisible) {
            UpdateLegendVisibility();
        }
    }

    private void UpdateLegendToggleVisual() {
        if (_legendToggleGlyph?.RenderTransform is RotateTransform rotate) {
            rotate.Angle = IsLegendVisible ? 180 : 0;
        }

        if (_legendToggleButton is not null) {
            var automationName = IsLegendVisible ? "Hide legend" : "Show legend";
            AutomationProperties.SetName(_legendToggleButton, automationName);
        }
    }

    private double GetCollapsedLegendWidth() {
        if (_legendToggleButton is null) {
            return 0d;
        }

        double width = _legendToggleButton.ActualWidth;

        if (width <= 0 && _legendToggleButton.Width > 0) {
            width = _legendToggleButton.Width;
        }

        if (width <= 0) {
            width = DefaultCollapsedLegendWidth;
        }

        return width;
    }
}
