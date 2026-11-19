# Decisions

## L-001 Self-contained launcher assembly
The launcher has its own asmdef and vendored UI scripts (new namespace) instead of referencing `KMaxPCBAssembly.KmaxVx`.
Why: removing or adding a project must not break the launcher's compile. Cost: five small files duplicated.

## L-002 Catalog entries load scenes by asset path
`LauncherProjectEntry.scenePath` is used with `SceneManager.LoadScene` and `SceneUtility.GetBuildIndexByScenePath`.
Why: projects can share scene names. Moving a project changes its path: re-run `KMax/Launcher/Build Everything` (it repairs missing paths).

## L-003 Scene built from the existing launcher scene
The builder reuses the launcher scene's stereo rig; there is no template copy from a project any more (the PCB Home scene was merged into its `Main`). Why: no dependency on a project.

## L-004 URP and Gamma (user decision 2026-11-16)
The combined launcher uses URP with the anatomy URP asset and Gamma colour space (Game Info So: `urp 1`, colour space 0), as the editor already runs.
Why: a build has one render pipeline; the user chose URP and to convert PCB later. Consequence: PCB's Built-in materials were pink under URP; the PCB project was converted to URP on 2026-11-16 (see its decisions D-URP). The launcher's own dust uses a URP particle shader. `applicationType` left at Game.

## L-005 Projects are not edited
Projects are separate repos. The launcher does not modify them, so a project has no Back-to-launcher button yet.

## L-006 Launcher owns its card pictures
Thumbnails are copied into `UI/Thumbnails/`. Why: the anatomy `Generated/` folder is git-ignored and rebuilt. The preview prefab still references the project.

## L-007 Launcher-owned URP asset (2026-11-16)
`URP/KMaxLauncherURP.asset` (+ renderer) is a copy of the anatomy project's URP asset with soft shadows (High), a 4096 shadow map and 4x MSAA, made by the builder
(`KMax/Launcher/Ensure Render Pipeline Asset`). It is the `renderingPipelineAsset` of `Game Info So.asset`, so a launcher build uses it for every project. Why: PCB shadows were hard because the anatomy
asset has soft shadows off, and the reference KMaxDisplayBase ran soft shadows. The anatomy project itself is untouched; its own Game Info still points at its own asset.
Side effect: anatomy also gets soft shadows and 4x MSAA when it runs inside the launcher (checked: it looks the same). `KMax/Launcher/Apply Render Pipeline To Editor` assigns the asset to the editor's graphics and quality
settings, as the base project's Packages window does (this changes ProjectSettings of the base project).

