# GitHub Copilot Instructions – TimelineControl for WinUI/Uno

This file guides Copilot (and you) to continue developing the `TimelineControl` component.
The goal: a high-performance horizontal timeline/Gantt-style control with legend, time scale, zoom, and minimap.

---

## Technology Stack

* **Platform:** WinUI 3 or Uno Platform (WinUI targets)
* **Language:** C# + XAML
* **Rendering:** Canvas + manual virtualization (not Win2D yet)
* **Editor:** VS Code + GitHub Copilot or Visual Studio with Copilot

---

## Control Purpose

Visualize time intervals across multiple series (rows):

* Left fixed legend (series names)
* Top dual time axis (days + hours)
* Scrollable bars timeline
* Smooth zoom (Ctrl + mouse wheel)
* API to scroll to item or time
* Virtualized rectangle pooling

---

## Data Model

```csharp
public record TimelineItem(
    string SeriesId,
    string KeyId,
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
* `MinTime` / `MaxTime`
* `PixelsPerHour` (zoom factor)
* `RowHeight`
* `SeriesLabelWidth`

### Methods

```csharp
void ScrollToItem(string keyId, bool center = false);
void ScrollToTime(DateTime time, bool center = false);
void EnsureItemVisible(TimelineItem item);
void ZoomIn(); void ZoomOut(); void ResetZoom();
```

### Commands

* `ScrollToItemCommand`
* `ScrollToTimeCommand`
* `EnsureVisibleCommand`

---

## Template Parts (Generic.xaml)

Must exist:

* `PART_TimelineScroll` – ScrollViewer
* `PART_TimelineCanvas` – Canvas (bars)
* `PART_SeriesRepeater` – ItemsRepeater (legend)
* `PART_TimeScaleDays` / `PART_TimeScaleHours`

Layout:

```
Time Scale
┌────Legend────┬────────Timeline─────────┐
|              | Canvas + ScrollViewer   |
```

---

## Virtualization Rules

* Group items by `SeriesId`
* Sort each group by `Start`
* Binary search to find first visible item in group
* Allocate rectangles from pool, reuse, return when off-screen
* Render only items inside viewport

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
* Use `DispatcherQueue` for UI updates
* Do not create TextBlocks for each hour for huge ranges (optimize when zoomed out)

