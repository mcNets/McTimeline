# Millores Proposades per McTimeline

## Millores al control (`McTimeline`)
[ ] 1. Eixos amb nivells de granularitat automatics (`dia/hora/30min/15min`) segons zoom.
[ ] 2. Zoom `fit to selection` i `fit to visible items`.
[ ] 3. Seleccio d'items (click, multiseleccio amb `Ctrl/Shift`) i estat visual de seleccio.
[ ] 4. Drag & drop per moure/reimensionar barres amb `snap` a intervals (15m, 30m, 1h).
[ ] 5. Tooltips avancats amb plantilla (`DataTemplate`) en lloc de text simple.
[ ] 6. Linia de `now` (temps actual) amb estil configurable.
[ ] 7. Capa de fons opcional: caps de setmana, franges no laborables, festius.
[x] 8. API d'esdeveniments mes rica: `ItemClicked`, `ItemDoubleClicked`, `ItemResized`, `ViewportChanged`.
[ ] 9. Export visual (PNG/SVG) del rang visible.
[ ] 10. Accessibilitat: navegacio teclat, focus visuals, automation names.

## Millores d'estil/theming
[ ]1. Brushes separats per: text escala, ticks, grid major/minor, linia `now`.
[x]2. Mides i espaiats configurables: alcada de files, alcada escala, mida font per nivell de zoom.
[ ]3. Estils diferenciats per `major ticks` (00:00, inici de dia) i `minor ticks`.
[ ]4. ~~Temes de demo predefinits (`Classic`, `High Contrast`, `Compact`).~~

## Rendiment i robustesa
[ ] 1. Reutilitzacio incremental de visuals a escala (evitar `Children.Clear()` total cada redraw).
[ ] 2. Pool separat per labels major/minor.
[ ] 3. Opcio de limit de labels per frame per evitar pics en zoom rapid.
[ ] 4. Tests de regressio de rendering (especialment limits de rang i `MinDate/MaxDate`).
[ ] 5. `Telemetry hooks` opcionals (fps estimat, count d'elements visibles).

## Millores de la demo
[ ] 1. Assistent de configuracio rapida per generar presets de vista (compacte, equilibrat, detallat) amb un clic.
[ ] 2. Escenaris de dades `preset`: 10, 100, 1000 series; rang curt/llarg; items densos/escassos.
[ ] 3. Vista `debug overlay`: rang visible, offset, pph, nombre d'elements dibuixats.
[ ] 4. Playground d'estils: canviar brushes/fonts i veure-ho instantaniament.
[ ] 5. Casos d'us reals: planificacio projectes, torns, manteniments, roadmap releases.
[ ] 6. Seccio `Known limitations / roadmap` per documentar properes fites.

## Roadmap curt recomanat (2-3 sprints)
[ ] 1. Sprint 1: seleccio + linia `now` + events basics.
[ ] 2. Sprint 2: drag/resize amb snap + tooltips amb template.
[ ] 3. Sprint 3: demo playground + presets de dades + overlay de debug.

## Propera passa (opcional)
[ ] Puc convertir aquest document en un backlog en format taula (`feature`, `impacte`, `esforc`, `risc`, `prioritat`) per facilitar la decisio de planificacio.
