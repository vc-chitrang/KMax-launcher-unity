using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Renders the card picture of each Retail category (one furniture design of the category on the launcher's dark background) into
    /// <c>kmax-retail-products/UI/Thumbnails</c>. A picture is only drawn when its file is missing, so rebuilding the launcher screens
    /// leaves the pictures alone; delete a file to have it drawn again.
    /// </summary>
    public static class RetailCategoryThumbnails {
        public const string Folder = "Assets/Games/kmax-retail-products/UI/Thumbnails";

        private const int IsolatedLayer = 31;
        private const int Width = 512;
        private const int Height = 288;
        private const float FieldOfView = 24f;
        private const float Margin = 0.8f;

        public static string PathFor(string label) {
            return Folder + "/Category_" + label.Replace(' ', '_') + ".png";
        }

        /// <summary>Makes the picture for <paramref name="label"/> from <paramref name="prefabPath"/> if its file does not exist yet.</summary>
        public static bool Ensure(string label, string prefabPath) {
            return EnsureAt(PathFor(label), prefabPath);
        }

        /// <summary>Makes the picture at <paramref name="target"/> from <paramref name="prefabPath"/> if its file does not exist yet.</summary>
        public static bool EnsureAt(string target, string prefabPath, float margin = Margin) {
            if (File.Exists(target)) {
                return true;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) {
                Debug.LogError("[Retail Thumbnails] Missing prefab " + prefabPath);
                return false;
            }

            UnityEngine.SceneManagement.Scene scratch = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            RenderTexture texture = null;
            GameObject cameraObject = null;
            GameObject lightObject = null;
            GameObject instance = null;
            try {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scratch);
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.Euler(0f, 215f, 0f);
                SetLayer(instance.transform);
                Bounds bounds = Measure(instance);

                lightObject = new GameObject("Light");
                SceneManagementMove(lightObject, scratch);
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.shadows = LightShadows.None;
                light.cullingMask = 1 << IsolatedLayer;
                lightObject.layer = IsolatedLayer;
                lightObject.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

                cameraObject = new GameObject("Camera");
                SceneManagementMove(cameraObject, scratch);
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.06f, 0.09f, 0.165f, 1f);
                camera.fieldOfView = FieldOfView;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 50f;
                camera.cullingMask = 1 << IsolatedLayer;

                float halfVertical = FieldOfView * 0.5f * Mathf.Deg2Rad;
                float halfHorizontal = Mathf.Atan(Mathf.Tan(halfVertical) * Width / Height);
                float radius = bounds.extents.magnitude;
                float distance = Mathf.Max(radius / Mathf.Sin(Mathf.Min(halfVertical, halfHorizontal)), 0.2f) * margin;
                cameraObject.transform.position = bounds.center + new Vector3(0f, radius * 0.45f, -distance);
                cameraObject.transform.LookAt(bounds.center);

                texture = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
                texture.antiAliasing = 4;
                camera.targetTexture = texture;
                camera.Render();

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = texture;
                Texture2D readable = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                readable.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                readable.Apply();
                RenderTexture.active = previous;
                camera.targetTexture = null;
                byte[] png = readable.EncodeToPNG();
                Object.DestroyImmediate(readable);

                Directory.CreateDirectory(Path.GetDirectoryName(target));
                File.WriteAllBytes(target, png);
                AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
                return true;
            } finally {
                if (texture != null) {
                    texture.Release();
                    Object.DestroyImmediate(texture);
                }

                EditorSceneManager.CloseScene(scratch, true);
            }
        }

        private static void SetLayer(Transform root) {
            root.gameObject.layer = IsolatedLayer;
            for (int i = 0; i < root.childCount; i++) {
                SetLayer(root.GetChild(i));
            }
        }

        private static void SceneManagementMove(GameObject target, UnityEngine.SceneManagement.Scene scene) {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(target, scene);
        }

        private static Bounds Measure(GameObject root) {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds(root.transform.position, Vector3.zero);
            bool first = true;
            for (int i = 0; i < renderers.Length; i++) {
                if (first) {
                    bounds = renderers[i].bounds;
                    first = false;
                } else {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return bounds;
        }
    }
}
