using System.IO;
using UnityEditor;
using UnityEngine;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Redraws the glass fill and outline sprites of every project at four times the resolution (256 px instead of 64) so rounded
    /// corners stay smooth when the display scales the interface up. The size on screen does not change: the pixels per unit and the
    /// nine-slice border grow by the same factor, so one canvas unit is still one unit of outline and a 12 unit corner.
    /// </summary>
    public static class GlassSpriteUpgrader {
        private const int Scale = 4;
        private const int PanelSize = 64 * Scale;
        private const float CornerRadius = 12f * Scale;
        private const float OutlineWidth = 1.5f * Scale;
        private const float Border = 16f * Scale;
        private const float PixelsPerUnit = 100f * Scale;

        private static readonly string[] Folders = new string[] {
            "Assets/Games/kmax-human-anatomy/Content/Textures",
            "Assets/Games/KMax-launcher-unity/UI/Sprites",
            "Assets/Games/kmax-display-example/Materials/UI",
            "Assets/Games/KMaxPCBAssembly/UI/Sprites",
            "Assets/Games/kmax-automobile-engine/UI/Sprites"
        };

        [MenuItem("KMax/Launcher/Upgrade Glass Sprites")]
        public static void Run() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[Glass] Do not run in play mode.");
                return;
            }

            byte[] fill = Render(false);
            byte[] outline = Render(true);
            int written = 0;
            for (int i = 0; i < Folders.Length; i++) {
                written += Write(Folders[i] + "/GlassFill.png", fill);
                written += Write(Folders[i] + "/GlassBorder.png", outline);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Glass] Upgraded " + written + " glass sprites to " + PanelSize + " px.");
        }

        private static int Write(string path, byte[] png) {
            if (!File.Exists(path)) {
                Debug.LogError("[Glass] Missing " + path);
                return 0;
            }

            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.spriteBorder = new Vector4(Border, Border, Border, Border);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 512;
            TextureImporterPlatformSettings standalone = importer.GetPlatformTextureSettings("Standalone");
            standalone.overridden = false;
            importer.SetPlatformTextureSettings(standalone);
            importer.SaveAndReimport();
            return 1;
        }

        private static byte[] Render(bool outlineOnly) {
            Texture2D texture = new Texture2D(PanelSize, PanelSize, TextureFormat.RGBA32, false);
            float centre = (PanelSize - 1) * 0.5f;
            float inner = PanelSize * 0.5f - Scale - CornerRadius;
            Color[] pixels = new Color[PanelSize * PanelSize];
            for (int y = 0; y < PanelSize; y++) {
                for (int x = 0; x < PanelSize; x++) {
                    float qx = Mathf.Abs(x - centre) - inner;
                    float qy = Mathf.Abs(y - centre) - inner;
                    float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                    float distance = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - CornerRadius;
                    float alpha = Mathf.Clamp01(0.5f - distance);
                    if (outlineOnly) {
                        alpha *= Mathf.Clamp01(distance + OutlineWidth + 0.5f);
                    }

                    pixels[y * PanelSize + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
            return png;
        }
    }
}
