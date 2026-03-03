# Proposed Improvements for McTimeline

## Control Improvements (`McTimeline`)
[ ] 1. Axes with automatic granularity levels (day/hour/30min/15min) according to zoom.
[ ] 2. "Fit to selection" and "fit to visible items" zoom commands.
[ ] 3. Item selection (click, multi-select with `Ctrl/Shift`) and visual selection state.
[ ] 4. Drag & drop to move/resize bars with snapping to intervals (15m, 30m, 1h).
[ ] 5. Advanced tooltips using `DataTemplate` instead of plain text.
[ ] 6. Configurable "now" line (current time) with styling options.
[ ] 7. Optional background layer: weekends, non-working hours, holidays.
[x] 8. Richer events API: `ItemClicked`, `ItemDoubleClicked`, `ItemResized`, `ViewportChanged`.
[ ] 9. Visual export (PNG/SVG) of the visible range.
[ ] 10. Accessibility: keyboard navigation, focus visuals, automation names.

## Styling / Theming Improvements
[ ] 1. Separate brushes for: scale text, ticks, major/minor grid, "now" line.
[x] 2. Configurable sizes and spacings: row height, scale height, font sizes per zoom level.
[ ] 3. Distinct styles for major ticks (00:00, start of day) and minor ticks.
[ ] 4. Predefined demo themes (`Classic`, `High Contrast`, `Compact`).

## Performance and Robustness
[ ] 1. Incremental reuse of visuals at scale (avoid clearing all children each redraw).
[ ] 2. Separate pools for major/minor labels.
[ ] 3. Option to limit labels per frame to avoid spikes during rapid zooming.
[ ] 4. Rendering regression tests (especially for range limits and `MinDate/MaxDate`).
[ ] 5. Optional telemetry hooks (estimated FPS, count of visible elements).

## Demo Improvements
[ ] 1. Quick-setup wizard to generate view presets (compact, balanced, detailed) with one click.
[ ] 2. Preset data scenarios: 10, 100, 1000 series; short/long ranges; dense/sparse items.
[ ] 3. Debug overlay: visible range, offset, pph, number of drawn elements.
[ ] 4. Style playground: change brushes/fonts and see live updates.
[ ] 5. Real-world usage examples: project planning, shifts, maintenance, release roadmaps.
[ ] 6. "Known limitations / roadmap" section to document next steps.

## Short Recommended Roadmap (2-3 sprints)
[ ] 1. Sprint 1: selection + "now" line + basic events.
[ ] 2. Sprint 2: drag/resize with snap + templated tooltips.
[ ] 3. Sprint 3: demo playground + data presets + debug overlay.

## Next Step (optional)
[ ] I can convert this document into a backlog table (`feature`, `impact`, `effort`, `risk`, `priority`) to help planning.
