# AI Handoff

## 2026-11-19 Back, Load, Explore more, stylus tutorial (latest)
What changed (all checked in the editor; the pen and the display were tried by the owner only on build 1.0.0-9, before the later items):
- **Back button.** The Menu button of the exhibits and the Home button of the home screens are one 'Back' button, 150 x 42, label 17, bottom left (32 from the left and bottom, scaled on wider canvases). `Scripts/Editor/BackButtonLayout.cs` (menu `KMax/Launcher/Apply Back Button And Load Layout`) places it in every scene, moves the top-right column up into the old Menu spot, renames the next-experience button, hides the Inline-Four `BuildCaption` and the Pontiac bottom `CaptionCard`. Run it after rebuilding a scene. `LauncherSceneBuilder` builds the new layout itself (Home slot bottom left, label 'Back').
- **Load button** sits between the model and the card rows (first child of the `Cards` stack) and is as wide as its label (`BackButtonLayout.FitLoadWidth`: host fits itself and is centred in its slot).
- **Explore more** (was 'Try another', before that 'Next exhibit'): top of the right column of every exhibit of engineering, automobile, retail and education; electronics and biology have their own. `kmax-display-example` has `TryAnotherButton` and the installer `TryAnotherInstaller` (`KMax/Exhibits/Add Try Another Button`).
- **Stylus tutorial.** `Scripts/Ui/StylusTutorial.cs`, `StylusTutorialPanel.cs` and `Scripts/Editor/StylusTutorialBuilder.cs` (menu `KMax/Tutorial/Add Stylus Tutorial To All Screens`, rebuilds the tutorial of every screen: 30 canvases). A round (i) at the bottom right opens a panel (tap toggles, closes after 8 s unless the pointer is on it) with the picture `UI/Tutorial/StylusController.png`: 0 top round button = zoom and click, 1 lower left = click, drag, move, 2 lower right = reset the view and click. Home screens say what each does there (no pen rotate or zoom on the turntable previews, by R-14; owner chose to leave it).
- **Pen standard** (Rules.md, last entries): SDK button index 0 'Left' = select, drag, wrist turn; index 1 'Right' = reset tap; index 2 'Center' = zoom hold. Every button now clicks the interface: `KmaxStylus.StateOf` in `Assets/Plugins/KmaxVxSDK/KMAX_Core/com.kmax.xr.core` was patched (an SDK update overwrites it; the AIO K1 copy is not patched). A press that starts on the interface no longer resets or zooms.
Modified files (this repo): `Rules.md` (a dated entry per change), `Scenes/*.unity`, `Scripts/Editor/{BackButtonLayout,StylusTutorialBuilder,LauncherSceneBuilder}.cs`, `Scripts/Ui/{StylusTutorial,StylusTutorialPanel}.cs`, `UI/Tutorial/*`, `docs/*`.
Changed in other repositories (not committed with this one): `kmax-display-example` (TryAnotherButton, TryAnotherInstaller, ViewerFlyController home focal centre, AnatomyStylusInput and StylusViewControl UI-press guard, KmaxTooltip.PointerIsOn, EyeAnatomyController tooltip gate, builders), `kmax-human-anatomy` (tour auto-advance 6 s, hover tooltip gate, 'Explore more' wraps to the first card, StylusNavigation guard), `kmax-automobile-engine` (EngineExperience and AssemblyGame tooltip gate), the SDK file above.
Next recommended task: play-test on the display (pen clicks with all three buttons, tutorial, Explore more wrap in Education; the owner reported the last Education lesson not wrapping on build 1.0.0-9, not reproduced in the editor, `TryAnotherButton` now logs the scenes it opens: read the Player log).

Read `../Rules.md` (global rules for the launcher and every project: check them before and after every change in any repository) and `../../AGENTS.md` (code rules, parent `Assets/Games`), then `architecture.md`, `decisions.md`, `tasks.md`.

## State (2026-11-16)
- Writable: this repo only (`Assets/Games/KMax-launcher-unity`) and `Packages/`. The Unity project around it is read-only. Projects live beside it
  in `Assets/Games/` (`KMaxPCBAssembly`, `kmax-human-anatomy`); each is its own git repo, do not edit them from here.
- Unity MCP: stdio server `unityMCP`; pin the instance `KMax-launcher-unity-base@...` with `set_active_instance`.
- Editor runs URP + Gamma (anatomy URP asset). Launcher scene works: both cards, subtitles and turntable models show; Load opens PCB `Main`
  (one scene with its own home screen) and anatomy `Main`. Console clean.
- `Game Info So.asset`: gameID KMaxLauncher, repo vc-chitrang/KMax-launcher-unity, "KMax Launcher" 1.0.0 (1), Gamma, URP, 8 scenes.
- PCB project was converted to URP (2026-11-16); nothing pink in the launcher preview or 00_BlinkLED.

