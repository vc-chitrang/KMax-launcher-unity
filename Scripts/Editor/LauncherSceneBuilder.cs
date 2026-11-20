using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ViitorCloud.KMaxLauncher.Ui;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Builds Scenes/KmaxLauncherScene.unity to the anatomy exhibit's launcher layout: a letter-spaced title, the chosen
    /// project in cyan under it, the project's model turning in front of the glass, rows of cards and a Load pill. The
    /// stereo rig, lights, input and backdrop come from a project's Home scene; every component of that project is then
    /// removed, so the launcher scene depends on no project's scripts. Also keeps the build settings and the Game Info So
    /// in step with the projects under Kmax_projects.
    /// </summary>
    public static class LauncherSceneBuilder {
        private const string GamesRoot = "Assets/Games";
        private const string LauncherRoot = GamesRoot + "/KMax-launcher-unity";
        private const string LauncherScenePath = LauncherRoot + "/Scenes/KmaxLauncherScene.unity";
        private const string EngineeringScenePath = LauncherRoot + "/Scenes/EngineeringHomeScene.unity";
        private const string CatalogFolder = LauncherRoot + "/Data";
        private const string CatalogPath = CatalogFolder + "/LauncherCatalog.asset";
        private const string EngineeringCatalogPath = CatalogFolder + "/EngineeringCatalog.asset";
        private const string LauncherTitle = "EXPLORE IN 3D";
        private const string EngineeringTitle = "UNDER THE HOOD";
        private const string AutomobileScenePath = LauncherRoot + "/Scenes/AutomobileHomeScene.unity";
        private const string AutomobileCatalogPath = CatalogFolder + "/AutomobileCatalog.asset";
        private const string AutomobileTitle = "CHOOSE YOUR RIDE";
        private const string RetailScenePath = GamesRoot + "/kmax-retail-products/Scenes/RetailShowroom.unity";
        private const string RetailHomeScenePath = LauncherRoot + "/Scenes/RetailHomeScene.unity";
        private const string RetailCatalogPath = CatalogFolder + "/RetailCategoryCatalog.asset";
        private const string RetailTitle = "STEP INTO THE SHOWROOM";
        private const string RetailPrefabs = GamesRoot + "/kmax-retail-products/Prefabs";
        private const string RetailCategoryKey = "KMax.Retail.Category";
        private const string EducationRoot = GamesRoot + "/kmax-education";
        private const string EducationHomeScenePath = LauncherRoot + "/Scenes/EducationHomeScene.unity";
        private const string EducationCatalogPath = CatalogFolder + "/EducationCatalog.asset";
        private const string EducationTitle = "LEARN BY EXPLORING";
        private const string EarthPrefabPath = EducationRoot + "/Prefabs/Earth.prefab";
        private const string GravitySceneAssetPath = EducationRoot + "/Scenes/GravityWell.unity";
        private const string EducationThumbnail = EducationRoot + "/UI/Thumbnails/Education.png";
        private const string PhysicsThumbnail = EducationRoot + "/UI/Thumbnails/Physics.png";
        private const string AircraftPrefabPath = EducationRoot + "/Prefabs/Aircraft.prefab";
        private const string AircraftSceneAssetPath = EducationRoot + "/Scenes/AircraftFlight.unity";
        private const string TechnologyThumbnail = EducationRoot + "/UI/Thumbnails/Technology.png";
        private const float AircraftThumbnailMargin = 0.75f;
        private const string ArtScenePath = EducationRoot + "/Scenes/ArtPainting.unity";
        private const string MonaLisaPrefabPath = EducationRoot + "/Prefabs/MonaLisa.prefab";
        private const string ArtThumbnail = EducationRoot + "/UI/Thumbnails/Arts.png";
        private const string MonaLisaFramePath = EducationRoot + "/Prefabs/MonaLisaFrame.prefab";
        private const string ChemistryScenePath = EducationRoot + "/Scenes/ChemistryBonds.unity";
        private const string ChemistryThumbPrefab = EducationRoot + "/Prefabs/ChemistryThumb.prefab";
        private const string ChemistryThumbnail = EducationRoot + "/UI/Thumbnails/Chemistry.png";
        private const float ChemistryThumbnailMargin = 0.8f;
        private const string SolarScenePath = EducationRoot + "/Scenes/SolarSystem.unity";
        private const string SolarPrefabPath = EducationRoot + "/Prefabs/SolarSystem.prefab";
        private const string SolarPreviewPrefabPath = EducationRoot + "/Prefabs/SolarSystemPreview.prefab";
        private const string SolarThumbnail = EducationRoot + "/UI/Thumbnails/SolarSystem.png";
        private const float SolarThumbnailMargin = 0.9f;
        private const float ArtThumbnailMargin = 0.7f;
        private const float EarthThumbnailMargin = 0.5f;
        private const float HomeWidth = 150f;
        private const float HomeHeight = 42f;
        private const float HomeMargin = 32f;
        private const float HomeBottom = 32f;
        private const float HomeFontSize = 17f;
        private const string SpritesFolder = LauncherRoot + "/UI/Sprites";
        private const string ThumbnailsFolder = LauncherRoot + "/UI/Thumbnails";
        private const string MaterialsFolder = LauncherRoot + "/Materials";
        private const string GameInfoPath = LauncherRoot + "/Game Info So.asset";
        private const string AppIconPath = LauncherRoot + "/UI/Icons/FMaxAppIcon.png";
        private const string TemplateProject = GamesRoot + "/KMaxPCBAssembly";
        private const string PipelineFolder = LauncherRoot + "/URP";
        private const string RenderPipelineAssetPath = PipelineFolder + "/KMaxLauncherURP.asset";
        private const string RendererAssetPath = PipelineFolder + "/KMaxLauncherURP_Renderer.asset";
        private const string SourcePipelineAssetPath = GamesRoot + "/kmax-human-anatomy/URPAssets/URPAsset.asset";
        private const string SourceRendererAssetPath = GamesRoot + "/kmax-human-anatomy/URPAssets/URPAsset_Renderer.asset";
        private const int SoftShadowQualityHigh = 3;
        private const int ShadowMapResolution = 4096;
        private const int AntiAliasingSamples = 4;
        private const string FillSpriteName = "GlassFill.png";
        private const string BorderSpriteName = "GlassBorder.png";
        private const string DustSpriteName = "SoftDot.png";
        private const string DustShaderName = "Universal Render Pipeline/Particles/Unlit";
        private const string MainSceneFileName = "Main.unity";

        private const float GlassBaseSize = 64f;
        private const float GlassBaseSlice = 16f;
        private const int DefaultCardsPerRow = 7;
        private const int RetailCardsPerRow = 6;
        private const int RowCount = 2;

        private static int cardsPerRow = DefaultCardsPerRow;
        private const float CardWidth = 172f;
        private const float CardHeight = 132f;
        private const float CardSpacing = 12f;
        private const float ThumbnailMargin = 8f;
        private const float ThumbnailWidth = 156f;
        private const float ThumbnailHeight = 88f;
        private const float LabelHeight = 26f;
        private const float LabelFontSize = 14f;
        private const float SelectBarHeight = 3f;
        private const float SelectBarInset = 18f;
        private const float LoadHeight = 42f;
        private const float CardsBottom = 26f;
        private const float LoadFontSize = 17f;
        private const float TitleTop = 22f;
        private const float CameraFarPlane = 3f;
        private const string GlowName = "BackdropGlow";
        private const float GlowDepth = 0.25f;
        private const string StudioReflectionPath = MaterialsFolder + "/LauncherStudioReflection.asset";
        private const string SourceStudioReflectionPath = GamesRoot + "/KMaxPCBAssembly/Models/Materials/PcbStudioReflection.asset";
        private const float TitleFontSize = 30f;
        private const float TitleLetterSpacing = 6f;
        private const float SubtitleFontSize = 17f;
        private const float VersionFontSize = 15f;

        private static readonly ProjectDefaults[] DefaultProjects = new ProjectDefaults[] {
            new ProjectDefaults(
                "Biology",
                "Explore the body, organ by organ",
                GamesRoot + "/kmax-human-anatomy/Scene/Main.unity",
                GamesRoot + "/kmax-human-anatomy/Generated/Resources/Anatomy/body.prefab",
                GamesRoot + "/kmax-human-anatomy/Generated/Resources/Anatomy/Thumbs/body.png"),
            new ProjectDefaults(
                "Medical",
                "Take the eye apart, structure by structure",
                GamesRoot + "/kmax-display-example/Scenes/EyeAnatomy.unity",
                GamesRoot + "/kmax-display-example/Model/EyeAnatomy.glb",
                GamesRoot + "/kmax-display-example/Materials/Thumbnails/EyeAnatomyThumbnail.png",
                true),
            new ProjectDefaults(
                "Engineering",
                "Three car engines: explore, assemble, run",
                EngineeringScenePath,
                GamesRoot + "/kmax-display-example/CarEngineAnimated - i4/Prefabs/Enginei4.prefab",
                GamesRoot + "/kmax-display-example/Materials/Thumbnails/EngineThumbnail.png"),
            new ProjectDefaults(
                "Electronics",
                "Build circuits on a 3D board",
                GamesRoot + "/KMaxPCBAssembly/Scenes/Main.unity",
                GamesRoot + "/KMaxPCBAssembly/Prefabs/Exhibits/Exhibit_BlinkLED.prefab",
                GamesRoot + "/KMaxPCBAssembly/UI/Thumbnails/Thumb_BlinkLED.png"),
            new ProjectDefaults(
                "Automobile",
                "Three cars: open, light up, start and tour them",
                AutomobileScenePath,
                GamesRoot + "/kmax-display-example/Model/VOLVO/Volvo S90.fbx",
                GamesRoot + "/kmax-display-example/Materials/Thumbnails/VolvoS90Thumbnail.png"),
            new ProjectDefaults(
                "Retail",
                "Furniture: pick a category, a design, then a finish",
                RetailHomeScenePath,
                GamesRoot + "/kmax-retail-products/Prefabs/Armchair_04.prefab",
                GamesRoot + "/kmax-retail-products/UI/Thumbnails/RetailThumbnail.png"),
            new ProjectDefaults(
                "Education",
                "Physics, and more subjects to come: start with gravity",
                EducationHomeScenePath,
                EarthPrefabPath,
                EducationThumbnail)
        };

        private static readonly ProjectDefaults[] EducationProjects = new ProjectDefaults[] {
            new ProjectDefaults(
                "Physics",
                "Gravity explained with the Earth: drop, orbit, escape",
                GravitySceneAssetPath,
                EarthPrefabPath,
                PhysicsThumbnail),
            new ProjectDefaults(
                "Technology",
                "How an aeroplane flies, on a 1909 biplane",
                AircraftSceneAssetPath,
                AircraftPrefabPath,
                TechnologyThumbnail),
            new ProjectDefaults(
                "Arts",
                "Famous paintings: the Mona Lisa, explained",
                ArtScenePath,
                MonaLisaFramePath,
                ArtThumbnail),
            new ProjectDefaults(
                "Chemistry",
                "Carbon compounds in 3D: single, double and triple bonds",
                ChemistryScenePath,
                ChemistryThumbPrefab,
                ChemistryThumbnail),
            new ProjectDefaults(
                "Solar System",
                "The solar system: planets in orbit, explained",
                SolarScenePath,
                SolarPreviewPrefabPath,
                SolarThumbnail)
        };

        private static readonly ProjectDefaults[] AutomobileProjects = new ProjectDefaults[] {
            new ProjectDefaults(
                "Volvo S90",
                "Tour a Volvo S90, door by door",
                GamesRoot + "/kmax-display-example/Scenes/VolvoS90.unity",
                GamesRoot + "/kmax-display-example/Model/VOLVO/Volvo S90.fbx",
                GamesRoot + "/kmax-display-example/Materials/Thumbnails/VolvoS90Thumbnail.png"),
            new ProjectDefaults(
                "Volvo C40 Recharge",
                "A fully electric coupe SUV, part by part",
                GamesRoot + "/kmax-display-example/Scenes/VolvoC40.unity",
                GamesRoot + "/kmax-display-example/Model/VolvoC40/Prefabs/VolvoC40Preview.prefab",
                GamesRoot + "/kmax-display-example/Materials/Thumbnails/VolvoC40Thumbnail.png"),
            new ProjectDefaults(
                "Ford Mustang GT",
                "A V8 fastback coupe, part by part",
                GamesRoot + "/kmax-display-example/Scenes/FordMustangGT.unity",
                GamesRoot + "/kmax-display-example/Model/FordMustangGT/Prefabs/FordMustangGTPreview.prefab",
                GamesRoot + "/kmax-display-example/Materials/Thumbnails/FordMustangGTThumbnail.png")
        };

        private static readonly ProjectDefaults[] RetailProjects = new ProjectDefaults[] {
            new ProjectDefaults(
                "Armchair",
                "Five armchairs in leather and fabric",
                RetailScenePath,
                RetailPrefabs + "/Armchair_04.prefab",
                RetailCategoryThumbnails.PathFor("Armchair"),
                RetailCategoryKey,
                "Armchair"),
            new ProjectDefaults(
                "Bed",
                "Four beds, each in several variants",
                RetailScenePath,
                RetailPrefabs + "/Bed_02.prefab",
                RetailCategoryThumbnails.PathFor("Bed"),
                RetailCategoryKey,
                "Bed"),
            new ProjectDefaults(
                "Dining table",
                "Four dining tables to look over",
                RetailScenePath,
                RetailPrefabs + "/Dining_table_02.prefab",
                RetailCategoryThumbnails.PathFor("Dining table"),
                RetailCategoryKey,
                "Dining table"),
            new ProjectDefaults(
                "Coffee table",
                "Four coffee tables for the lounge",
                RetailScenePath,
                RetailPrefabs + "/Coffee_table_03.prefab",
                RetailCategoryThumbnails.PathFor("Coffee table"),
                RetailCategoryKey,
                "Coffee table"),
            new ProjectDefaults(
                "TV unit",
                "Three TV units, in size and in feet",
                RetailScenePath,
                RetailPrefabs + "/TV_unit_02.prefab",
                RetailCategoryThumbnails.PathFor("TV unit"),
                RetailCategoryKey,
                "TV unit"),
            new ProjectDefaults(
                "Wardrobe",
                "Two wardrobes, door by door",
                RetailScenePath,
                RetailPrefabs + "/Wardrobe_02.prefab",
                RetailCategoryThumbnails.PathFor("Wardrobe"),
                RetailCategoryKey,
                "Wardrobe")
        };

        private static readonly ProjectDefaults[] EngineeringProjects = new ProjectDefaults[] {
            new ProjectDefaults(
                "Inline-Four Engine",
                "Explode and run an i4 engine",
                GamesRoot + "/kmax-display-example/Scenes/VirtualExhibition WR.unity",
                GamesRoot + "/kmax-display-example/CarEngineAnimated - i4/Prefabs/Enginei4.prefab",
                GamesRoot + "/kmax-display-example/Materials/Thumbnails/EngineThumbnail.png"),
            new ProjectDefaults(
                "Pontiac V8 Assembly",
                "Drag the parts of a Pontiac V8 into place",
                GamesRoot + "/kmax-automobile-engine/Scenes/EngineAssembly.unity",
                GamesRoot + "/kmax-automobile-engine/Prefabs/PontiacEnginePreview.prefab",
                GamesRoot + "/kmax-automobile-engine/UI/Thumbnails/EngineAssemblyThumbnail.png"),
            new ProjectDefaults(
                "Pontiac V8 Experience",
                "Explode, explore and start a Pontiac V8",
                GamesRoot + "/kmax-automobile-engine/Scenes/EngineExperience.unity",
                GamesRoot + "/kmax-automobile-engine/Prefabs/PontiacEnginePreview.prefab",
                GamesRoot + "/kmax-automobile-engine/UI/Thumbnails/EngineExperienceThumbnail.png")
        };

        /// <summary>One home screen the builder makes: the launcher itself, or a topic's own home screen with a Home button back to the launcher.</summary>
        private class ScreenSpec {
            public ScreenSpec(string scenePath, string catalogPath, string title, ProjectDefaults[] projects, bool homeButton, int cardsPerRow = DefaultCardsPerRow) {
                CardsPerRow = cardsPerRow;
                ScenePath = scenePath;
                CatalogPath = catalogPath;
                Title = title;
                Projects = projects;
                HomeButton = homeButton;
            }

            public string ScenePath { get; private set; }

            public string CatalogPath { get; private set; }

            public string Title { get; private set; }

            public ProjectDefaults[] Projects { get; private set; }

            public bool HomeButton { get; private set; }

            public int CardsPerRow { get; private set; }
        }

        private static readonly ScreenSpec LauncherScreen = new ScreenSpec(LauncherScenePath, CatalogPath, LauncherTitle, DefaultProjects, false);
        private static readonly ScreenSpec EngineeringScreen = new ScreenSpec(EngineeringScenePath, EngineeringCatalogPath, EngineeringTitle, EngineeringProjects, true);
        private static readonly ScreenSpec AutomobileScreen = new ScreenSpec(AutomobileScenePath, AutomobileCatalogPath, AutomobileTitle, AutomobileProjects, true);

        private static readonly ScreenSpec RetailScreen = new ScreenSpec(RetailHomeScenePath, RetailCatalogPath, RetailTitle, RetailProjects, true, RetailCardsPerRow);

        private static readonly ScreenSpec EducationScreen = new ScreenSpec(EducationHomeScenePath, EducationCatalogPath, EducationTitle, EducationProjects, true);

        private static readonly Color Muted = new Color32(148, 163, 184, 255);
        private static readonly Color Accent = new Color32(6, 182, 212, 255);
        private static readonly Color AccentBright = new Color32(34, 211, 238, 255);
        private static readonly Color CardRestFill = new Color(0.059f, 0.09f, 0.165f, 0.82f);
        private static readonly Color CardRestOutline = new Color(0.2f, 0.255f, 0.333f, 0.6f);
        private static readonly Color CardHover = new Color32(16, 110, 134, 245);
        private static readonly Color CardHoverNormal = new Color32(14, 63, 78, 235);
        private static readonly Color LoadFill = new Color32(8, 86, 104, 240);
        private static readonly Color LoadHover = new Color32(14, 124, 148, 250);
        private static readonly Color LabelRest = new Color(0.886f, 0.91f, 0.941f, 1f);

        [MenuItem("KMax/Launcher/Build Everything")]
        public static void BuildEverything() {
            if (!BuildLauncherScene()) {
                return;
            }

            SyncBuildSettings();
            UpdateGameInfo();
        }

        /// <summary>
        /// Makes the launcher's own URP asset (a copy of the anatomy project's with soft shadows, a 4096 shadow map and 4x MSAA, as the
        /// reference KMaxDisplayBase ran its shadows) if it is missing, and returns it.
        /// </summary>
        [MenuItem("KMax/Launcher/Ensure Render Pipeline Asset")]
        public static UnityEngine.Rendering.RenderPipelineAsset EnsureRenderPipelineAsset() {
            UnityEngine.Rendering.RenderPipelineAsset existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.RenderPipelineAsset>(RenderPipelineAssetPath);
            if (existing != null) {
                EnableReflectionProbeAtlas(existing);
                return existing;
            }

            EnsureFolder(PipelineFolder);
            if (!AssetDatabase.CopyAsset(SourcePipelineAssetPath, RenderPipelineAssetPath) || !AssetDatabase.CopyAsset(SourceRendererAssetPath, RendererAssetPath)) {
                Debug.LogError($"[Launcher Builder] Could not copy the URP assets from '{SourcePipelineAssetPath}'.");
                return null;
            }

            UnityEngine.Rendering.RenderPipelineAsset created = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.RenderPipelineAsset>(RenderPipelineAssetPath);
            UnityEngine.Object renderer = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(RendererAssetPath);
            SerializedObject serialized = new SerializedObject(created);
            SerializedProperty renderers = serialized.FindProperty("m_RendererDataList");
            for (int i = 0; i < renderers.arraySize; i++) {
                renderers.GetArrayElementAtIndex(i).objectReferenceValue = renderer;
            }

            serialized.FindProperty("m_SoftShadowsSupported").boolValue = true;
            serialized.FindProperty("m_SoftShadowQuality").intValue = SoftShadowQualityHigh;
            serialized.FindProperty("m_MainLightShadowmapResolution").intValue = ShadowMapResolution;
            serialized.FindProperty("m_MSAA").intValue = AntiAliasingSamples;
            serialized.FindProperty("m_ReflectionProbeAtlas").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(created);
            AssetDatabase.SaveAssets();
            return created;
        }

        /// <summary>Complex Lit (the Volvo's car paint) and the glTF shader graph draw flat cyan in the preview with the reflection probe atlas off.</summary>
        private static void EnableReflectionProbeAtlas(UnityEngine.Rendering.RenderPipelineAsset asset) {
            SerializedObject serialized = new SerializedObject(asset);
            SerializedProperty atlas = serialized.FindProperty("m_ReflectionProbeAtlas");
            if (atlas != null && !atlas.boolValue) {
                atlas.boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>Applies the launcher's URP asset to the editor the way the base project's Packages window does for a Game Info So.</summary>
        [MenuItem("KMax/Launcher/Apply Render Pipeline To Editor")]
        public static void ApplyRenderPipelineToEditor() {
            UnityEngine.Rendering.RenderPipelineAsset asset = EnsureRenderPipelineAsset();
            if (asset == null) {
                return;
            }

            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = asset;
            QualitySettings.renderPipeline = asset;
            Debug.Log($"[Launcher Builder] Editor now renders with '{asset.name}'.");
        }

        [MenuItem("KMax/Launcher/Build Launcher Scene")]
        public static bool BuildLauncherScene() {
            Scene open = EditorSceneManager.GetActiveScene();
            if (open.isDirty && open.path != LauncherScenePath && open.path != EngineeringScenePath && open.path != AutomobileScenePath && open.path != RetailHomeScenePath && open.path != EducationHomeScenePath) {
                Debug.LogError($"[Launcher Builder] Save or discard the open scene '{open.path}' first.");
                return false;
            }

            for (int i = 0; i < RetailProjects.Length; i++) {
                if (!RetailCategoryThumbnails.Ensure(RetailProjects[i].Label, RetailProjects[i].PreviewPrefabPath)) {
                    return false;
                }
            }

            if (!RetailCategoryThumbnails.EnsureAt(EducationThumbnail, EarthPrefabPath, EarthThumbnailMargin) || !RetailCategoryThumbnails.EnsureAt(PhysicsThumbnail, EarthPrefabPath, EarthThumbnailMargin)) {
                return false;
            }

            if (!RetailCategoryThumbnails.EnsureAt(TechnologyThumbnail, AircraftPrefabPath, AircraftThumbnailMargin)
                || !RetailCategoryThumbnails.EnsureAt(ArtThumbnail, MonaLisaFramePath, ArtThumbnailMargin)
                || !RetailCategoryThumbnails.EnsureAt(ChemistryThumbnail, ChemistryThumbPrefab, ChemistryThumbnailMargin)
                || !RetailCategoryThumbnails.EnsureAt(SolarThumbnail, SolarPreviewPrefabPath, SolarThumbnailMargin)) {
                return false;
            }

            return BuildScreen(LauncherScreen) && BuildScreen(EngineeringScreen) && BuildScreen(AutomobileScreen) && BuildScreen(RetailScreen) && BuildScreen(EducationScreen);
        }

        private static bool BuildScreen(ScreenSpec spec) {
            cardsPerRow = spec.CardsPerRow;
            Sprite fill = EnsureSprite(FillSpriteName);
            Sprite border = EnsureSprite(BorderSpriteName);
            Sprite dust = EnsureSprite(DustSpriteName);
            if (fill == null || border == null || dust == null) {
                return false;
            }

            LauncherCatalog catalog = EnsureCatalog(spec);
            if (catalog == null) {
                return false;
            }

            if (spec.ScenePath != LauncherScenePath && !File.Exists(ToAbsolute(spec.ScenePath))) {
                if (!AssetDatabase.CopyAsset(LauncherScenePath, spec.ScenePath)) {
                    Debug.LogError($"[Launcher Builder] Could not make '{spec.ScenePath}' from the launcher scene.");
                    return false;
                }
            }

            Scene scene;
            if (!TryOpenBaseScene(spec.ScenePath, out scene)) {
                return false;
            }

            catalog = AssetDatabase.LoadAssetAtPath<LauncherCatalog>(spec.CatalogPath);
            RemoveProjectContent(scene);
            FixDustMaterial(scene, dust);
            ApplyLighting(scene);
            BuildBackdropGlow(scene, dust);
            RectTransform canvas = (RectTransform)FindRoot(scene, "HudCanvas").transform;
            ReplaceAlwaysOnTop(canvas.gameObject);
            if (canvas.GetComponent<StylusUiOnlyRay>() == null) {
                canvas.gameObject.AddComponent<StylusUiOnlyRay>();
            }

            for (int i = canvas.childCount - 1; i >= 0; i--) {
                UnityEngine.Object.DestroyImmediate(canvas.GetChild(i).gameObject);
            }

            TMP_Text title;
            TMP_Text subtitle;
            BuildHeader(canvas, out title, out subtitle);
            GameObject[] rows;
            LauncherCard[] cards = BuildCards(canvas, fill, border, out rows);
            Button load;
            TMP_Text loadLabel;
            BuildLoad((RectTransform)canvas.Find("Cards"), fill, border, out load, out loadLabel);
            if (spec.HomeButton) {
                BuildHomeButton(canvas, fill, border);
            }

            if (spec.ScenePath == LauncherScenePath) {
                BuildVersionLabel(canvas);
            }

            GameObject stage = new GameObject("PreviewStage");
            SceneManager.MoveGameObjectToScene(stage, scene);
            LauncherPreview preview = stage.AddComponent<LauncherPreview>();

            GameObject launcherObject = new GameObject("Launcher");
            SceneManager.MoveGameObjectToScene(launcherObject, scene);
            launcherObject.AddComponent<AudioSource>();
            launcherObject.AddComponent<LauncherAudio>();
            launcherObject.AddComponent<DoubleTapEscExit>();
            LauncherController controller = launcherObject.AddComponent<LauncherController>();
            SerializedObject serialized = new SerializedObject(controller);
            SetReference(serialized, "catalog", catalog);
            SetReferences(serialized, "cards", cards);
            SetReferences(serialized, "rows", rows);
            serialized.FindProperty("cardsPerRow").intValue = cardsPerRow;
            SetReference(serialized, "titleText", title);
            SetReference(serialized, "subtitleText", subtitle);
            SetReference(serialized, "loadButton", load);
            SetReference(serialized, "loadLabel", loadLabel);
            SetReference(serialized, "preview", preview);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Launcher Builder] Built {spec.ScenePath} with {catalog.Count} projects.");
            return true;
        }

        [MenuItem("KMax/Launcher/Sync Build Settings")]
        public static void SyncBuildSettings() {
            List<string> paths = CollectScenePaths();
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[paths.Count];
            for (int i = 0; i < paths.Count; i++) {
                scenes[i] = new EditorBuildSettingsScene(paths[i], true);
            }

            EditorBuildSettings.scenes = scenes;
            Debug.Log($"[Launcher Builder] Build settings now list {paths.Count} scenes, the launcher first.");
        }

        [MenuItem("KMax/Launcher/Update Game Info")]
        public static void UpdateGameInfo() {
            ScriptableObject info = AssetDatabase.LoadAssetAtPath<ScriptableObject>(GameInfoPath);
            if (info == null) {
                Debug.LogError($"[Launcher Builder] '{GameInfoPath}' is missing.");
                return;
            }

            SerializedObject serialized = new SerializedObject(info);
            SerializedProperty value = serialized.FindProperty("value");
            value.FindPropertyRelative("gameID").stringValue = "KMaxLauncher";
            value.FindPropertyRelative("repoPath").stringValue = "https://github.com/vc-chitrang/KMax-launcher-unity.git";
            value.FindPropertyRelative("projectSettings.colorSpace").intValue = 0;
            value.FindPropertyRelative("properties.title").stringValue = "FMax Launcher";
            value.FindPropertyRelative("properties.companyName").stringValue = "ViitorCloud";
            SerializedProperty versionProperty = value.FindPropertyRelative("properties.version");
            if (string.IsNullOrEmpty(versionProperty.stringValue)) {
                versionProperty.stringValue = "1.0.0";
            }

            SerializedProperty versionCodeProperty = value.FindPropertyRelative("properties.versionCode");
            if (versionCodeProperty.intValue <= 0) {
                versionCodeProperty.intValue = 1;
            }
            value.FindPropertyRelative("properties.icon").objectReferenceValue = EnsureAppIcon();
            value.FindPropertyRelative("packages.urp").intValue = 1;
            value.FindPropertyRelative("packages.doNotForceChangePipeline").intValue = 0;
            UnityEngine.Object pipeline = EnsureRenderPipelineAsset();
            if (pipeline == null) {
                Debug.LogError($"[Launcher Builder] The render pipeline asset '{RenderPipelineAssetPath}' could not be made.");
            }

            value.FindPropertyRelative("packages.renderingPipelineAsset").objectReferenceValue = pipeline;

            List<string> paths = CollectScenePaths();
            SerializedProperty scenes = value.FindPropertyRelative("scenes");
            scenes.arraySize = paths.Count;
            for (int i = 0; i < paths.Count; i++) {
                SerializedProperty element = scenes.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("active").boolValue = true;
                element.FindPropertyRelative("scene").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(paths[i]);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(info);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Launcher Builder] Game Info So updated: gameID KMaxLauncher, URP, {paths.Count} scenes.");
        }

        /// <summary>The app icon the build pipeline puts into every icon slot (BuildBase.SetIcon): the FMax icon, imported uncompressed.</summary>
        private static Texture2D EnsureAppIcon() {
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconPath);
            if (icon == null) {
                Debug.LogError($"[Launcher Builder] The app icon '{AppIconPath}' is missing.");
                return null;
            }

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(AppIconPath);
            if (importer.textureType != TextureImporterType.Default || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.mipmapEnabled) {
                importer.textureType = TextureImporterType.Default;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 1024;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconPath);
            }

            return icon;
        }

        private static List<string> CollectScenePaths() {
            List<string> projectScenes = new List<string>();
            string[] folders = AssetDatabase.GetSubFolders(GamesRoot);
            for (int f = 0; f < folders.Length; f++) {
                if (folders[f] == LauncherRoot) {
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets("t:Scene", new string[] { folders[f] });
                for (int i = 0; i < guids.Length; i++) {
                    projectScenes.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
                }
            }

            projectScenes.Sort(CompareProjectScenes);
            List<string> all = new List<string>();
            all.Add(LauncherScenePath);
            all.Add(EngineeringScenePath);
            all.Add(AutomobileScenePath);
            all.Add(RetailHomeScenePath);
            all.Add(EducationHomeScenePath);
            all.AddRange(projectScenes);
            return all;
        }

        private static int CompareProjectScenes(string left, string right) {
            string leftFolder = Path.GetDirectoryName(left);
            string rightFolder = Path.GetDirectoryName(right);
            int byFolder = string.CompareOrdinal(leftFolder, rightFolder);
            if (byFolder != 0) {
                return byFolder;
            }

            bool leftMain = Path.GetFileName(left) == MainSceneFileName;
            bool rightMain = Path.GetFileName(right) == MainSceneFileName;
            if (leftMain != rightMain) {
                return leftMain ? -1 : 1;
            }

            return string.CompareOrdinal(left, right);
        }

        private static bool TryOpenBaseScene(string scenePath, out Scene scene) {
            if (!File.Exists(ToAbsolute(scenePath))) {
                Debug.LogError($"[Launcher Builder] The scene '{scenePath}' is missing; it holds the stereo rig the builder reuses.");
                scene = default(Scene);
                return false;
            }

            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!HasRoot(scene, "XRRig")) {
                Debug.LogError("[Launcher Builder] The launcher scene has no 'XRRig'; add the KMAX stereo rig prefab first.");
                return false;
            }

            return true;
        }

        private static bool HasRoot(Scene scene, string rootName) {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++) {
                if (roots[i].name == rootName) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The template's dust uses a Built-in shader that draws pink under URP; swap in a URP additive particle material.</summary>
        private static void FixDustMaterial(Scene scene, Sprite dust) {
            if (!HasRoot(scene, "DustMotes")) {
                return;
            }

            ParticleSystemRenderer particles = FindRoot(scene, "DustMotes").GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find(DustShaderName);
            if (particles == null || shader == null) {
                Debug.LogError($"[Launcher Builder] Cannot fix the dust material: shader '{DustShaderName}' or the particle renderer is missing.");
                return;
            }

            EnsureFolder(MaterialsFolder);
            string path = MaterialsFolder + "/LauncherDust.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetTexture("_BaseMap", dust.texture);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            particles.sharedMaterial = material;
        }

        private static Sprite EnsureSprite(string fileName) {
            bool sliced = fileName != DustSpriteName;
            string target = SpritesFolder + "/" + fileName;
            if (AssetDatabase.LoadAssetAtPath<Sprite>(target) == null) {
                string source = TemplateProject + "/UI/Sprites/" + fileName;
                if (!File.Exists(ToAbsolute(source))) {
                    Debug.LogError($"[Launcher Builder] The sprite '{source}' is missing.");
                    return null;
                }

                EnsureFolder(SpritesFolder);
                if (!AssetDatabase.CopyAsset(source, target)) {
                    Debug.LogError($"[Launcher Builder] Could not copy '{source}' to '{target}'.");
                    return null;
                }
            }

            Sprite loaded = AssetDatabase.LoadAssetAtPath<Sprite>(target);
            float scale = sliced && loaded != null ? Mathf.Max(1f, loaded.texture.width / GlassBaseSize) : 1f;
            float slice = GlassBaseSlice * scale;
            ImportAsSprite(target, sliced ? new Vector4(slice, slice, slice, slice) : Vector4.zero, 100f * scale);
            return AssetDatabase.LoadAssetAtPath<Sprite>(target);
        }

        private static void ImportAsSprite(string assetPath, Vector4 border, float pixelsPerUnit) {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder != border || importer.mipmapEnabled
                || !Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit)) {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = border;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        /// <summary>The launcher keeps its own copy of each card picture, so a project's generated folder can be rebuilt freely.</summary>
        private static Sprite EnsureThumbnail(string sourcePath, string label) {
            string target = ThumbnailsFolder + "/" + label.Replace(' ', '_') + Path.GetExtension(sourcePath);
            if (AssetDatabase.LoadAssetAtPath<Sprite>(target) == null) {
                if (!File.Exists(ToAbsolute(sourcePath))) {
                    Debug.LogWarning($"[Launcher Builder] The picture '{sourcePath}' for '{label}' is missing; its card shows the name only.");
                    return null;
                }

                EnsureFolder(ThumbnailsFolder);
                if (!AssetDatabase.CopyAsset(sourcePath, target)) {
                    Debug.LogWarning($"[Launcher Builder] Could not copy '{sourcePath}' to '{target}'.");
                    return null;
                }
            }

            ImportAsSprite(target, Vector4.zero, 100f);
            return AssetDatabase.LoadAssetAtPath<Sprite>(target);
        }

        private static LauncherCatalog EnsureCatalog(ScreenSpec spec) {
            LauncherCatalog catalog = AssetDatabase.LoadAssetAtPath<LauncherCatalog>(spec.CatalogPath);
            if (catalog == null) {
                EnsureFolder(CatalogFolder);
                catalog = ScriptableObject.CreateInstance<LauncherCatalog>();
                AssetDatabase.CreateAsset(catalog, spec.CatalogPath);
            }

            SerializedObject serialized = new SerializedObject(catalog);
            if (spec.Title != null) {
                serialized.FindProperty("title").stringValue = spec.Title;
            }

            SerializedProperty entries = serialized.FindProperty("entries");
            for (int i = entries.arraySize - 1; i >= 0; i--) {
                string label = entries.GetArrayElementAtIndex(i).FindPropertyRelative("label").stringValue;
                bool wanted = false;
                for (int d = 0; d < spec.Projects.Length; d++) {
                    if (spec.Projects[d].Label == label) {
                        wanted = true;
                    }
                }

                if (!wanted) {
                    entries.DeleteArrayElementAtIndex(i);
                }
            }

            for (int d = 0; d < spec.Projects.Length; d++) {
                ProjectDefaults project = spec.Projects[d];
                int index = FindEntry(entries, project.Label);
                if (index < 0) {
                    index = entries.arraySize;
                    entries.arraySize = index + 1;
                    SerializedProperty added = entries.GetArrayElementAtIndex(index);
                    added.FindPropertyRelative("label").stringValue = project.Label;
                    added.FindPropertyRelative("subtitle").stringValue = project.Subtitle;
                    added.FindPropertyRelative("scenePath").stringValue = project.ScenePath;
                    added.FindPropertyRelative("previewPrefab").objectReferenceValue = null;
                    added.FindPropertyRelative("thumbnail").objectReferenceValue = null;
                }

                SerializedProperty entry = entries.GetArrayElementAtIndex(index);
                SerializedProperty scenePath = entry.FindPropertyRelative("scenePath");
                if (scenePath.stringValue != project.ScenePath) {
                    scenePath.stringValue = project.ScenePath;
                }

                entry.FindPropertyRelative("hidden").boolValue = project.Hidden;
                entry.FindPropertyRelative("launchKey").stringValue = project.LaunchKey ?? "";
                entry.FindPropertyRelative("launchValue").stringValue = project.LaunchValue ?? "";

                SerializedProperty prefab = entry.FindPropertyRelative("previewPrefab");
                if (prefab.objectReferenceValue == null) {
                    prefab.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(project.PreviewPrefabPath);
                }

                SerializedProperty thumbnail = entry.FindPropertyRelative("thumbnail");
                UnityEngine.Object current = thumbnail.objectReferenceValue;
                bool owned = current != null && AssetDatabase.GetAssetPath(current).StartsWith(LauncherRoot, StringComparison.Ordinal);
                if (!owned) {
                    thumbnail.objectReferenceValue = EnsureThumbnail(project.ThumbnailSourcePath, project.Label);
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static int FindEntry(SerializedProperty entries, string label) {
            for (int i = 0; i < entries.arraySize; i++) {
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("label").stringValue == label) {
                    return i;
                }
            }

            return -1;
        }

        private static void RemoveProjectContent(Scene scene) {
            string[] removedRoots = new string[] { "Launcher", "ExhibitStage", "PreviewStage", "ShadowBackdrop" };
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++) {
                if (Array.IndexOf(removedRoots, roots[i].name) >= 0) {
                    UnityEngine.Object.DestroyImmediate(roots[i]);
                }
            }

            RemoveForeignComponents(scene, false);
            RemoveForeignComponents(scene, true);

            roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++) {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(roots[i]);
                Transform[] all = roots[i].GetComponentsInChildren<Transform>(true);
                for (int t = 0; t < all.Length; t++) {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(all[t].gameObject);
                }
            }
        }

        private static void RemoveForeignComponents(Scene scene, bool dependencies) {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++) {
                MonoBehaviour[] behaviours = roots[r].GetComponentsInChildren<MonoBehaviour>(true);
                for (int i = 0; i < behaviours.Length; i++) {
                    MonoBehaviour behaviour = behaviours[i];
                    if (behaviour == null) {
                        continue;
                    }

                    string assembly = behaviour.GetType().Assembly.GetName().Name;
                    if (!assembly.StartsWith("KMaxPCBAssembly", StringComparison.Ordinal)) {
                        continue;
                    }

                    bool isDependency = behaviour.GetType().Name == "StylusTip";
                    if (isDependency == dependencies) {
                        UnityEngine.Object.DestroyImmediate(behaviour);
                    }
                }
            }
        }

        private static void ReplaceAlwaysOnTop(GameObject canvasObject) {
            MonoBehaviour[] behaviours = canvasObject.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++) {
                if (behaviours[i] == null || behaviours[i].GetType().Name != "UiAlwaysOnTop") {
                    continue;
                }

                SerializedObject old = new SerializedObject(behaviours[i]);
                int renderQueue = old.FindProperty("renderQueue").intValue;
                int sortingOrder = old.FindProperty("canvasSortingOrder").intValue;
                bool reapply = old.FindProperty("reapplyOnEnable").boolValue;
                UnityEngine.Object.DestroyImmediate(behaviours[i]);

                UiAlwaysOnTop replacement = canvasObject.AddComponent<UiAlwaysOnTop>();
                SerializedObject created = new SerializedObject(replacement);
                created.FindProperty("renderQueue").intValue = renderQueue;
                created.FindProperty("canvasSortingOrder").intValue = sortingOrder;
                created.FindProperty("reapplyOnEnable").boolValue = reapply;
                created.ApplyModifiedPropertiesWithoutUndo();
                return;
            }

            if (canvasObject.GetComponent<UiAlwaysOnTop>() == null) {
                canvasObject.AddComponent<UiAlwaysOnTop>();
            }
        }

        /// <summary>
        /// The studio lighting of the PCB exhibits, without shadows: warm key light, cool fill and rim, a tri-light ambient and the studio
        /// reflection cubemap (copied from the PCB project) so the glossy parts of the turntable model catch soft highlights.
        /// </summary>
        private static void ApplyLighting(Scene scene) {
            SetLight(scene, "Key Light", new Color(1f, 0.94f, 0.84f, 1f), 1.4f, LightShadows.None);
            SetLight(scene, "Fill Light", new Color(0.62f, 0.74f, 1f, 1f), 0.4f, LightShadows.None);
            SetLight(scene, "Rim Light", new Color(0.55f, 0.72f, 1f, 1f), 1.2f, LightShadows.None);
            Light key = FindRoot(scene, "Key Light").GetComponent<Light>();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.34f, 0.40f, 0.52f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.22f, 0.25f, 0.31f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.10f, 0.12f, 0.11f, 1f);
            RenderSettings.skybox = null;
            RenderSettings.sun = key;

            Cubemap studio = EnsureStudioReflection();
            if (studio != null) {
                RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
                RenderSettings.customReflectionTexture = studio;
                RenderSettings.reflectionIntensity = 1f;
            }

            Camera[] cameras = FindRoot(scene, "XRRig").GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < cameras.Length; i++) {
                cameras[i].farClipPlane = CameraFarPlane;
            }
        }

        private static Cubemap EnsureStudioReflection() {
            Cubemap existing = AssetDatabase.LoadAssetAtPath<Cubemap>(StudioReflectionPath);
            if (existing != null) {
                return existing;
            }

            EnsureFolder(MaterialsFolder);
            if (!AssetDatabase.CopyAsset(SourceStudioReflectionPath, StudioReflectionPath)) {
                Debug.LogError($"[Launcher Builder] The studio reflection '{SourceStudioReflectionPath}' is missing; run KMax/PCB Assembly/Apply Studio Lighting first.");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Cubemap>(StudioReflectionPath);
        }

        /// <summary>A large, very soft glow behind the turntable so the model stands out against a lit patch of backdrop.</summary>
        private static void BuildBackdropGlow(Scene scene, Sprite dust) {
            if (HasRoot(scene, GlowName)) {
                UnityEngine.Object.DestroyImmediate(FindRoot(scene, GlowName));
            }

            Shader shader = Shader.Find("KMaxLauncher/SoftGlow");
            if (shader == null) {
                Debug.LogError("[Launcher Builder] Shader 'KMaxLauncher/SoftGlow' was not found; no backdrop glow made.");
                return;
            }

            EnsureFolder(MaterialsFolder);
            string path = MaterialsFolder + "/LauncherBackdropGlow.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetTexture("_MainTex", dust.texture);
            material.SetColor("_Color", new Color(0.025f, 0.05f, 0.085f, 1f));
            EditorUtility.SetDirty(material);

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = GlowName;
            UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
            SceneManager.MoveGameObjectToScene(quad, scene);
            quad.transform.position = new Vector3(0f, 0.01f, GlowDepth);
            quad.transform.localScale = new Vector3(1.4f, 0.85f, 1f);
            MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static void SetLight(Scene scene, string lightName, Color color, float intensity, LightShadows shadows) {
            Light light = FindRoot(scene, lightName).GetComponent<Light>();
            light.color = color;
            light.intensity = intensity;
            light.shadows = shadows;
        }

        private static void BuildHeader(RectTransform canvas, out TMP_Text title, out TMP_Text subtitle) {
            RectTransform header = CreateRect("Header", canvas);
            Anchor(header, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -TitleTop), new Vector2(1200f, 80f));

            title = CreateText("Title", header, TitleFontSize, FontStyles.Bold, Color.white, TextAlignmentOptions.Top);
            title.characterSpacing = TitleLetterSpacing;
            title.text = LauncherTitle;
            Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 40f));

            subtitle = CreateText("Subtitle", header, SubtitleFontSize, FontStyles.Normal, AccentBright, TextAlignmentOptions.Top);
            subtitle.text = "";
            Anchor(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(0f, 28f));
        }

        private static LauncherCard[] BuildCards(RectTransform canvas, Sprite fill, Sprite border, out GameObject[] rows) {
            float width = cardsPerRow * CardWidth + (cardsPerRow - 1) * CardSpacing;
            float height = RowCount * CardHeight + (RowCount - 1) * CardSpacing + LoadHeight + CardSpacing;
            RectTransform container = CreateRect("Cards", canvas);
            Anchor(container, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, CardsBottom), new Vector2(width, height));
            VerticalLayoutGroup stack = container.gameObject.AddComponent<VerticalLayoutGroup>();
            stack.spacing = CardSpacing;
            stack.childAlignment = TextAnchor.LowerCenter;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = false;
            stack.childForceExpandHeight = false;

            rows = new GameObject[RowCount];
            LauncherCard[] cards = new LauncherCard[RowCount * cardsPerRow];
            for (int row = 0; row < RowCount; row++) {
                RectTransform rowRect = CreateRect("Row" + (row + 1), container);
                rows[row] = rowRect.gameObject;
                LayoutElement rowElement = rowRect.gameObject.AddComponent<LayoutElement>();
                rowElement.preferredHeight = CardHeight;
                HorizontalLayoutGroup line = rowRect.gameObject.AddComponent<HorizontalLayoutGroup>();
                line.spacing = CardSpacing;
                line.childAlignment = TextAnchor.MiddleCenter;
                line.childControlWidth = true;
                line.childControlHeight = true;
                line.childForceExpandWidth = false;
                line.childForceExpandHeight = false;

                for (int column = 0; column < cardsPerRow; column++) {
                    int number = row * cardsPerRow + column + 1;
                    cards[number - 1] = BuildCard(rowRect, fill, border, number);
                }
            }

            return cards;
        }

        private static LauncherCard BuildCard(RectTransform row, Sprite fill, Sprite border, int number) {
            RectTransform slot = CreateRect("Card" + number + "Slot", row);
            LayoutElement element = slot.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = CardWidth;
            element.preferredHeight = CardHeight;

            GameObject host;
            Image outline;
            UiButtonMotion motion;
            TMP_Text label;
            CreateGlassButton(slot, "Card" + number, fill, border, CardRestFill, CardRestOutline, CardHover, LabelFontSize, LabelRest, out host, out outline, out motion, out label);
            label.enableAutoSizing = true;
            label.fontSizeMin = 10f;
            label.fontSizeMax = LabelFontSize;
            Anchor(label.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(-12f, LabelHeight));

            RectTransform hostRect = (RectTransform)host.transform;
            Image thumbnail = CreateImage("Thumbnail", hostRect, null, Color.white);
            Anchor(thumbnail.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -ThumbnailMargin), new Vector2(ThumbnailWidth, ThumbnailHeight));
            thumbnail.preserveAspect = false;

            Image selectBar = CreateImage("SelectBar", hostRect, null, Accent);
            Anchor(selectBar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -1f), new Vector2(-2f * SelectBarInset, SelectBarHeight));
            selectBar.enabled = false;
            label.transform.SetAsLastSibling();

            LauncherCard card = host.AddComponent<LauncherCard>();
            SerializedObject serialized = new SerializedObject(card);
            SetReference(serialized, "slot", slot.gameObject);
            SetReference(serialized, "button", host.GetComponent<Button>());
            SetReference(serialized, "motion", motion);
            SetReference(serialized, "thumbnail", thumbnail);
            SetReference(serialized, "label", label);
            SetReference(serialized, "selectBar", selectBar);
            SetReference(serialized, "outline", outline);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return card;
        }

        /// <summary>The Load pill is the first child of the cards stack, so it sits between the turning model and the rows of cards that are shown.</summary>
        private static void BuildLoad(RectTransform cards, Sprite fill, Sprite border, out Button button, out TMP_Text label) {
            RectTransform slot = CreateRect("LoadSlot", cards);
            slot.SetAsFirstSibling();
            LayoutElement slotElement = slot.gameObject.AddComponent<LayoutElement>();
            slotElement.preferredHeight = LoadHeight;

            GameObject host;
            Image outline;
            UiButtonMotion motion;
            CreateGlassButton(slot, "Load", fill, border, LoadFill, AccentBright, LoadHover, LoadFontSize, Color.white, out host, out outline, out motion, out label);
            label.text = "Load";
            label.enableAutoSizing = true;
            label.fontSizeMin = 12f;
            label.fontSizeMax = LoadFontSize;
            Anchor(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-24f, 0f));
            button = host.GetComponent<Button>();
            BackButtonLayout.FitLoadWidth(slot, true);
        }

        /// <summary>The Back button of a topic's home screen: 150 x 42, bottom left, opens the launcher through the loading screen (Rules.md R-7).</summary>
        private static void BuildHomeButton(RectTransform canvas, Sprite fill, Sprite border) {
            RectTransform slot = CreateRect("HomeSlot", canvas);
            Anchor(slot, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(HomeMargin, HomeBottom), new Vector2(HomeWidth, HomeHeight));

            GameObject host;
            Image outline;
            UiButtonMotion motion;
            TMP_Text label;
            CreateGlassButton(slot, "Home", fill, border, CardRestFill, CardRestOutline, CardHoverNormal, HomeFontSize, Color.white, out host, out outline, out motion, out label);
            label.text = "Back";
            label.enableAutoSizing = true;
            label.fontSizeMin = 12f;
            label.fontSizeMax = HomeFontSize;
            Anchor(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-20f, 0f));
            host.AddComponent<LauncherHomeButton>();
        }

        /// <summary>The glass button of the anatomy exhibit: sliced fill, outline, bold label, spring motion and sound.</summary>
        private static void CreateGlassButton(RectTransform parent, string objectName, Sprite fill, Sprite border, Color fillColor, Color outlineColor, Color hoverTint, float fontSize, Color labelColor, out GameObject host, out Image outline, out UiButtonMotion motion, out TMP_Text label) {
            host = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform hostRect = (RectTransform)host.transform;
            hostRect.SetParent(parent, false);
            Anchor(hostRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            Image background = host.GetComponent<Image>();
            background.sprite = fill;
            background.type = Image.Type.Sliced;
            background.color = fillColor;
            Button button = host.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = background;

            outline = CreateImage("Outline", hostRect, border, outlineColor);
            outline.type = Image.Type.Sliced;
            Anchor(outline.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            label = CreateText("Label", hostRect, fontSize, FontStyles.Bold, labelColor, TextAlignmentOptions.Center);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            Anchor(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            motion = host.AddComponent<UiButtonMotion>();
            SerializedObject serialized = new SerializedObject(motion);
            serialized.FindProperty("hoverScale").floatValue = 1.04f;
            serialized.FindProperty("pressScale").floatValue = 0.94f;
            serialized.FindProperty("springFrequency").floatValue = 18f;
            serialized.FindProperty("springDamping").floatValue = 1.05f;
            serialized.FindProperty("pressDepthMillimetres").floatValue = 2f;
            serialized.FindProperty("hoverLift").floatValue = 0f;
            serialized.FindProperty("tintTarget").objectReferenceValue = background;
            serialized.FindProperty("hoverTint").colorValue = hoverTint;
            serialized.FindProperty("tintBlendTime").floatValue = 0.1f;
            serialized.FindProperty("pressFlash").boolValue = true;
            serialized.FindProperty("pressFlashTint").colorValue = new Color(0.133f, 0.827f, 0.933f, 1f);
            serialized.FindProperty("pressFlashDecay").floatValue = 0.26f;
            serialized.FindProperty("idlePulse").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            host.AddComponent<UiButtonSound>();
        }

        private static RectTransform CreateRect(string objectName, Transform parent) {
            GameObject created = new GameObject(objectName, typeof(RectTransform));
            RectTransform rect = (RectTransform)created.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image CreateImage(string objectName, RectTransform parent, Sprite sprite, Color color) {
            GameObject created = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            created.transform.SetParent(parent, false);
            Image image = created.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void BuildVersionLabel(RectTransform canvas) {
            TMP_Text version = CreateText("VersionLabel", canvas, VersionFontSize, FontStyles.Normal, Muted, TextAlignmentOptions.BottomLeft);
            Anchor((RectTransform)version.transform, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(HomeMargin, 24f), new Vector2(400f, 24f));
            version.textWrappingMode = TextWrappingModes.NoWrap;
            version.gameObject.AddComponent<VersionLabel>();
        }

        private static TMP_Text CreateText(string objectName, RectTransform parent, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment) {
            GameObject created = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            created.transform.SetParent(parent, false);
            TextMeshProUGUI text = created.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta) {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static GameObject FindRoot(Scene scene, string rootName) {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++) {
                if (roots[i].name == rootName) {
                    return roots[i];
                }
            }

            throw new InvalidOperationException($"[Launcher Builder] Scene '{scene.path}' has no root object '{rootName}'.");
        }

        private static void SetReference(SerializedObject serialized, string propertyName, UnityEngine.Object target) {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null) {
                throw new InvalidOperationException($"[Launcher Builder] No serialized field '{propertyName}' on {serialized.targetObject.GetType().Name}.");
            }

            property.objectReferenceValue = target;
        }

        private static void SetReferences(SerializedObject serialized, string propertyName, UnityEngine.Object[] targets) {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null) {
                throw new InvalidOperationException($"[Launcher Builder] No serialized field '{propertyName}' on {serialized.targetObject.GetType().Name}.");
            }

            property.arraySize = targets.Length;
            for (int i = 0; i < targets.Length; i++) {
                property.GetArrayElementAtIndex(i).objectReferenceValue = targets[i];
            }
        }

        private static void EnsureFolder(string assetFolder) {
            string[] parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++) {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static string ToAbsolute(string assetPath) {
            return Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);
        }

        private class ProjectDefaults {
            public ProjectDefaults(string label, string subtitle, string scenePath, string previewPrefabPath, string thumbnailSourcePath, bool hidden) : this(label, subtitle, scenePath, previewPrefabPath, thumbnailSourcePath) {
                Hidden = hidden;
            }

            public ProjectDefaults(string label, string subtitle, string scenePath, string previewPrefabPath, string thumbnailSourcePath, string launchKey, string launchValue) : this(label, subtitle, scenePath, previewPrefabPath, thumbnailSourcePath) {
                LaunchKey = launchKey;
                LaunchValue = launchValue;
            }

            public ProjectDefaults(string label, string subtitle, string scenePath, string previewPrefabPath, string thumbnailSourcePath) {
                Label = label;
                Subtitle = subtitle;
                ScenePath = scenePath;
                PreviewPrefabPath = previewPrefabPath;
                ThumbnailSourcePath = thumbnailSourcePath;
            }

            public string Label { get; private set; }

            public string LaunchKey { get; private set; }

            public string LaunchValue { get; private set; }

            public bool Hidden { get; private set; }

            public string Subtitle { get; private set; }

            public string ScenePath { get; private set; }

            public string PreviewPrefabPath { get; private set; }

            public string ThumbnailSourcePath { get; private set; }
        }
    }
}
