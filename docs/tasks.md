# Tasks

## Done
- Launcher scene built from the anatomy-style layout (title, subtitle, turntable model, card grid, Load pill).
- Two projects: PCB Assembly (opens `KMaxPCBAssembly/Scenes/Main.unity`, its single scene) and Human Anatomy (opens `kmax-human-anatomy/Scene/Main.unity`).
  Both Load clicks verified in play mode (scripted click), console clean.
- Builder follows the reorganised layout (projects as siblings of this repo); stale `Kmax_projects` paths removed.
- Game Info So: URP + anatomy URP asset, Gamma, 8 scenes.
- PCB project converted to URP at the user's request (no pink in launcher preview or 00_BlinkLED).
- Preview fit uses the full 3D reach, so tall models (anatomy) fit above the cards.

## Next up
- Real mouse and KMAX stylus test of card and Load clicks (only scripted clicks so far).
- Back-to-launcher path from each project (change in each project's repo, see L-005).
- Decide `applicationType` and an app icon in Game Info So.

- 2026-11-16: launcher URP asset with soft shadows applied (L-007); PCB LED blink fixed in the PCB repo.

- 2026-11-16: Home buttons in the PCB and anatomy home screens open the launcher (verified with scripted clicks both ways); launcher lighting and shadows match PCB (L-008).

- 2026-11-16: launcher shadows removed, studio lighting and backdrop glow added (L-010); `Rules.md` created (empty, the owner adds the rules).

- 2026-11-16: cards for the three kmax-display-example scenes (verified: each opens its scene); cards renamed by industry (L-011). Open: the Next Scene button inside that project cycles by build index and will now step into the launcher and other scenes; no Home button yet in its scenes.

- 2026-11-16: FMax app icon set in Game Info So (L-013). Not done: a real build to see the icon on the executable.
- 2026-11-16: single loading screen (L-014, R-9) done and verified in play mode (launcher to scene and back). PCB unlit (L-015), Next Scene hidden, palette audit done. Not done: stereo-hardware check of the overlay loading screen; eyeball of PCB LED blink after unlit; real build.
- 2026-11-16: kmax-automobile-engine registered, Home buttons and layout done (L-016). Open: sounds and stylus mapping for that project, bring the other projects' scenes to the 6.1 layout (display-example controls are bottom/side, PCB differs), real build.
- 2026-11-17: Engineering home screen with three engines done (L-017), verified in play mode. Open: engine scenes' Home returns to the launcher, not to the Engineering screen (owner's call); a Back button on topic screens; Esc on the topic screen exits the app like the launcher (DoubleTapEscExit).
- 2026-11-17: Medical card hidden (L-019); slate backdrop and dust on the engine scenes, slate on i4, Eye and PCB. Open: anatomy topic screens keep their dark backdrop (reference project, not touched); Volvo untouched by request; PCB look on slate not reviewed by the owner.
- 2026-11-17: Inline-Four screen reworked to R-10 (L-020). Open: apply R-10 to `EyeAnatomy` (same orbit, particles) and the two Pontiac scenes (Back / Menu, caption card vs hint card), Volvo on hold.
- 2026-11-17: engine screens finished to R-10 / L-021 (Menu only, anatomy Previous / Next, caption cards). Open: Eye and Volvo not converted; Pontiac screens have no Previous / Next (none in the scene); a second button next to Menu like anatomy's "Next exhibit" is not offered by the engines.
- 2026-11-17: Volvo screen laid out to R-10 (L-022). Open: decide how to fix the bottom-UI occlusion in the tour (needs a shader and build inclusion), Eye screen still on the old layout.
- 2026-11-17: Volvo ground plate hidden, shared backdrop added (L-024).

- [x] Automobile home screen with three cars, thumbnails, Menu wiring (2026-11-17)
- [ ] Play-test C40 doors, sunroof, tour on the headset; confirm Mustang model name with the asset source

- [x] Retail card and showroom (2026-11-17)
- [ ] Owner review of the retail showroom: names for finishes, TV unit models, orientation of each design

## 2026-11-19
- [x] One Back button at the bottom left of every screen; top-right Menu removed, columns moved up.
- [x] Load button above the cards, fit to its text.
- [x] Explore more button (engineering, automobile, retail, education).
- [x] Stylus tutorial (i) button and panel on every screen.
- [x] Every pen button clicks interface buttons (SDK `StateOf` patch).
- [ ] Try the pen, the tutorial and Explore more on the display; read the Player log for the Education wrap.
- [ ] Builders of the cars, Retail, PCB, anatomy and engines still place the old layout: run the three menu tools after rebuilding a scene.
