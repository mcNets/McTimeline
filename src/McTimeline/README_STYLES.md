# McTimeline Theme Resources

The following resources are automatically available when you add the McTimeline library to your project.

## Automatic Theme Support

The McTimeline control automatically responds to theme changes through WinUI's `ThemeResource` system. No additional configuration is required - simply change the theme at the app or page level and all `ThemeResource` bindings will update automatically.

## Colors

| Resource | Description | Light | Dark |
|--------|------------|-------|------|
| `McTimelineAccentColor` | Primary accent color | `#0078D4` | `#60A5FA` |
| `McTimelineBackgroundColor` | Background color | `#FFFFFF` | `#1E1E1E` |
| `McTimelineForegroundColor` | Text color | `#000000` | `#FFFFFF` |
| `McTimelineGridLineColor` | Grid line color | `#E0E0E0` | `#3F3F3F` |
| `McTimelineLegendBackgroundColor` | Legend background | `#F5F5F5` | `#2D2D2D` |
| `McTimelineMinimapBackgroundColor` | Minimap background | `#FAFAFA` | `#252525` |
| `McTimelineHoverColor` | Hover color | `#E5F3FF` | `#2D4F6E` |
| `McTimelineSelectionColor` | Selection color | `#CCE8FF` | `#1E3A5F` |

## Brushes

| Resource | Description |
|--------|------------|
| `McTimelineAccentBrush` | Accent brush |
| `McTimelineBackgroundBrush` | Background brush |
| `McTimelineForegroundBrush` | Foreground brush |
| `McTimelineGridLineBrush` | Grid lines brush |
| `McTimelineLegendBackgroundBrush` | Legend background brush |
| `McTimelineMinimapBackgroundBrush` | Minimap background brush |
| `McTimelineHoverBrush` | Hover brush |
| `McTimelineSelectionBrush` | Selection brush |

## Dimensions

| Resource | Value | Description |
|--------|-------|------------|
| `McTimelineDefaultRowHeight` | `40` | Default row height |
| `McTimelineDefaultLegendWidth` | `200` | Legend width |
| `McTimelineDefaultPixelsPerHour` | `60` | Pixels per hour (zoom) |
| `McTimelineMinPixelsPerHour` | `5` | Minimum zoom |
| `McTimelineMaxPixelsPerHour` | `300` | Maximum zoom |
| `McTimelineDefaultTimeScaleHeight` | `50` | Time scale height |
| `McTimelineMinimapHeight` | `80` | Minimap height |

## Thickness

| Resource | Value |
|--------|-------|
| `McTimelineDefaultPadding` | `8` |
| `McTimelineItemPadding` | `4,2` |
| `McTimelineBorderThickness` | `1` |

## Fonts

| Resource | Value |
|--------|-------|
| `McTimelineFontFamily` | `Segoe UI` |
| `McTimelineFontSize` | `14` |
| `McTimelineSmallFontSize` | `12` |
| `McTimelineLargeFontSize` | `16` |

## Customizable Style Properties

The McTimeline control exposes the following style properties that allow you to customize different parts:

| Property | Type | Description |
|----------|------|-------------|
| `TimeScaleStyle` | `Style` | Style for the time scale grid (top section) |
| `LegendStyle` | `Style` | Style for the legend border (left section) |
| `LegendItemStyle` | `Style` | Style for each legend item border |
| `TimelineScrollStyle` | `Style` | Style for the timeline ScrollViewer |
| `TimelineCanvasStyle` | `Style` | Style for the timeline Canvas |
| `LegendItemTemplate` | `DataTemplate` | Custom template for legend items |

## Usage in Projects

### Using Theme Resources

Resources are loaded automatically. To use them in other controls:

```xaml
<Border Background="{ThemeResource McTimelineAccentBrush}">
    <TextBlock Foreground="{ThemeResource McTimelineForegroundBrush}"
               FontSize="{ThemeResource McTimelineFontSize}" />
</Border>
```

### Basic Control Usage

```xaml
<Page xmlns:mctl="using:McTimeline">
    <!-- Control automatically adapts to page/app theme -->
    <mctl:McTimeline />
</Page>
```

### Theme Management

The control uses standard WinUI `RequestedTheme` property inherited from `FrameworkElement`:

