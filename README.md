# McTimeline

McTimeline is a reusable WinUI/Uno Platform timeline control designed to visualize time-based data across multiple series. It ships as a multi-targeted class library (`net9.0`, `net9.0-windows10.0.26100`, `net9.0-ios`, `net9.0-android`, `net9.0-desktop`, `net9.0-browserwasm`) so it can be consumed from WinAppSDK, Uno Platform, and cross-platform projects that share XAML and C#.

## Features

- Timeline surface rendered on a `Canvas` for precise placement of scheduled items
- Legend column powered by `ItemsRepeater` so you can bind any collection of series labels
- Theme-aware resource dictionaries (light and dark) that respond automatically to `RequestedTheme`
- Customizable visual parts via dependency properties (`TimeScaleStyle`, `LegendStyle`, `LegendItemStyle`, `TimelineScrollStyle`, `TimelineCanvasStyle`, `LegendItemTemplate`)
- Exposed template parts (`PART_*`) to enable deeper styling or custom drawing logic when overriding the default control template

## Getting Started

1. Reference the `McTimeline` project from your app or add the published package when available.
2. Ensure the `xmlns:mctl="using:McTimeline"` namespace is declared in your XAML.
3. Drop the control into your page and bind it to your data:

```xaml
<Page
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mctl="using:McTimeline">
    <mctl:McTimeline Height="400" Width="800" />
</Page>
```

> Tip: The control inherits `RequestedTheme`, so it will automatically pick up app or page-level theme changes.

## Customizing The Look

Every visual section of the control can be restyled. Define custom resources or templates and apply them via the dependency properties above:

```xaml
<Page.Resources>
    <Style x:Key="MyLegendStyle" TargetType="Border">
        <Setter Property="Background" Value="DarkSlateGray" />
        <Setter Property="BorderBrush" Value="LightGray" />
    </Style>
</Page.Resources>

<mctl:McTimeline LegendStyle="{StaticResource MyLegendStyle}" />
```

Light and dark theme resource dictionaries are provided out of the box. See `src/McTimeline/README_STYLES.md` for the full catalog of colors, brushes, dimensions, and template parts that ship with the control.

## Template Parts

Advanced scenarios can replace the default control template to draw custom time scale, legend, or item visuals. The default template exposes the following named parts:

| Part | Type | Purpose |
| --- | --- | --- |
| `PART_Container` | `Grid` | Root layout container |
| `PART_TimeScaleGrid` | `Grid` | Hosts day/hour canvases |
| `PART_TimeScaleDays` | `Canvas` | Draw day headers |
| `PART_TimeScaleHours` | `Canvas` | Draw hour ticks |
| `PART_LegendBorder` | `Border` | Wraps legend content |
| `PART_SeriesRepeater` | `ItemsRepeater` | Renders series labels |
| `PART_TimelineScroll` | `ScrollViewer` | Provides scroll/zoom viewport |
| `PART_TimelineCanvas` | `Canvas` | Surface for timeline items |

When you provide a custom `ControlTemplate`, ensure these parts are preserved (or replaced with equivalents) so the code-behind can locate them during `OnApplyTemplate`.

## Demo Application

A sample app lives under `src/McTimelineDemo/`. It shows how to embed the control in an Uno Platform solution, toggle between light and dark themes, and start experimenting with custom styles.

## Roadmap & Contributions

The current implementation focuses on structure and styling hooks. Rendering logic for time scale markers, item positioning, and minimap interactions is evolving. Contributions and issue reports are welcome—open a discussion or pull request describing proposed improvements.
