using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Makes every glass panel's fill and outline the same shape. A panel is two sliced images, the rounded fill and an outline child
    /// laid over it; they only line up when both use the same pixels-per-unit multiplier and the outline covers the fill exactly.
    /// Restyled buttons in the display-example scenes kept the multiplier (1.25, 1.15, 0.68) of their old pill sprite on the fill, so the
    /// fill's corners were drawn at a different size from the outline's. Safe to run again after any rebuild.
    /// </summary>
    public static class GlassAlignFixer {
        private const string FillName = "GlassFill";
        private const string BorderName = "GlassBorder";

        [MenuItem("KMax/Launcher/Align Glass Panels")]
        public static void Run() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[Glass Align] Do not run in play mode.");
                return;
            }

            StringBuilder report = new StringBuilder();
            int scenesChanged = 0;
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes) {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                int fixedCount = 0;
                Image[] images = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < images.Length; i++) {
                    fixedCount += Align(images[i]);
                }

                if (fixedCount > 0) {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    scenesChanged++;
                    report.AppendLine("  " + System.IO.Path.GetFileName(entry.path) + ": " + fixedCount + " fixes");
                }
            }

            Debug.Log("[Glass Align] " + scenesChanged + " scenes changed.\n" + report);
        }

        private static int Align(Image image) {
            if (image.sprite == null) {
                return 0;
            }

            string spriteName = image.sprite.name;
            if (spriteName != FillName && spriteName != BorderName) {
                return 0;
            }

            int changes = 0;
            if (!Mathf.Approximately(image.pixelsPerUnitMultiplier, 1f)) {
                image.pixelsPerUnitMultiplier = 1f;
                EditorUtility.SetDirty(image);
                changes++;
            }

            if (spriteName == BorderName) {
                RectTransform rect = (RectTransform)image.transform;
                if (rect.anchorMin != Vector2.zero || rect.anchorMax != Vector2.one || rect.offsetMin != Vector2.zero || rect.offsetMax != Vector2.zero) {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    EditorUtility.SetDirty(rect);
                    changes++;
                }
            }

            return changes;
        }
    }
}