## Files (this session)
Scripts/{KMaxLauncher.asmdef, Launcher/*.cs, Ui/*.cs, Editor/KMaxLauncher.Editor.asmdef, Editor/LauncherSceneBuilder.cs},
Data/LauncherCatalog.asset, UI/Sprites/*, UI/Thumbnails/*, Materials/LauncherDust.mat, Scenes/KmaxLauncherScene.unity, Game Info So.asset, docs/*.

## Next recommended task
Real mouse/stylus test, then a back-to-launcher button per project.
Gotchas: the builder must reload assets (e.g. the catalog) after opening a scene; Bash heredocs with quotes can break, write scripts with the Write tool.

## 2026-11-16 (latest)
Game Info So now uses `URP/KMaxLauncherURP.asset` (soft shadows, 4096, 4x MSAA). The editor was switched to it with `KMax/Launcher/Apply Render Pipeline To Editor`, which edits the base project's
ProjectSettings (graphics default and current quality level); re-apply the anatomy Game Info to switch back.

## Home buttons, launcher lighting (2026-11-16, latest)
PCB `Screen_Home/HudCanvas/HomeSlot/Home` and anatomy `Interface/Launcher/HomeSlot/Home` go back to `KmaxLauncherScene`. Launcher scene rebuilt with `ApplyLighting` and `BuildShadowBackdrop` (decisions L-008, L-009).
Changed outside this repo: PCB `Scripts/Game/Launcher/{SceneLinkButton, ExhibitNavigator}.cs`, `Scenes/Main.unity`; anatomy `Scripts/Anatomy/Runtime/Launcher/LauncherHomeButton.cs` (new), `Scene/Main.unity`.

## Rules.md, studio lighting (2026-11-16, latest)
`Rules.md` (launcher root) holds the global rules; it has none yet. Check it before and after every change in any repo under `Assets/Games/` and add a line to its check log. Launcher scene: no shadows, studio lighting, backdrop glow (L-010).

## kmax-display-example, industry names (2026-11-16, latest)
Launcher has five cards: Electronics, Biology, Medical, Engineering, Automotive (decisions L-011). `Rules.md` R-2 now says `Main` is recommended, other names accepted. The launcher URP asset needs the probe atlas on.

## Rules R-7, R-8 and the example project audit (2026-11-16)
R-7 (Home button on every home screen) and R-8 (newly added projects: change looks, never behaviour) are in `Rules.md`; the three display-example scenes got Home buttons (kmax-display-example repo) and were audited for R-1 and R-3 (see the Rules.md check log).

## Display-example button restyle (2026-11-16, latest)
Pill buttons of the three kmax-display-example scenes restyled to the anatomy glass look (visuals only). Details in that repo's `docs/ai_handoff.md` and the Rules.md check log.

## Design.md (2026-11-16, latest)
`docs/Design.md` holds the detailed design rules (palette, typography, spacing, components, motion, sound, scene look). Rules.md says to follow it for any UI/UX, animation or visual work. Numbers come from the kmax-human-anatomy builders and scene.

## Card order, engine preview, uniform example buttons (2026-11-16, latest)
See decisions L-012 and the Rules.md check log. kmax-display-example scenes: every text button 150 x 42 aligned with the Home button (scene data only).

## App icon (2026-11-16, latest)
`properties.icon` of `Game Info So.asset` is `UI/Icons/FMaxAppIcon.png` (L-013); applied at the next build by the base pipeline.

## Update 2026-11-16 (loading screen, PCB unlit, palette)
Modified: `Scripts/Loading/*`, `Scripts/Editor/LoadingScreenBuilder.cs`, `Resources/KMaxLoadingScreen.prefab`, `LauncherController`, Rules.md (R-9), Design.md (12a). Other repos: `SceneLoader` helpers and callers; PCB `PcbUnlitConverter`, LED scripts, unlit materials; display-example scenes (Next Scene hidden, palette snap). Next recommended: check the loading screen and PCB on KMAX stereo hardware; decide the button-size standard (220x46 vs 150x42).

## Update 2026-11-17 (Engineering home screen)
`KMax/Launcher/Build Everything` now also builds `Scenes/EngineeringHomeScene.unity` (catalog `Data/EngineeringCatalog.asset`); new script `Scripts/Launcher/LauncherHomeButton.cs`. Main launcher has 5 cards; Engineering opens the sub screen with Inline-Four Engine, Pontiac V8 Assembly and Pontiac V8 Experience. Build Settings and Game Info list 13 scenes.

## 2026-11-17 Automobile screen
Modified: `Scripts/Editor/LauncherSceneBuilder.cs` (Automobile screen, cards), `Scenes/AutomobileHomeScene.unity`, `Data/AutomobileCatalog.asset`, Build Settings and Game Info (14 scenes). Run `KMax/Launcher/Build Everything` after any catalog change. Next: headset test of both new cars; if the Mustang name is corrected, edit `CarSpecs.cs` and the builder table, then rebuild.

## 2026-11-17 Retail
Modified: `Scripts/Editor/LauncherSceneBuilder.cs` (Retail card, `RetailScenePath`). Run `KMax/Launcher/Build Everything` after changing the retail scene path or thumbnail.
