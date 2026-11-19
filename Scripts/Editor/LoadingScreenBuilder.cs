using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Builds Resources/KMaxLoadingScreen.prefab, the single loading screen of the application (see <see cref="LoadingScreen"/>):
    /// a full-screen overlay canvas with the slate backdrop of the home screens, one of the Animated Loading Icons rings in
    /// bright cyan, and the text "Loading..." in the interface's title type (white, bold). Re-run to rebuild; it is idempotent.
    /// </summary>
    public static class LoadingScreenBuilder {
        private const string ResourcesFolder = "Assets/Games/KMax-launcher-unity/Resources";
        private const string PrefabPath = ResourcesFolder + "/KMaxLoadingScreen.prefab";
        private const string SpinnerPrefabPath = "Assets/Plugins/Animated Loading Icons/Prefabs/Circle open rotating/Circle open rotating 1.prefab";
        private const int SortingOrder = 30000;
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float ScreenWidthMetres = 0.35f;
        private const float BackdropOverscan = 2.2f;
        private const float SpinnerSize = 64f;
        private const float SpinnerOffset = 22f;
        private const float LabelOffset = -40f;
        private const float LabelFontSize = 19f;
        private const float LabelLetterSpacing = 1f;

        private static readonly Color Backdrop = new Color(0.145f, 0.165f, 0.21f, 1f);
        private static readonly Color AccentBright = new Color32(34, 211, 238, 255);

        [MenuItem("KMax/Launcher/Build Loading Screen")]
        public static void Build() {
            GameObject spinnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpinnerPrefabPath);
            if (spinnerPrefab == null) {
                Debug.LogError($"[Loading Screen] The loading icon prefab '{SpinnerPrefabPath}' is missing.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(ResourcesFolder)) {
                AssetDatabase.CreateFolder("Assets/Games/KMax-launcher-unity", "Resources");
            }

            GameObject root = new GameObject("KMaxLoadingScreen", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = SortingOrder;
            RectTransform rootRect = (RectTransform)root.transform;
            rootRect.sizeDelta = new Vector2(ReferenceWidth, ReferenceHeight);
            rootRect.localScale = Vector3.one * (ScreenWidthMetres / ReferenceWidth);
            CanvasGroup group = root.GetComponent<CanvasGroup>();

            Image backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            backdrop.transform.SetParent(root.transform, false);
            RectTransform backdropRect = (RectTransform)backdrop.transform;
            backdropRect.anchorMin = new Vector2(0.5f, 0.5f);
            backdropRect.anchorMax = new Vector2(0.5f, 0.5f);
            backdropRect.pivot = new Vector2(0.5f, 0.5f);
            backdropRect.anchoredPosition = Vector2.zero;
            backdropRect.sizeDelta = new Vector2(ReferenceWidth * BackdropOverscan, ReferenceHeight * BackdropOverscan);
            backdrop.color = Backdrop;
            backdrop.raycastTarget = true;

            GameObject spinner = (GameObject)PrefabUtility.InstantiatePrefab(spinnerPrefab);
            spinner.name = "Spinner";
            spinner.transform.SetParent(root.transform, false);
            RectTransform spinnerRect = (RectTransform)spinner.transform;
            spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            spinnerRect.pivot = new Vector2(0.5f, 0.5f);
            spinnerRect.anchoredPosition = new Vector2(0f, SpinnerOffset);
            spinnerRect.sizeDelta = new Vector2(SpinnerSize, SpinnerSize);
            Image[] spinnerImages = spinner.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < spinnerImages.Length; i++) {
                spinnerImages[i].color = AccentBright;
                spinnerImages[i].raycastTarget = false;
                spinnerImages[i].preserveAspect = true;
                RectTransform imageRect = (RectTransform)spinnerImages[i].transform;
                if (imageRect != spinnerRect) {
                    imageRect.anchorMin = Vector2.zero;
                    imageRect.anchorMax = Vector2.one;
                    imageRect.offsetMin = Vector2.zero;
                    imageRect.offsetMax = Vector2.zero;
                }
            }

            TextMeshProUGUI label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(root.transform, false);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0f, LabelOffset);
            labelRect.sizeDelta = new Vector2(400f, 36f);
            label.font = TMP_Settings.defaultFontAsset;
            label.text = "Loading...";
            label.fontSize = LabelFontSize;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.characterSpacing = LabelLetterSpacing;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;

            root.AddComponent<Ui.UiAlwaysOnTop>();
            SerializedObject onTop = new SerializedObject(root.GetComponent<Ui.UiAlwaysOnTop>());
            onTop.FindProperty("canvasSortingOrder").intValue = SortingOrder;
            onTop.ApplyModifiedPropertiesWithoutUndo();

            LoadingScreen screen = root.AddComponent<LoadingScreen>();
            SerializedObject serialized = new SerializedObject(screen);
            serialized.FindProperty("canvas").objectReferenceValue = canvas;
            serialized.FindProperty("group").objectReferenceValue = group;
            serialized.FindProperty("label").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Loading Screen] Built {PrefabPath}.");
        }
    }
}