```xaml
<!-- Use app/page theme (default) -->
<mctl:McTimeline />

<!-- Force specific theme -->
<mctl:McTimeline RequestedTheme="Dark" />
<mctl:McTimeline RequestedTheme="Light" />

<!-- Page-level theme change affects all controls -->
<Page RequestedTheme="Dark">
    <mctl:McTimeline /> <!-- Will use Dark theme -->
</Page>
```

### Programmatic Theme Change

```csharp
// Change theme for entire page (affects McTimeline and all other controls)
this.RequestedTheme = this.ActualTheme == ElementTheme.Dark 
    ? ElementTheme.Light 
    : ElementTheme.Dark;

// Change theme only for McTimeline control
myTimeline.RequestedTheme = ElementTheme.Dark;
```

### Customizing Control Parts

```xaml
<Page xmlns:mctl="using:McTimeline">
    <Page.Resources>
        <!-- Custom style for legend -->
        <Style x:Key="CustomLegendStyle" TargetType="Border">
            <Setter Property="Background" Value="#2A2A2A" />
            <Setter Property="BorderBrush" Value="Orange" />
            <Setter Property="BorderThickness" Value="2,0,2,0" />
        </Style>
        
        <!-- Custom style for time scale -->
        <Style x:Key="CustomTimeScaleStyle" TargetType="Grid">
            <Setter Property="Background" Value="#1E1E1E" />
            <Setter Property="BorderBrush" Value="DodgerBlue" />
            <Setter Property="BorderThickness" Value="0,0,0,3" />
        </Style>
        
        <!-- Custom legend item template -->
        <DataTemplate x:Key="CustomLegendItemTemplate">
            <Border Background="#333" Padding="12,8" Margin="4">
                <StackPanel Orientation="Horizontal">
                    <Ellipse Width="12" Height="12" Fill="Orange" Margin="0,0,8,0" />
                    <TextBlock Text="{Binding}" FontWeight="Bold" Foreground="White" />
                </StackPanel>
            </Border>
        </DataTemplate>
    </Page.Resources>

    <mctl:McTimeline LegendStyle="{StaticResource CustomLegendStyle}"
                     TimeScaleStyle="{StaticResource CustomTimeScaleStyle}"
                     LegendItemTemplate="{StaticResource CustomLegendItemTemplate}" />
</Page>
```

### Overriding Theme Colors

To override colors globally in your app:

```xaml
<!-- App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <!-- Light theme overrides -->
            <ResourceDictionary x:Key="Light">
                <Color x:Key="McTimelineAccentColor">#FF5722</Color>
                <SolidColorBrush x:Key="McTimelineAccentBrush" 
                                 Color="{StaticResource McTimelineAccentColor}" />
            </ResourceDictionary>
            
            <!-- Dark theme overrides -->
            <ResourceDictionary x:Key="Dark">
                <Color x:Key="McTimelineAccentColor">#FF8A65</Color>
                <SolidColorBrush x:Key="McTimelineAccentBrush" 
                                 Color="{StaticResource McTimelineAccentColor}" />
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## How Theme Changes Work

1. **Automatic detection**: WinUI framework monitors `RequestedTheme` at app/page/control level
2. **ThemeResource resolution**: When theme changes, all `{ThemeResource}` bindings automatically re-evaluate
3. **Theme dictionaries**: `ThemeDictionaries` in `Generic.xaml` provide Light/Dark variants
4. **No code required**: The framework handles everything - no manual refresh needed

### Theme Resolution Order

1. Control's `RequestedTheme` (if set explicitly)
2. Parent page's `RequestedTheme` (if set)
3. App's `RequestedTheme` (if set)
4. System theme preference (default)

## Template Parts

The following named template parts are available for advanced customization:

| Part Name | Type | Description |
|-----------|------|-------------|
| `PART_Container` | `Grid` | Main container grid |
| `PART_TimeScaleGrid` | `Grid` | Time scale grid |
| `PART_TimeScaleDays` | `Canvas` | Canvas for day markers |
| `PART_TimeScaleHours` | `Canvas` | Canvas for hour markers |
| `PART_LegendBorder` | `Border` | Legend border container |
| `PART_SeriesRepeater` | `ItemsRepeater` | Legend items repeater |
| `PART_TimelineScroll` | `ScrollViewer` | Timeline scroll viewer |
| `PART_TimelineCanvas` | `Canvas` | Main timeline canvas |
