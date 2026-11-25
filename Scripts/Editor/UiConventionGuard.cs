using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Regression guard (docs/regressions.md). The fast check reads source and scene text and runs before every player build, failing it on
    /// a violation; the full check (menu) also opens every scene. Each rule is one fixed issue that must never come back.
    /// </summary>
    public class UiConventionGuard : IPreprocessBuildWithReport {
        private const string GamesFolder = "Assets/Games";
        private const string KitRuntimeFolder = "Assets/Plugins/KmaxVxSDK/Kit/Runtime";
        private const string SolarRuntimeFolder = "Assets/Games/kmax-solar-system/Scripts/Runtime";
        private static readonly string[] CarScenes = new string[] { "VolvoS90.unity", "VolvoC40.unity", "FordMustangGT.unity" };
        private static readonly Regex WayOutName = new Regex("m_Name: (Home|HomeSlot|HomeButton|BackToHomeButton|MenuButton)\\s*$", RegexOptions.Multiline);

        public int callbackOrder {
            get {
                return -100;
            }
        }

        public void OnPreprocessBuild(BuildReport report) {
            List<string> errors = new List<string>();
            RunFastChecks(errors);
            if (errors.Count > 0) {
                string message = "UiConventionGuard found " + errors.Count + " violation(s). Fix them (docs/regressions.md) or the build is refused:\n" + string.Join("\n", errors);
                throw new BuildFailedException(message);
            }
        }

        [MenuItem("KMax/QA/Check UI Conventions")]
        public static void RunFromMenu() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[UiGuard] Do not run in play mode.");
                return;
            }

            Scene active = SceneManager.GetActiveScene();
            if (active.isDirty) {
                Debug.LogError("[UiGuard] Save or discard the open scene first.");
                return;
            }

            string returnPath = active.path;
            List<string> errors = new List<string>();
            RunFastChecks(errors);
            RunSceneChecks(errors);
            if (!string.IsNullOrEmpty(returnPath)) {
                EditorSceneManager.OpenScene(returnPath, OpenSceneMode.Single);
            }

            if (errors.Count == 0) {
                Debug.Log("[UiGuard] PASS: no UI convention violations.");
            } else {
                Debug.LogError("[UiGuard] " + errors.Count + " violation(s):\n" + string.Join("\n", errors));
            }
        }

        private static void RunFastChecks(List<string> errors) {
            ScanCode(errors);
            ScanSceneText(errors);
        }

        private static void ScanCode(List<string> errors) {
            string[] files = Directory.GetFiles(GamesFolder, "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++) {
                string path = files[i].Replace('\\', '/');
                if (path.Contains("/Editor/") || path.Contains("/Tests/") || path.EndsWith("/SceneLoader.cs") || path.EndsWith("/LoadingScreen.cs")) {
                    continue;
                }

                string[] lines = File.ReadAllLines(path);
                for (int line = 0; line < lines.Length; line++) {
                    string text = lines[line].Trim();
                    if (text.StartsWith("//")) {
                        continue;
                    }
                    if (text.Contains("SceneManager.LoadScene(") || text.Contains("SceneManager.LoadSceneAsync(")) {
                        errors.Add("R-9 direct scene load (use SceneLoader.Load): " + path + ":" + (line + 1));
                    }
                }
            }

            ScanMainCamera(KitRuntimeFolder, errors);
            ScanMainCamera(SolarRuntimeFolder, errors);
        }

        private static void ScanMainCamera(string folder, List<string> errors) {
            if (!Directory.Exists(folder)) {
                return;
            }

            string[] files = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++) {
                string path = files[i].Replace('\\', '/');
                if (path.EndsWith("/KitPointer.cs")) {
                    continue;
                }

                string[] lines = File.ReadAllLines(path);
                for (int line = 0; line < lines.Length; line++) {
                    string text = lines[line].Trim();
                    if (!text.StartsWith("//") && text.Contains("Camera.main")) {
                        errors.Add("Camera.main is null on the Kmax rig (use KitPointer.ResolveCamera): " + path + ":" + (line + 1));
                    }
                }
            }
        }

        private static void ScanSceneText(List<string> errors) {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++) {
                if (!scenes[i].enabled) {
                    continue;
                }

                string path = scenes[i].path;
                if (path.Contains("/Showcase/") || path.EndsWith("KmaxLauncherScene.unity") || path.EndsWith("PresentationScene.unity") || path.EndsWith("PresentationEnvironment.unity") || path.EndsWith("PresentationScene_Backup.unity") || path.EndsWith("PresentationScene_Backup_Copy.unity") || path.EndsWith("PresentationScene_v01.unity") || !File.Exists(path)) {
                    continue;
                }

                string text = File.ReadAllText(path);
                if (!text.Contains("m_Name: EventSystem")) {
                    errors.Add("Scene has no EventSystem (no button, chip or slider would react): " + path);
                }

                if (!WayOutName.IsMatch(text)) {
                    errors.Add("R-7 scene has no way-out button (Back, bottom-left): " + path);
                }

                for (int c = 0; c < CarScenes.Length; c++) {
                    if (path.EndsWith("/" + CarScenes[c])) {
                        if (!text.Contains("m_Name: FinishPanel") || !text.Contains("m_Name: CaptionCard")) {
                            errors.Add("Car scene has the old UI (needs FinishPanel and CaptionCard): " + path);
                        }
                    }
                }
            }
        }

        private static void RunSceneChecks(List<string> errors) {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++) {
                if (!scenes[i].enabled || scenes[i].path.Contains("/Showcase/")) {
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(scenes[i].path, OpenSceneMode.Single);
                Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int c = 0; c < canvases.Length; c++) {
                    if (canvases[c].isRootCanvas && canvases[c].gameObject.activeSelf && canvases[c].renderMode != RenderMode.WorldSpace) {
                        errors.Add("Canvas '" + canvases[c].name + "' is " + canvases[c].renderMode + ", must be WorldSpace (stereo display): " + scenes[i].path);
                    }
                }

                MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int b = 0; b < behaviours.Length; b++) {
                    if (behaviours[b] == null || behaviours[b].GetType().Name != "UiButtonMotion") {
                        continue;
                    }

                    Transform parent = behaviours[b].transform.parent;
                    if (parent != null && parent.GetComponent<LayoutGroup>() != null) {
                        errors.Add("UiButtonMotion '" + behaviours[b].name + "' sits directly in layout group '" + parent.name + "' (wrap it in a slot, Design.md 7.1): " + scenes[i].path);
                    }
                }

                if (!scene.IsValid()) {
                    errors.Add("Scene could not be opened: " + scenes[i].path);
                }
            }

            string[] previewGuids = AssetDatabase.FindAssets("Preview t:Prefab", new string[] { GamesFolder });
            for (int p = 0; p < previewGuids.Length; p++) {
                string prefabPath = AssetDatabase.GUIDToAssetPath(previewGuids[p]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) {
                    continue;
                }

                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < renderers.Length; r++) {
                    Material[] materials = renderers[r].sharedMaterials;
                    for (int m = 0; m < materials.Length; m++) {
                        if (materials[m] == null) {
                            errors.Add("Preview prefab has a null (pink) material: " + prefabPath + " / " + renderers[r].name);
                            m = materials.Length;
                            r = renderers.Length;
                        }
                    }
                }
            }
        }
    }
}
