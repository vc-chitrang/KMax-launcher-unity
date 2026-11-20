using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ViitorCloud.KmaxEducation.Editor;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Synchronizes all education assets for the launcher:
    /// - Rebuilds MonaLisaFrame with visible portrait and generates new rounded Arts thumbnail.
    /// - Rebuilds SolarSystemPreview prefab with animated orbits and glowing rings, and generates rounded SolarSystem thumbnail.
    /// - Rounds corners for Chemistry and Solar System card thumbnails.
    /// - Updates EducationCatalog.asset to rename Geography to Solar System and bind the updated preview model and thumbnail.
    /// - Rebuilds ArtPainting scene to wire Explore more to wrap back to the first painting without touching the car scene.
    /// </summary>
    public static class EducationLauncherSync {
        private const string Root = "Assets/Games/kmax-education";
        private const string CatalogPath = "Assets/Games/KMax-launcher-unity/Data/EducationCatalog.asset";
        private const string ArtsThumbPath = Root + "/UI/Thumbnails/Arts.png";
        private const string SolarThumbPath = Root + "/UI/Thumbnails/SolarSystem.png";
        private const string GeographyThumbPath = Root + "/UI/Thumbnails/Geography.png";
        private const string ChemistryThumbPath = Root + "/UI/Thumbnails/Chemistry.png";
        private const string MonaLisaFramePath = Root + "/Prefabs/MonaLisaFrame.prefab";
        private const string SolarPreviewPath = Root + "/Prefabs/SolarSystemPreview.prefab";

        [MenuItem("KMax/Education/Sync Launcher Thumbnails and Models")]
        public static void SyncAll() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[EducationSync] Do not run in play mode.");
                return;
            }

            Debug.Log("[EducationSync] Step 1: Rebuilding Mona Lisa prefabs with painting visible...");
            MonaLisaPrefabBuilder.Build();

            Debug.Log("[EducationSync] Step 2: Rebuilding Solar System Prefab with rotated planet bodies...");
            SolarSystemSceneBuilder.BuildPrefab();

            Debug.Log("[EducationSync] Step 3: Building Solar System Preview prefab with orbits and rings...");
            SolarSystemPreviewBuilder.Build();

            Debug.Log("[EducationSync] Step 4: Rebuilding ArtPainting scene with Explore More loop...");
            ArtSceneBuilder.Build();

            Debug.Log("[EducationSync] Step 5: Rebuilding SolarSystem scene and Chemistry scene...");
            SolarSystemSceneBuilder.BuildScene();
            ChemistrySceneBuilder.Build();
            EducationAnchors.Run();

            Debug.Log("[EducationSync] Step 6: Regenerating Arts and Solar System thumbnails...");
            if (File.Exists(ArtsThumbPath)) {
                File.Delete(ArtsThumbPath);
                string meta = ArtsThumbPath + ".meta";
                if (File.Exists(meta)) {
                    File.Delete(meta);
                }
            }

            if (File.Exists(SolarThumbPath)) {
                File.Delete(SolarThumbPath);
                string meta = SolarThumbPath + ".meta";
                if (File.Exists(meta)) {
                    File.Delete(meta);
                }
            }

            AssetDatabase.Refresh();

            RetailCategoryThumbnails.EnsureAt(ArtsThumbPath, MonaLisaFramePath, 0.7f);
            RetailCategoryThumbnails.EnsureAt(SolarThumbPath, SolarPreviewPath, 0.9f);

            Debug.Log("[EducationSync] Step 7: Rounding thumbnail corners for Chemistry, Solar System, and Arts...");
            ThumbnailStyler.Run();

            Debug.Log("[EducationSync] Step 8: Updating EducationCatalog asset...");
            UpdateCatalog();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EducationSync] Complete! All thumbnails, models, and scenes synchronized.");
        }

        private static void UpdateCatalog() {
            LauncherCatalog catalog = AssetDatabase.LoadAssetAtPath<LauncherCatalog>(CatalogPath);
            if (catalog == null) {
                Debug.LogError($"[EducationSync] Could not load catalog at '{CatalogPath}'.");
                return;
            }

            SerializedObject serialized = new SerializedObject(catalog);
            SerializedProperty entries = serialized.FindProperty("entries");

            Sprite artsSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtsThumbPath);
            Sprite solarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SolarThumbPath);
            GameObject solarPreview = AssetDatabase.LoadAssetAtPath<GameObject>(SolarPreviewPath);
            GameObject monaLisaFrame = AssetDatabase.LoadAssetAtPath<GameObject>(MonaLisaFramePath);

            for (int i = 0; i < entries.arraySize; i++) {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                SerializedProperty label = entry.FindPropertyRelative("label");
                string labelText = label.stringValue;

                if (labelText == "Arts") {
                    if (artsSprite != null) {
                        entry.FindPropertyRelative("thumbnail").objectReferenceValue = artsSprite;
                    }
                    if (monaLisaFrame != null) {
                        entry.FindPropertyRelative("previewPrefab").objectReferenceValue = monaLisaFrame;
                    }
                } else if (labelText == "Geography" || labelText == "Solar System") {
                    label.stringValue = "Solar System";
                    entry.FindPropertyRelative("subtitle").stringValue = "The solar system: planets in orbit, explained";
                    if (solarSprite != null) {
                        entry.FindPropertyRelative("thumbnail").objectReferenceValue = solarSprite;
                    }
                    if (solarPreview != null) {
                        entry.FindPropertyRelative("previewPrefab").objectReferenceValue = solarPreview;
                    }
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }
    }
}
