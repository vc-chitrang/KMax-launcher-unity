using System.IO;
using UnityEditor;
using UnityEngine;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Rounds the corners of every home-screen thumbnail picture (launcher, topic home screens, anatomy and PCB cards) by clearing the
    /// alpha outside a rounded rectangle. The corner radius is 8 canvas units at the size the picture is drawn (156 wide), so it is
    /// proportional to the picture's width. A picture whose corner is already clear is left alone, so this is safe to run again.
    /// </summary>
    public static class ThumbnailStyler {
        private const float RadiusPerWidth = 8f / 156f;
        private const float VariantRadiusPerWidth = 12f / 90f;
        private const string RetailCatalogPath = "Assets/Games/kmax-retail-products/Data/RetailCatalog.asset";
        private const string VariantOutputFolder = "Assets/Games/kmax-retail-products/UI/Variants";
        private const int VariantPixels = 256;
        private const int CornerProbe = 1;

        private static readonly string[] Folders = new string[] {
            "Assets/Games/KMax-launcher-unity/UI/Thumbnails",
            "Assets/Games/kmax-human-anatomy/Generated/Resources/Anatomy/Thumbs",
            "Assets/Games/KMaxPCBAssembly/UI/Thumbnails",
            "Assets/Games/kmax-display-example/Materials/Thumbnails",
            "Assets/Games/kmax-automobile-engine/UI/Thumbnails",
            "Assets/Games/kmax-retail-products/UI/Thumbnails"
        };

        [MenuItem("KMax/Launcher/Round Thumbnail Corners")]
        public static void Run() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[Thumbnails] Do not run in play mode.");
                return;
            }

            int rounded = 0;
            int skipped = 0;
            for (int f = 0; f < Folders.Length; f++) {
                if (!AssetDatabase.IsValidFolder(Folders[f])) {
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { Folders[f] });
                for (int i = 0; i < guids.Length; i++) {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (!path.EndsWith(".png")) {
                        continue;
                    }

                    if (Round(path, RadiusPerWidth)) {
                        rounded++;
                    } else {
                        skipped++;
                    }
                }
            }

            rounded += RoundRetailVariants();

            AssetDatabase.SaveAssets();
            Debug.Log("[Thumbnails] Rounded " + rounded + " pictures, " + skipped + " already round.");
        }

        /// <summary>
        /// The Retail variant pictures are the vendor's JPG files, which cannot carry a clear corner, so each gets a rounded PNG copy in
        /// <c>kmax-retail-products/UI/Variants</c> (256 px, corner 12 units at the 90 unit size it is drawn) and the catalog points to the copy.
        /// </summary>
        private static int RoundRetailVariants() {
            RetailCatalogHandle handle = new RetailCatalogHandle(RetailCatalogPath);
            if (!handle.IsValid) {
                return 0;
            }

            int made = 0;
            Directory.CreateDirectory(VariantOutputFolder);
            SerializedProperty categories = handle.Serialized.FindProperty("categories");
            for (int c = 0; c < categories.arraySize; c++) {
                SerializedProperty designs = categories.GetArrayElementAtIndex(c).FindPropertyRelative("designs");
                for (int d = 0; d < designs.arraySize; d++) {
                    SerializedProperty finishes = designs.GetArrayElementAtIndex(d).FindPropertyRelative("finishes");
                    for (int f = 0; f < finishes.arraySize; f++) {
                        SerializedProperty thumbnail = finishes.GetArrayElementAtIndex(f).FindPropertyRelative("thumbnail");
                        Object current = thumbnail.objectReferenceValue;
                        if (current == null) {
                            continue;
                        }

                        string sourcePath = AssetDatabase.GetAssetPath(current);
                        if (!sourcePath.EndsWith(".jpg")) {
                            continue;
                        }

                        string targetPath = VariantOutputFolder + "/" + Path.GetFileNameWithoutExtension(sourcePath) + ".png";
                        Sprite sprite = MakeVariant(sourcePath, targetPath);
                        if (sprite != null) {
                            thumbnail.objectReferenceValue = sprite;
                            made++;
                        }
                    }
                }
            }

            handle.Apply();
            return made;
        }

        private static Sprite MakeVariant(string sourcePath, string targetPath) {
            Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(File.ReadAllBytes(sourcePath));
            Texture2D target = new Texture2D(VariantPixels, VariantPixels, TextureFormat.RGBA32, false);
            float radius = VariantPixels * VariantRadiusPerWidth;
            Color[] pixels = new Color[VariantPixels * VariantPixels];
            for (int y = 0; y < VariantPixels; y++) {
                for (int x = 0; x < VariantPixels; x++) {
                    Color colour = source.GetPixelBilinear((x + 0.5f) / VariantPixels, (y + 0.5f) / VariantPixels);
                    float qx = Mathf.Abs(x + 0.5f - VariantPixels * 0.5f) - (VariantPixels * 0.5f - radius);
                    float qy = Mathf.Abs(y + 0.5f - VariantPixels * 0.5f) - (VariantPixels * 0.5f - radius);
                    float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                    float distance = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                    colour.a = Mathf.Clamp01(0.5f - distance);
                    pixels[y * VariantPixels + x] = colour;
                }
            }

            target.SetPixels(pixels);
            File.WriteAllBytes(targetPath, target.EncodeToPNG());
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(target);
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(targetPath);
            if (importer.textureType != TextureImporterType.Sprite || !importer.alphaIsTransparency || importer.mipmapEnabled) {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(targetPath);
        }

        private class RetailCatalogHandle {
            private readonly Object asset;

            public RetailCatalogHandle(string path) {
                asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null) {
                    Serialized = new SerializedObject(asset);
                }
            }

            public SerializedObject Serialized { get; private set; }

            public bool IsValid {
                get { return asset != null; }
            }

            public void Apply() {
                Serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
            }
        }

        private static bool Round(string path, float radiusPerWidth) {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(path));
            int width = texture.width;
            int height = texture.height;
            Color32[] pixels = texture.GetPixels32();
            if (pixels[CornerProbe * width + CornerProbe].a < 250) {
                Object.DestroyImmediate(texture);
                return false;
            }

            float radius = width * radiusPerWidth;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    float qx = Mathf.Abs(x + 0.5f - width * 0.5f) - (width * 0.5f - radius);
                    float qy = Mathf.Abs(y + 0.5f - height * 0.5f) - (height * 0.5f - radius);
                    float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                    float distance = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                    float coverage = Mathf.Clamp01(0.5f - distance);
                    if (coverage < 1f) {
                        Color32 pixel = pixels[y * width + x];
                        pixel.a = (byte)Mathf.RoundToInt(pixel.a * coverage);
                        pixels[y * width + x] = pixel;
                    }
                }
            }

            texture.SetPixels32(pixels);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && !importer.alphaIsTransparency) {
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return true;
        }
    }
}
