# GitHub Copilot Instructions – TimelineControl for WinUI/Uno

This file guides Copilot (and you) to continue developing the `McTimeline` Uno Platform/WinUI3 control.
The goal: a high-performance horizontal timeline/Gantt-style control with legend, time scale and zoom.

---

## Technology Stack

* **Platform:** WinUI 3 or Uno Platform
* **Language:** C# + XAML
* **Rendering:** Canvas + manual virtualization (not Win2D yet)
* **Editor:** VS Code + GitHub Copilot or Visual Studio with Copilot

---

## Control Purpose

Visualize time intervals across multiple series (rows):

* Left fixed legend (series names), can be scrolled vertically and hidden or shown. 
* Top dual time axis (days + hours)
* Scrollable bars timeline
* Smooth zoom (Ctrl + mouse wheel)
* API to scroll to item or time
* Virtualized rectangle pooling

---

## Data Model

```csharp
public record McTimelineItem(
    string IdKey,
    string Title,
    string Description,
    DateTime Start,
    DateTime End
);
```

---

## Key Public API

### Dependency Properties

* `ItemsSource` (ObservableCollection<TimelineItem>)
* `MinDate` / `MaxDate`
* `PixelsPerHour` (zoom factor)
* `RowHeight`
* `LegendWidth`

---

## Template Parts (Generic.xaml)

* `PART_Container` – Root Grid
* `PART_LegendCaption` – TextBlock
* `PART_TimeScaleGrid` – Grid (time scale)
* `PART_TimeScaleDays` - Canvas (days)
* `PART_TimeScaleHours` - Canvas (hours)
* `PART_LegendBorder` – Border (legend area)
* `PART_SeriesRepeater` – ItemsRepeater (legend rows)
* `PART_TimelineCanvas` – Canvas (timeline bars)
* `PART_VScroll` - Scrollbar (vertical)
* `PART_HScroll` - Scrollbar (horizontal)

Layout:

```
Time Scale
┌────Legend──────┬────────Timeline─────────┐
| Legend Caption | Days canvas             |
|                | Hours canvas            |
|----------------|-------------------------|
| Legend         | Timeline Canvas         |
|                |                         |
└────────────────┴─────────────────────────┘
```

---

## Virtualization Rules

* Sort each group by `Start`
* Binary search to find first visible item in group
* Allocate rectangles from pool, reuse, return when off-screen
* Render only items inside viewport
* Do not create new elements, use Span<T> or arrays for calculations

---

## Zoom Behavior

* Ctrl + mouse wheel changes `PixelsPerHour`
* Anchor zoom at mouse cursor:
 * keep same time position under pointer
* Clamp zoom values (5–300 px per hour)

---

## Prompts for Copilot

Use these when coding:

* "Add binary search helper to find first item starting after a given DateTime."
* "Implement rectangle pooling for Canvas to reduce UI elements."
* "Handle Ctrl+MouseWheel to zoom anchored at pointer and adjust ScrollViewer offset."
* "Bind ItemsRepeater to grouped series list and align rows with timeline Canvas."
* "All code, function names, comments, etc, generated must be in English."

---

## Developer Tips

* Separate `RebuildLayout()` vs `RenderVisible()`
* Use `DispatcherQueue` for UI updates if needed.
* Do not create TextBlocks for each hour for huge ranges (optimize when zoomed out)

