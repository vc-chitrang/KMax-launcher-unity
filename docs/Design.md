# Design

The visual and interaction design rules for the KMax launcher and every project it hosts. **Whoever works on UI/UX, animations or visuals (agent or person) must follow this file.** `Rules.md` points here (R-3 and the "How these rules are used" list).

**Reference:** the `Assets/Games/kmax-human-anatomy` project. Its builders hold the numbers (`AnatomyUiStyle`, `AnatomyInterfaceBuilder`, `AnatomyLauncherBuilder`), its scene holds the final values. Where this file and the reference differ, the reference wins and this file must be corrected.

**Units.** Sizes are canvas units on the 1920 x 1080 reference canvas (anatomy's own builders work at "half size", `Scale = 0.5`, and the numbers below are the result). Colours are given as hex plus 0-1 RGBA where Unity needs it. "Gamma" colour space is in use (see the render pipeline note at the end).

**Behaviour is out of scope.** In a newly added project this file only governs how things look, move and sound. It never licenses changing what a button or interaction does (`Rules.md` R-8).

---

## 1. Design principles

1. **Dark glass.** Every panel and button is a dark slate translucent glass with a thin slate outline. Nothing is a flat opaque rectangle.
2. **Cyan means chosen or lit.** Neon cyan marks the selected card, the main action, the pressed flash, subtitles and highlights. It is never used as decoration.
3. **Buttons move.** On a stereo display a button that only changes colour reads as flat. Every button swells on hover, compresses and sinks on press, tints, flashes and sounds.
4. **Small and in the corners.** The interface is kept small and to the screen edges so it does not crowd the 3D model in the middle.
5. **Everything eases.** Fades, springs and tweens; nothing pops. Colours tween, panels fade, parts spring.
6. **One look across projects.** A visitor moving between projects must not be able to tell the projects apart by their interface.

---

## 2. Canvas and depth

| Item | Value |
|---|---|
| Canvas | world space, 1920 x 1080 reference, sized for the display by the SDK `UIScaler` (anatomy keeps it; the PCB project removed it so the UI never floats) |
| Sorting order | **100** on every interface canvas, so the pen's ray meets a button before a model collider that floats in front of it |
| Always on top | `UiAlwaysOnTop` on the canvas (render queue 4000, depth test off for graphics) so 3D parts never cover the UI |
| Raycasters | `GraphicRaycaster` + `KmaxUIRaycaster`; event system with `KmaxInputModule` |
| Layering | the screen fader is the last object on the canvas and covers everything |
| Depth rule | the UI sits at the screen plane; never let a panel intersect geometry (an interpenetrating panel does not fuse in stereo) |
| Callout canvas | a separate world-space canvas 7 cm toward the viewer (PCB `FrontCanvas`, 1920 x 1080 at scale 0.000157, z -0.07) so cards draw in front of pop-out parts |

---

## 3. Colour

### 3.1 Palette

| Name | Hex | Unity RGBA (0-1) | Use |
|---|---|---|---|
| Glass | `#0F172A` @ 82% | (0.059, 0.090, 0.165, 0.82) | fill of every panel, button and card |
| Glass outline | `#334155` @ 60% | (0.200, 0.255, 0.333, 0.60) | 1.5 px outline of every panel, button and card |
| Glass hover | `#0E3F4E` @ 92-92.2% | (0.055, 0.247, 0.306, 0.922) | tint of a normal button under the pointer |
| Card hover | `#106E86` @ 96% | (16, 110, 134, 245)/255 | tint of a launcher card under the pointer |
| Glass chosen | `#085668` @ 94% | (8, 86, 104, 240)/255 = (0.031, 0.337, 0.408, 0.94) | the chosen card, the Load button |
| Load hover | `#0E7C94` @ 98% | (14, 124, 148, 250)/255 | Load button under the pointer |
| Accent | `#06B6D4` | (6, 182, 212) | select bar on the chosen card, active highlights |
| Accent bright | `#22D3EE` | (0.133, 0.827, 0.933) | subtitles, headings of callouts, chosen outline, press flash |
| Heading | `#FFFFFF` | (1, 1, 1) | titles, button labels |
| Body | `#E2E8F0` | (0.886, 0.910, 0.941) | body text, card labels at rest |
| Muted | `#94A3B8` | (148, 163, 184) | secondary text, panel headings ("Parts") |
| Fact | `#FCD34D` | (252, 211, 77) | a fact set apart from the body |
| Backdrop (home / launcher) | `#252A36` | (0.145, 0.165, 0.210) | camera background while a home screen or the launcher is up |
| Backdrop (exhibit) | `#050810` | (0.02, 0.03, 0.05) | anatomy topic screens only. Every other experience, Automobile included since 2026-11-17, uses the home backdrop `#252A36` with the drifting dust (owner's rule); where a scene has post-processing the camera colour is set so the colour on screen is `#252A36` |

### 3.2 Pointer beam states

| State | Colour | When | HDR emission |
|---|---|---|---|
| Calm | `#06B6D4` | no button down (brighter on a target) | x1.5 |
| Select | `#10B981` | select button down on something selectable | x2.2 |
| Secondary | `#F59E0B` | reset (secondary) button down | x1.8 |
| Tertiary | `#A855F7` | third button down, or select held over nothing (zoom gesture) | x1.8 |

The multipliers stop where the brightest channel reaches 1 (`limitToDisplayRange`) because a display without bloom clamps each channel separately.

### 3.3 Other fixed colours

- Callout line: (0.55, 0.9, 1.0), base width 0.7 mm, tapering 0.6 -> 1.8, cubic Bezier.
- Held-part outline (PCB): (1.0, 0.93, 0.55).
- Placed / dimmed list chip: label and swatch at 35% alpha.
- Ghost highlight (PCB): cyan (0.3, 0.8, 1.0, 0.35) while carried, green once within reach.
- Dust motes: soft white-blue, additive; backdrop glow (very soft blue, additive) `#0A1424`-ish (0.025-0.04, 0.05-0.08, 0.085-0.14).

---

## 4. Typography

| Item | Value |
|---|---|
| Font | TextMeshPro **LiberationSans SDF** everywhere |
| Wrapping | normal for body text; buttons and cards use no-wrap with auto-size |
| Raycast target | off for every label, outline and thumbnail (only the button object takes a press) |

| Style | Size | Weight | Colour | Notes |
|---|---|---|---|---|
| Exhibit title | 36 | Bold | white | top left, (32, -22) |
| Exhibit subtitle | 18 | Regular | `#22D3EE` | 46 below the title |
| Home-screen title ("SELECT AN EXHIBIT") | 30 | Bold | white | letter spacing 6, centred, 22 from the top |
| Home-screen subtitle | 17 | Regular | `#22D3EE` | centred, 42 below the title top; "name  •  tagline" (two spaces round a bullet) |
| Button label | 19 | Bold | white | controls column; pill labels (navigation) 17 |
| Load / Home button label | 17 | Bold | white | auto-size 12-17 |
| Card label | 14 | Bold | `#E2E8F0` at rest, white when chosen | auto-size 10-14, one line |
| Panel heading ("Parts") | 15 | Bold | `#94A3B8` | |
| List chip label | 17 | Bold | white | |
| Callout / caption heading | 24 | Bold | `#22D3EE` | |
| Callout / caption body | 18 | Regular | `#E2E8F0` | |
| Caption fact / step text | 15 | Regular | fact `#FCD34D` / muted | |
| Marker badge number | 18 | Bold | white | |

---

## 5. Sprites

| Sprite | Spec |
|---|---|
| `GlassFill` | 64 px, 12 px rounded corners, 9-sliced with a **16 px** border, pixels per unit 100, no mipmaps; tinted by the Glass colour |
| `GlassBorder` | same size and slice, 1.5 px outline; tinted by the Glass outline colour |
| `SoftDot` | soft round white dot, used for dust, LED glow and the backdrop glow (additive) |

Images use `Image.Type.Sliced`. The outline is always a child named `Outline`, first sibling, stretched to fill, raycast off.

---

## 6. Spacing and layout

| Item | Value |
|---|---|
| Screen margin | **32** from the left, right and top edges (anatomy `Margin` 64 x 0.5) |
| Header (title) | top left at (32, -22); subtitle 46 below |
| Controls column | top right, anchored (1,1), at (-32, -32), **220** wide; buttons **220 x 46**, 9 between buttons, 8 between row items |
| Home button | top left, **150 x 42**, at (32, -22), label "Home" 17 bold |
| Uniform button size (kmax-display-example scenes) | **every** text button is **150 x 42**, left edge at x 32 (right edge 32 from the right, bottom edge 32 from the bottom), **9** between stacked buttons, label padded 10 on each side so the text auto-sizes with room; a top-left stack starts 9 under the Home button; captions sit 6 under a button; swatches stay 26 x 26; bottom centre: Previous / Next 9 from a 120 x 34 counter |
| Part / layer panel | left, **150** wide, top 145 (`LayerPanelTop` 290 x 0.5), heading 22 tall, chips **40 tall**, **7** gap, swatch 15, label 17 |
| Progress pill | bottom centre, 140 x 46, 32 from the bottom |
| Caption card | bottom centre, as wide and as tall as its text needs (never wider than 1040, at least 160 x 44), text centred, 24 above the bottom edge; step row 340 wide, 12 above the caption; main text column 640, fact column 340, inner margin 20 |
| Callout card | 480 wide, in the left or right column on the pad's side, level with its target (PCB) |
| Marker badge | slot 56 (hit area), disc 32, ring 36, spacing 52; hit area larger than the picture so it stays easy to point at |
| Home-screen cards | **172 x 132**, 12 apart, 5 per row, rows centred; picture 156 x 88 with an 8 margin at the top; label 26 tall at the bottom (4 up, 12 inset) |
| Select bar | 3 high along the top of the chosen card, inset 18 each side, `#06B6D4` |
| Load button | **300 x 42**, centred, 26 above the bottom edge; the card rows sit 14 above it |
| Home-screen preview | model turns in front of the glass, centre of the screen, 3 cm toward the viewer |

Alignment rules: columns hug the corners; the centre of the screen belongs to the model; a stack of buttons keeps the 9 spacing; nothing may overlap the Home button area (32..182 x 22..64 from the top left).

### 6.1 Exhibit screen layout (reference: `kmax-human-anatomy`, owner's rule)

Every exhibit screen is laid out the same way, whatever the project:

| Zone | What goes there |
|---|---|
| Top left | **Home** button (150 x 42), then the **title** directly **below the Home button** (36 bold white, left edge 32, top 72 from the top edge; at (32, 22) where there is no Home button) with a **very short description** under it (18 regular `#22D3EE`, 46 below the title top). The title names the screen; the description is one short line. |
| Top right | The **controls column**: every action button, 220 x 46, stacked downwards from 32 below the top edge, 9 apart, right edge 32 from the screen edge, label bold white centred (no icons on text buttons). Screen-change buttons (Menu, Next exhibit, Build Mode, Explore Mode) are part of this column. |
| Left | Part / layer chips (section 7.5) under a "Show" style muted heading, below the title. An info panel opens here, under the title (top 160), never in the controls column. |
| Centre | The model, and nothing else. |
| Bottom centre | **Control instructions and captions always live here**, in one glass caption card: 1040 wide, 24 above the bottom edge, text left-aligned, inset 18, 18 regular Muted `#94A3B8`. The card is 124 tall for a multi-line caption and **70 tall for a one-line control hint**. Previous / Next, progress pills, toasts and step rows sit centred directly above the card, 12 apart. |

Never put control instructions in a corner, over the model or in the controls column. A scene that offers several lines of help puts them in the same bottom-centre card.

### 6.2 Navigation, stepping controls and backdrop motion (owner's rule, from the engine screens)

- **Menu (top of the controls column):** the first button of the top-right column, labelled "Menu", opens the topic's menu screen (the engines open the Engineering screen; a scene with no topic screen above it opens the launcher, `KmaxLauncherScene`). It is the screen's Home / Back button, so there is **no separate Back or Home button** on a screen that has Menu (Rules.md R-7 is met through Menu and the topic screen's own Home button). A topic's own home screen keeps its Home button top left.
- **Title and description:** top left at the corner, (32, 22), title 36 bold white, one-line description 18 `#22D3EE` 46 below.
- **Controls column order, top to bottom:** Menu, the screen's own actions (for example See inside), a muted 15 bold group heading where a group of choices follows (for example BUILD, then Stock, Sport, Throttle bodies, Turbo), then the screen's other actions and Reset view. All 220 x 46, 9 apart. Nothing sits bottom-left or bottom-right.
- **Previous / Next (anatomy buttons, every experience that steps through parts):** text glass buttons 163 x 46, labels "Previous" and "Next" (bold, up to 19), 11 apart and centred, 12 above the caption card; the counter (20 bold `#E2E8F0`) sits centred above them.
- **Caption card (information panel):** always bottom centre, **as wide and as tall as its text needs** (24 padding left and right, 14 top and bottom, never wider than 1040, at least 160 x 44, growing upwards from 24 above the bottom, text centred), so a short hint is a small card and a long description a larger one; it is visible even when idle and holds the control instruction or idle prompt (18 `#94A3B8`, left aligned, inset 18). A part's name (24 bold `#22D3EE`) and description (18 `#E2E8F0`) replace the prompt in the same card. Where a screen keeps a one-line control hint card as well (Pontiac screens, 70 tall), the information card sits directly above it, 12 apart.
- **Backdrop particles never rotate with the model or the view.** Where a screen orbits the camera rig around the model, the ambient motes are children of the rig, in local simulation space, so they hold still on screen; they are never left in the world where the orbit would sweep them round. Interaction effects (bursts, rings, sparks) stay in the world at the selected point.

---

## 7. Components

### 7.1 Glass button (every button)

- Object `Button` with `Image` (GlassFill, sliced, Glass colour), `Button` (**transition None**), `UiButtonMotion`, `UiButtonSound`.
- Children: `Outline` (GlassBorder, Glass outline colour) and `Label` (TextMeshPro).
- `UiButtonMotion` and the `Button` stay on the **same object**; the input module only clicks the object that took the press. Put the button inside a slot object that a layout positions; never move the motion to an inner visual (it fights layout groups).
- Rest fill: Glass. Hover tint: Glass hover. Chosen / main action: Glass chosen, outline `#22D3EE`.

### 7.2 Launcher card

Glass button of 172 x 132 with a picture on top (156 x 88), a name below, a `SelectBar` (hidden unless chosen). Chosen: fill Glass chosen, outline `#22D3EE`, label white, select bar shown. Rest: fill Glass, outline Glass outline, label Body. Hover tint: Card hover.

### 7.3 Load button

300 x 42, fill Glass chosen (0.031, 0.337, 0.408, 0.94), outline `#22D3EE`, hover tint Load hover, label "Load <name>" 17 bold white. In the reference the Load button also **breathes when idle** (idle pulse on, amplitude 0.02, speed 1.4).

### 7.4 Home button

150 x 42 glass button, top left, label "Home", opens the launcher scene `KmaxLauncherScene`; hidden when the launcher is not in the build. Mandatory on every home screen (`Rules.md` R-7).

### 7.5 List chip (parts / layers)

150 wide, 40 tall, glass; coloured swatch 15 at the left, label 17 bold white; placed or hidden state dims to 35% alpha; colour changes tween over 0.3 s.

### 7.6 Callout and caption cards

Glass panel; heading 24 bold `#22D3EE`, body 18 `#E2E8F0`; a thin tapered Bezier line joins a callout to its target; panels fade, they never pop.

### 7.7 Marker badge

Numbered disc (32) with a ring (36) in a 56 hit area; number 18 bold white.

---

### 7.8 Dimension gizmo (retail showroom)

Three thin measuring lines (width along the top front edge, height up the right front edge, depth along the bottom right edge) drawn round a model, each with a short end tick and a pill label at its middle; they are children of the model, so they turn and scale with it.

- Line: callout colour (0.55, 0.9, 1.0), width **0.7 mm**, end ticks 5 mm; drawn on top of the model (ZTest Always).
- Label: LiberationSans SDF **bold**, Accent bright `#22D3EE`, text such as W 7'0", H 2'8", D 7'3" (feet and inches, nearest inch), always facing the viewer.
- Pill behind the label: fully rounded (capsule) Glass fill (`#0F172A`, 92%) with a Glass outline (`#334155` @ 60%); sized to the text (padding 8 mm across, 3.6 mm up) and placed beside its line on the side away from the model, never on the line.
- Shown or hidden with the **Dimensions** button in the controls column; on by default.

---

## 8. Motion and animation

### 8.1 Button feel (`UiButtonMotion`, spring `UiSpring`)

| Parameter | Value |
|---|---|
| Hover scale | **1.04** |
| Press scale | **0.94** |
| Press depth | sinks **2 mm** into the screen while pressed |
| Hover lift | 0 |
| Spring frequency | **18** rad/s |
| Spring damping | **1.05** (the reference scene; its usage guide says 0.7, the scene value is the one in use) |
| Hover tint | Glass hover (or the card / Load tint), blend **0.1 s** |
| Press flash | on, colour `#22D3EE`, decay **0.26 s** |
| Idle pulse | off, except the Load button (amplitude 0.02, speed 1.4) |
| Unity transition | None (the motion owns all feedback) |

Projects whose button script has no spring (for example kmax-display-example, scale smoothing 0.07 s) may use that script's equivalent values until the script itself is aligned.

### 8.2 Screens and flow

| Item | Value |
|---|---|
| Screen change | fade through black: out **0.25 s**, in **0.45 s** (reference `ScreenFader`) |
| Attract mode | after **45 s** idle the home screen starts showing its own cards, one every **7 s** |
| Return to home | a topic left alone for **120 s** gives way to the home screen |
| Any visitor input | ends attract mode and restarts the idle clock |
| PCB home screen | cards auto-advance every **6 s**, paused **20 s** after a press |

### 8.3 Turntable preview (home screen)

| Item | Value |
|---|---|
| Tilt toward the viewer | 42 degrees |
| Spin | 18 degrees per second |
| Bob | 3 mm, 1.8 rad/s |
| Choose another card | the old model shrinks away in 0.18 s while the new one springs up from 60% (frequency 16, damping 0.55) |
| Fit | every model scaled so its largest reach (3D) is 0.065 m |
| Rest position | (0, 0.012, -0.03) in front of the glass |

### 8.4 Other tweens (DOTween in the PCB project)

Part moves 0.35-0.7 s (InOutSine / OutCubic / OutBack), seat punch 0.3 s, grab scale 0.25 s OutBack, outline fade in 0.2 s / out 0.35 s, fade of everything else to 0.35-0.8 opacity in 0.3 s, ghost fade in 0.25 s / out 0.35 s, callout line drawn over 0.35 s OutCubic.

### 8.5 Particles and glow

Dust motes: life 16-26 s, size 1.2-2.8 mm, 7 per second, up to 140, noise 0.004, alpha 0 -> 1 -> 1 -> 0, additive soft dot. LED glow: soft additive camera-facing quad, 0.03 m wide at full brightness, opacity 0.55, grows and fades with the LED level. Backdrop glow: one very soft additive quad about 0.25 m behind the model.

---

## 9. Sound

All sounds are synthesised at run time (no audio assets, no licence question).

| Cue | Spec |
|---|---|
| Hover | short soft tick: about 15-20 ms blip around 900 Hz, played at 30% of the UI volume |
| Click / press | short two-tone drop, 35-85 ms around 587 Hz (D5), played at 60% of the UI volume |
| UI volume | 0.7 |
| Sweeps | rising and falling 120 ms frequency-modulated sweeps for transitions |
| Bell | soft E5 and B5 for a chosen card |
| Ambient bed | seamless loop: an A drone, a pad through four chords of the A minor pentatonic scale, a sparse echoing melody, 66 beats a minute |
| Director | one persistent audio director (`PersistentAudioDirector`) survives scene loads and removes a second copy |

Every button gets hover and click sound through `UiButtonSound`. Placement and error cues in the PCB project use stylus haptics too (impact on place, error on reject).

---

## 10. Pointer visuals

The stylus ray is replaced by the coloured beam (section 3.2): a thin line ending in a bead; contact brightens it. The pen pulses on contact, at most about once every 0.25 s. The mouse equivalent keeps the same hover and press feedback on buttons.

---

## 11. Scene look (3D)

| Item | Value |
|---|---|
| Render pipeline | URP, Gamma colour space, soft shadows on, 4x MSAA (launcher asset `KMaxLauncherURP`) |
| Key light | warm (1.0, 0.94, 0.84), intensity 1.4, soft shadows (exhibits only) |
| Fill light | cool (0.62, 0.74, 1.0), intensity 0.4, no shadows |
| Rim light | cool (0.55, 0.72, 1.0), intensity 1.2, no shadows |
| Ambient | tri-light: sky (0.34, 0.40, 0.52), equator (0.22, 0.25, 0.31), ground (0.10, 0.12, 0.11) |
| Reflections | the generated studio cubemap (dark room, warm soft box at the key, cool ones for rim and fill) at intensity 1; no skybox |
| Camera far plane | 3 m, so the shadow map covers the scene |
| Shadows | none on the launcher's turntable models; on exhibits, parts shadow the board |
| Models | opaque PBR with the studio reflection; transparency only as glass with an edge glow (a flat translucent pane does not fuse in stereo) |
| Materials | every material is URP (`Rules.md` R-1) |

---

## 12. Home screen composition

Top to bottom: **Home** button top left; title and subtitle centred at the top; the chosen project's model turning in the middle; rows of cards (5 per row, centred, last row hugging the bottom); **Load** button at the bottom. The same composition applies to the launcher and to every project's own home screen (`Rules.md` R-5).

---

## 12a. Loading screen (`Rules.md` R-9)

One screen for the whole application, built by `KMax/Launcher/Build Loading Screen` into `Resources/KMaxLoadingScreen.prefab`:

- Screen Space Overlay canvas, sorting order 30000, scaler 1920 x 1080 match 0.5, covers everything.
- Backdrop: `#252A36` (the home screen backdrop), opaque.
- Spinner: Animated Loading Icons "Circle open rotating 1", 120 x 120, tinted accent bright `#22D3EE`, 40 above centre.
- Text: "Loading..." 30 bold white, letter spacing 2, 64 below centre.
- Motion: fade in 0.25 s, load, hold at least 0.6 s, fade out 0.45 s (smooth-step, unscaled time). Raycasts blocked while it is up.
- Colours of swatches that show a product's real paint or trim are data, not UI colour, and are exempt from section 3.

---

## 13. Checklist before finishing UI work

- [ ] Every button is a glass button (section 7.1) with outline, label, motion, sound and no Unity transition.
- [ ] Colours come from section 3; no new colour without adding it here.
- [ ] Font LiberationSans SDF, sizes from section 4.
- [ ] Positions and sizes from section 6; nothing overlaps the Home button.
- [ ] Back, Menu, Previous / Next, the information panel and the backdrop particles follow section 6.2.
- [ ] Layout follows section 6.1: title and short description top left, controls column top right, control instructions in the bottom-centre caption card.
- [ ] Canvas sorting order 100, always-on-top, raycasters present.
- [ ] Animations from section 8; sounds from section 9.
- [ ] Behaviour untouched in newly added projects (`Rules.md` R-8).
- [ ] Differences from this file are reported to the user with a warning and a suggested change (`Rules.md` R-4).

## 14. Known differences and open items

- The reference's usage guide says spring damping 0.7; the reference scene uses 1.05 (this file uses the scene).
- Button sizes: anatomy's controls are 220 x 46 and the Home button is 150 x 42; the kmax-display-example scenes use 150 x 42 for every button. Pick one standard size (the owner's call) and align the others.
- kmax-display-example buttons have no spring or press-sink (script limitation); title size 22 and no subtitle; info panel and badge styling not yet matched.
- Volvo screen (`VolvoS90`): bottom elements (caption card, Previous / Next) can be hidden by the floor or the car when the tour pitches the camera, because the world-space UI is depth-tested; needs a real ZTest-Always UI shader (and its inclusion in the build) before the bottom layout is reliable there.
- kmax-automobile-engine: UI authored for a 2824 x 1588 canvas (UI scale 0.68), so sizes there are the Design values times 1.47; no click or hover sounds on its buttons; camera background (9, 10, 12) instead of `#050810`; no list chips; its builder script (`EngineSceneBuilder`) still describes the old layout, the scenes were edited in place.
- PCB: single row of cards on the home screen; no fade between screens; no idle return to the home screen; stylus button mapping and beam differ from anatomy (see the Rules.md check log).