## L-008 Launcher lighting and shadow backdrop (2026-11-16)
The builder now applies the PCB exhibits' lighting to the launcher scene (`ApplyLighting`): warm key light with soft shadows (shadow strength 0.85, bias 0.02, normal bias 0.5), cool fill and rim,
tri-light ambient, a procedural sky (`Materials/LauncherStudioSky.mat`) for reflections only, and a 3 m camera far plane so the single shadow map covers the scene (with the default 100 m far plane
URP's 50 m shadow distance left shadows invisible). `BuildShadowBackdrop` adds a transparent quad 0.02 m behind the turntable (`Shaders/ShadowCatcher.shader`, `Materials/LauncherShadowCatcher.mat`,
strength 0.5) that draws only the shadow the model throws on it, so the model has depth against the flat backdrop. The preview models also shadow themselves (the PCB's capacitor and transistor shadow the board).

## L-009 Home button in each project (2026-11-16)
Each project's home screen has a Home button at the top left (150 x 42 at 32, -22, the same glass style) that opens the scene named `KmaxLauncherScene` (build settings; the launcher is index 0).
PCB: `SceneLinkButton` (screen of Main if the navigator has one with that name, otherwise a scene) with `hideWhenUnavailable`. Anatomy: new `LauncherHomeButton`. Both hide the button when the scene is
not in the build, so a project still runs alone. This edits the two project repos (their scenes and one script each).

## L-010 Launcher without shadows, with studio lighting (2026-11-16) - replaces the shadow part of L-008
No shadows on the launcher's 3D models (the shadow backdrop looked like it floated, so it and `ShadowCatcher.shader` were removed; the key light has shadows off). The builder's `ApplyLighting` uses the PCB studio lights
(key x1.4, fill x0.4, rim x1.2), the tri-light ambient and the studio reflection cubemap (copied from the PCB project to `Materials/LauncherStudioReflection.asset`; run the PCB menu `Apply Studio Lighting` first if it is missing),
and `BuildBackdropGlow` puts a very soft blue glow (`Shaders/SoftGlow.shader`, `Materials/LauncherBackdropGlow.mat`) 0.25 m behind the turntable. The builder deletes a leftover `ShadowBackdrop` object (an old one without its shader drew the whole backdrop magenta).

## L-011 Five launcher cards named by industry; kmax-display-example added (2026-11-16)
Cards are named after the industry of the project, set by the owner: Electronics (PCB Assembly), Biology (Human Anatomy), and for the new kmax-display-example scenes: Medical (`EyeAnatomy`), Engineering (`VirtualExhibition WR`, the i4 engine)
and Automotive (`VolvoS90`). The last three names are my choice from the same rule; change the `ProjectDefaults` labels in `LauncherSceneBuilder` (and the catalog) to rename.
Each opens its scene by path. The turntable models are the project's own (`EyeAnatomy.glb`, `Enginei4.FBX`, `Volvo S90.fbx`), the pictures are copied from its `Materials/Thumbnails/`.
The project's own `Launcher` and `Showcase` scenes are in the build settings but have no launcher card.
Pipeline fix: the launcher URP asset now has the reflection probe atlas on (the example project's asset has it): without it the Volvo's Complex Lit paint and the eye's glTF shader graph drew flat cyan. The builder switches it on.

## L-012 Card order and names (2026-11-16)
Cards, left to right: Biology (human anatomy), Medical (eye), Engineering (i4 engine), Electronics (PCB), Automobile (Volvo S90), as set by the owner ("Automobile" is the owner's spelling; it replaced my "Automotive"). The Engineering preview uses `kmax-display-example/CarEngineAnimated - i4/Prefabs/Enginei4.prefab`, the textured prefab the scene uses; the raw `Enginei4.FBX` has plain white diffuse materials and looked white.

## L-013 App icon (2026-11-16)
`Game Info So.asset` has the FMax icon (`UI/Icons/FMaxAppIcon.png`, the owner's artwork converted from WebP to a 1024 x 1024 PNG, imported uncompressed, no mipmaps) as `properties.icon`. The base build pipeline (`BuildContainer` -> `BuildBase.SetIcon`) puts that texture into every player icon slot at build, so the next build uses it. The builder's `Update Game Info` sets it again, so it survives a rebuild. The editor's current Player Settings icon (the base project's) was not changed.

## L-014 One generic loading screen (2026-11-16)
`Resources/KMaxLoadingScreen.prefab` (built by `KMax/Launcher/Build Loading Screen`, `LoadingScreenBuilder`) holds the only loading screen; `LoadingScreenBootstrap` instantiates it before the first scene (`DontDestroyOnLoad`), `LoadingScreen.Load(scenePath)` fades in, loads async, holds 0.6 s and fades out. The launcher calls it directly; the other projects call it by reflection through their own `SceneLoader` helper so they keep no assembly reference to the launcher and still run alone (fallback `SceneManager.LoadScene`). Risk: Screen Space Overlay may not show in the KMAX stereo rig on hardware (untested); if so, switch the canvas to a world-space stereo canvas like the UI. `kmax-display-example` Next Scene still cycles by build index, but the button is hidden (L-015).

## L-015 PCB unlit, hidden Next Scene, colour audit (2026-11-16)
PCB materials are URP Unlit (`KMax/PCB Assembly/Convert Materials To Unlit`), scene lights off, LED glow drives `_BaseColor`. `Next Scene` button is inactive in the three kmax-display-example scenes. UI colours of the three example scenes were snapped to the Design.md palette; product swatches are exempt.

## L-016 kmax-automobile-engine added; exhibit screen layout rule (2026-11-16)
Two cards (Engine Assembly, Engine Experience) after Automobile. Design.md 6.1 now fixes the layout of every exhibit screen from the owner's reference: title and short description top left, controls column top right, control instructions in the bottom-centre caption card (70 tall for one line, 124 for several). Rules.md check log has the full R-1..R-9 result for the project.

## L-017 Topic home screens: Engineering (2026-11-17)
The launcher builder now makes more than one home screen from a `ScreenSpec` (scene, catalog, title, projects, Home button). `EngineeringHomeScene` is a copy of the launcher scene rebuilt with `EngineeringCatalog` (title "SELECT AN ENGINE") and a `LauncherHomeButton` top left. The main catalog's Engineering entry points at that scene; the builder syncs every entry's scene path with its table and removes catalog entries that are not in the table. To add another topic screen: add a `ProjectDefaults[]`, a `ScreenSpec`, build it in `BuildLauncherScene` and add its scene to `CollectScenePaths`.

## L-018 Engine scenes go Home to the Engineering screen (2026-11-17)
Owner's rule: the Home button of the three engine scenes (Inline-Four in `VirtualExhibition WR`, Pontiac Assembly, Pontiac Experience) returns to `EngineeringHomeScene`; every other scene's Home still returns to `KmaxLauncherScene` (Rules.md R-7 stays the default). Set in each scene's `LauncherHomeButton.sceneName`. The topic's own Home button reaches the launcher.

## L-019 Hidden cards, shared backdrop (2026-11-17)
`LauncherProjectEntry.hidden`: the catalog counts and lists only visible entries (`Count`/`Get`), so a hidden project keeps its entry, scene and build-settings slot but has no card. The builder table sets it (`ProjectDefaults(..., hidden)`); Medical is hidden by the owner until told otherwise. Backdrop rule: every experience except Automobile shows the anatomy home look (slate camera colour and drifting dust). Camera colours are compensated for each scene's tonemapping and colour adjustments (measured by rendering the camera), so they differ in raw value from `#252A36`.

## L-020 Exhibit screen layout and rule R-10 (2026-11-17)
From the owner's review of the Inline-Four screen: Back (chevron icon) top left, Menu first in the top-right controls column, all other buttons in that column, icon Previous / Next, information panel and idle prompt in the bottom-centre caption card, particles that do not rotate with the model. Written as Design.md 6.2 and Rules.md R-10. Applied so far to `VirtualExhibition WR`. The Menu button is a copy of the Home button (`LauncherHomeButton` to `KmaxLauncherScene`); the old Home became Back and goes to the Engineering screen (L-018). Particles: the ambient motes are re-parented under `XRRig` with local simulation space because that scene orbits the camera, not the model.

## L-021 Menu replaces Back on the engine screens (2026-11-17)
Owner: Menu does what Back did, so the engine screens keep only Menu (top-right column, opens `EngineeringHomeScene`). Previous / Next return to the anatomy text buttons (163 x 46) instead of icons; the idle caption card is always visible; information goes in the same card. Design.md 6.2 and Rules.md R-10 rewritten to match. The chevron icons stay in `kmax-display-example/Materials/UI/Icons` and `kmax-automobile-engine/UI/Sprites` unused.

## L-022 Volvo screen to R-10 (2026-11-17)
Owner asked for the car screen to follow the engine screens' layout. Done in `VolvoS90` only through scene edits (rect positions, sizes, labels, a Subtitle, a CaptionCard and a CardHint object; icons removed from the text buttons). Open risk: depth-tested world UI hides bottom elements in some tour poses. Fix options: a ZTest-Always UI shader plus a TMP overlay shader, listed in the build's always-included shaders; or keep information at the top. Waiting for the owner's choice.

## L-023 Floor follows the model in the Volvo tour (2026-11-17)
`ModelFloorFollower` (kmax-display-example, on `Showroom`) moves and scales the floor with `VolvoModelPivot/Volvo S90`, which the focus view scales and recentres during the tour (the Eye scene's behaviour, kept as is). Replaces the earlier idea of hiding the floor. Any scene that has a floor under a focus-zoomed model should use the same component.

## L-024 Volvo: no ground plate, shared backdrop (2026-11-17)
Owner: hide the ground plate on the Volvo screen and give it the particle background of the other scenes. `Showroom/Floor` is inactive (the L-023 follower stays in the scene, disabled, in case the floor returns); the Volvo now has the slate camera colour and the three mote layers like the Inline-Four and Eye screens. The "except Automobile" backdrop exception in L-020's era is lifted.

## L-025 Automobile home screen, three cars (2026-11-17)
The Automobile card opens `AutomobileHomeScene` (catalog `AutomobileCatalog.asset`) listing Volvo S90, Ford Mustang GT and Volvo C40 Recharge. Menu in each car scene returns to this screen; its Home button returns to the launcher (R-7). The Mustang is named from its shape (S550, 2015-2017 pre-facelift, pony badge, tri-bar tail lamps); the FBX has no embedded name, so confirm with the asset source. Facts used in text come from public spec sheets (about 435 hp 5.0 V8 for the Mustang GT; Volvo C40 Recharge Twin Motor about 300 kW, length 4440 mm).

## L-026 Car builder squares the pivot; Reset View auto-hides (2026-11-17)
The Automobile cars are built with the model pivot at yaw 0 (turned to 225 at the end) so world-space measurements are exact. Mustang lamps are extracted from the body glass sub-mesh into lens meshes. `ViewerFlyController.IsAtHome` hides Reset View at the home pose in all car scenes, S90 included.

## L-027 Cars: ghost the rest on every tour stop; C40 static (2026-11-17)
Owner request. Mustang and C40 tour stops fade all other parts. C40 has no door or sunroof movement. Mustang tyres are a separate sub-mesh.

## L-028 C40 tour: headlight group, no grille stop (2026-11-17)
Owner request; the grille text was wrong. Mustang doors re-seated against the measured body opening.

## L-029 Retail card opens the showroom directly (2026-11-17)
Owner request: Retail launches a screen with the furniture options. The showroom scene (`kmax-retail-products/Scenes/RetailShowroom.unity`) has the category buttons itself, so no topic card screen sits between. Menu returns to the launcher.

## L-030 Stylus standard R-11, one bottom panel R-12, 3D loading screen (2026-11-17)
Owner request after the build test. See Rules.md R-11, R-12 and the check log.

## 2026-11-19
- **One Back button replaces Menu and Home.** Both went back, so one button at the bottom left does it everywhere (R-7 and R-10 text still names Home and Menu; not reworded).
- **Layout tools patch saved scenes** (`BackButtonLayout`, `StylusTutorialBuilder`, `TryAnotherInstaller`) instead of every builder, because many scenes are copies of the Volvo scene; each tool is safe to run again.
- **The SDK was patched for the pen** (`KmaxStylus.StateOf`): any of the three buttons that starts a press on the interface is the primary press until all three are up. Chosen over synthesising clicks per scene. Cost: an SDK update overwrites it.
- **Turntable previews on home screens stay non-interactive** (R-14); the tutorial words say so.
