# Architecture

## What this repo is
The master launcher of the KMax display. `Scenes/KmaxLauncherScene.unity` lists every project placed under
`Assets/Games/` (one sibling folder per project) and opens the chosen project's scene. Each project is its own folder (and its own git repo) and owns its scenes.

## Launcher (this repo root: `Scenes/`, `Scripts/`, `Data/`, `UI/`, `Materials/`)
- `Scripts/` asmdef `KMaxLauncher` (namespace `ViitorCloud.KMaxLauncher`), `Scripts/Editor/` asmdef `KMaxLauncher.Editor`.
- `LauncherCatalog` (ScriptableObject, `Data/LauncherCatalog.asset`): title, Load button wording and one `LauncherProjectEntry`
  per project: label, subtitle, `scenePath` (asset path of the scene to open), preview prefab, card thumbnail.
- `LauncherController`: binds the catalog to a pool of 10 `LauncherCard`s (2 rows x 5; unused cards and empty rows are hidden,
  rows stay centred), shows the chosen project's subtitle, Load label and turntable model, auto-advances every 6 s (pauses 20 s
  after a press), loads `SceneManager.LoadScene(scenePath)`. Scene must be in the build settings (checked, logged).
- `LauncherPreview`: turntable (tilt 42, spin 18 deg/s, bob, spring pop) from the anatomy exhibit.
- `LauncherAudio`: synthesised hover/click cues from `UiFeedback`. `DoubleTapEscExit`: Esc twice quits.
- `Scripts/Ui/`: UiButtonMotion, UiSpring, UiButtonSound, UiFeedback, UiAlwaysOnTop vendored from the PCB project's `KmaxVx`
  (itself from the SDK) into namespace `ViitorCloud.KMaxLauncher.Ui`, so the launcher compiles without any project.
- Look: kmax-human-anatomy / PCB Home numbers (1920 reference, glass #0F172A@82% + #334155@60% outline, title 30 bold spacing 6,
  subtitle 17 #22D3EE, cards 172x132, Load 300x42).

## Scene generation (never hand-edit the scene)
`KMax/Launcher/Build Launcher Scene` (`LauncherSceneBuilder`): reopens the existing launcher scene as the base (stereo `XRRig`, lights,
`EventSystem` + `KmaxInputModule`, `DustMotes`); it stops with an error if the scene or its rig is missing (no template copy any more). Then it
removes every component from a `KMaxPCBAssembly*` assembly, swaps `UiAlwaysOnTop` for the launcher's copy, replaces the dust material with
`Materials/LauncherDust.mat` (URP additive particle), rebuilds header, cards and Load button and wires `LauncherController`.
It also copies the glass and dust sprites to `UI/Sprites/` and keeps `Data/LauncherCatalog.asset` in step with the `DefaultProjects`
table in the builder: missing projects are added, a missing scene path is repaired, card pictures are copied to `UI/Thumbnails/`.
`KMax/Launcher/Sync Build Settings`: launcher first, then every scene in each sibling folder of `Assets/Games/` (a project's `Main.unity` first).
`KMax/Launcher/Update Game Info`: writes `Game Info So.asset` (id, repo, title, version, Gamma colour space, URP + the anatomy URP asset, scene list).
`KMax/Launcher/Build Everything` runs all three.

## Projects in the launcher
| Card | Scene opened | Preview model | Notes |
|---|---|---|---|
| PCB Assembly | `Assets/Games/KMaxPCBAssembly/Scenes/Main.unity` | `Exhibits/Exhibit_BlinkLED.prefab` | Converted to URP; one scene with its own home screen |
| Human Anatomy | `Assets/Games/kmax-human-anatomy/Scene/Main.unity` | `Generated/Resources/Anatomy/body.prefab` | URP project; `Generated/` is git-ignored there |

## Adding a project
1. Put the project folder next to this one under `Assets/Games/`.
2. Add a `ProjectDefaults` entry in `LauncherSceneBuilder` (or an entry in `Data/LauncherCatalog.asset` by hand: scene path, preview prefab, thumbnail).
3. Run `KMax/Launcher/Build Everything`. Up to 10 projects fit; raise `RowCount` in the builder for more.
