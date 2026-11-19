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
    /// Adds the stylus tutorial to every screen: a small round (i) button at the bottom right and, over it, a panel with a picture of the
    /// stylus. Each button of the stylus carries a number and a thin line to a short text saying what it does. Safe to run again (the
    /// tutorial of a screen is rebuilt); run it after rebuilding a scene.
    /// </summary>
    public static class StylusTutorialBuilder {
        private const string GamesFolder = "Assets/Games";
        private const string TutorialFolder = "Assets/Games/KMax-launcher-unity/UI/Tutorial";
        private const string StylusImagePath = TutorialFolder + "/StylusController.png";
        private const string CirclePath = TutorialFolder + "/Circle.png";
        private const string RingPath = TutorialFolder + "/Ring.png";
        private const string FillSpritePath = "Assets/Games/KMax-launcher-unity/UI/Sprites/GlassFill.png";
        private const string BorderSpritePath = "Assets/Games/KMax-launcher-unity/UI/Sprites/GlassBorder.png";
        private const string RootName = "TutorialInfo";
        private const string AlwaysOnTopName = "UiAlwaysOnTop";
        private const float ReferenceWidth = 1920f;
        private const float MinCanvasWidth = 1000f;
        private const int CircleSize = 128;
        private const int RingThickness = 5;

        private const float ButtonSize = 44f;
        private const float EdgeMargin = 32f;
        private const float PanelGap = 12f;
        private const float PanelScale = 0.82f;
        private const float PanelWidth = 860f;
        private const float PanelHeight = 455f;
        private const float ImageHeight = 360f;
        private const float ImageAspect = 925f / 1212f;
        private const float ImageTop = 70f;
        private const float MarkerSize = 24f;
        private const float LineWidth = 1.6f;
        private const float TextWidth = 235f;

        private static readonly string[] HomeHeadings = new string[] { "0. Zoom and click", "1. Click, Drag, Move", "2. Reset the view" };

        private static readonly string[] HomeBodies = new string[] {
            "Tap to press a button or choose a card. Inside an experience, hold and push the stylus away to zoom in, pull it back to zoom out.",
            "Tap to press a button, choose a card and tap Load. Dragging and turning work inside an experience.",
            "Tap to press a button. Inside an experience, tap on empty space to bring the view back to the start."
        };

        private static readonly Vector2 TopButton = new Vector2(0.517f, 0.529f);
        private static readonly Vector2 LeftButton = new Vector2(0.454f, 0.652f);
        private static readonly Vector2 RightButton = new Vector2(0.570f, 0.652f);

        private static readonly Color Cyan = new Color(0.133f, 0.827f, 0.933f, 1f);
        private static readonly Color Light = new Color(0.886f, 0.91f, 0.941f, 1f);
        private static readonly Color Muted = new Color(0.62f, 0.68f, 0.76f, 1f);
        private static readonly Color PanelFill = new Color(0.043f, 0.063f, 0.1f, 0.95f);
        private static readonly Color PanelBorder = new Color(0.133f, 0.827f, 0.933f, 0.45f);
        private static readonly Color ButtonFill = new Color(0.063f, 0.086f, 0.14f, 0.92f);
        private static readonly string[] SkippedScenes = new string[] { "/Showcase/", "kmax-display-example/Scenes/Launcher.unity", "/Modules/" };

        [MenuItem("KMax/Tutorial/Add Stylus Tutorial To All Screens")]
        public static void Run() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[Tutorial] Do not run in play mode.");
                return;
            }

            Scene active = SceneManager.GetActiveScene();
            if (active.isDirty) {
                Debug.LogError("[Tutorial] Save or discard the changes in the open scene first.");
                return;
            }

            EnsureSprites();
            Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
            Sprite ring = AssetDatabase.LoadAssetAtPath<Sprite>(RingPath);
            Sprite stylus = AssetDatabase.LoadAssetAtPath<Sprite>(StylusImagePath);
            Sprite fill = AssetDatabase.LoadAssetAtPath<Sprite>(FillSpritePath);
            Sprite border = AssetDatabase.LoadAssetAtPath<Sprite>(BorderSpritePath);
            if (circle == null || ring == null || stylus == null || fill == null || border == null) {
                Debug.LogError("[Tutorial] A sprite of the tutorial is missing.");
                return;
            }

            string returnPath = active.path;
            string[] guids = AssetDatabase.FindAssets("t:Scene", new string[] { GamesFolder });
            int total = 0;
            for (int g = 0; g < guids.Length; g++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[g]);
                if (IsSkipped(path)) {
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int here = 0;
                GameObject[] roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++) {
                    Canvas[] canvases = roots[r].GetComponentsInChildren<Canvas>(true);
                    for (int c = 0; c < canvases.Length; c++) {
                        if (IsScreenCanvas(canvases[c]) && Build((RectTransform)canvases[c].transform, path, circle, ring, stylus, fill, border)) {
                            here++;
                        }
                    }
                }

                if (here > 0) {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[Tutorial] {path}: {here} tutorial button(s).");
                }

                total += here;
            }

            if (!string.IsNullOrEmpty(returnPath)) {
                EditorSceneManager.OpenScene(returnPath, OpenSceneMode.Single);
            }

            Debug.Log($"[Tutorial] Done: {total} tutorial button(s) added.");
        }

        private static bool IsSkipped(string path) {
            for (int i = 0; i < SkippedScenes.Length; i++) {
                if (path.Contains(SkippedScenes[i])) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>A screen's own interface canvas: the outermost canvas that has the always-on-top treatment of the interface.</summary>
        private static bool IsScreenCanvas(Canvas canvas) {
            if (canvas.transform.parent != null && canvas.transform.parent.GetComponentInParent<Canvas>(true) != null) {
                return false;
            }

            MonoBehaviour[] behaviours = canvas.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++) {
                if (behaviours[i] != null && behaviours[i].GetType().Name == AlwaysOnTopName) {
                    return true;
                }
            }

            return false;
        }

        private static float ScaleOf(RectTransform canvas) {
            float width = canvas.rect.width;
            return width < MinCanvasWidth ? 1f : width / ReferenceWidth;
        }

        private static bool Build(RectTransform canvas, string scenePath, Sprite circle, Sprite ring, Sprite stylus, Sprite fill, Sprite border) {
            Transform existing = canvas.Find(RootName);
            if (existing != null) {
                Object.DestroyImmediate(existing.gameObject);
            }

            float s = ScaleOf(canvas);
            TMP_FontAsset font = FindFont(canvas);

            RectTransform root = NewRect(RootName, canvas);
            root.SetAsLastSibling();
            Stretch(root);

            RectTransform button = NewRect("InfoButton", root);
            Anchor(button, new Vector2(1f, 0f), new Vector2(-EdgeMargin * s, EdgeMargin * s), new Vector2(ButtonSize * s, ButtonSize * s));
            Image face = button.gameObject.AddComponent<Image>();
            face.sprite = circle;
            face.color = ButtonFill;
            face.raycastTarget = true;
            RectTransform buttonRing = NewRect("Ring", button);
            Stretch(buttonRing);
            Image ringImage = buttonRing.gameObject.AddComponent<Image>();
            ringImage.sprite = ring;
            ringImage.color = Cyan;
            ringImage.raycastTarget = false;
            TMP_Text letter = NewText("Letter", button, font, "i", 26f * s, FontStyles.Bold, Cyan, TextAlignmentOptions.Center);
            Stretch(letter.rectTransform);
            UiButtonMotion motion = button.gameObject.AddComponent<UiButtonMotion>();
            SerializedObject motionSerialized = new SerializedObject(motion);
            motionSerialized.FindProperty("hoverScale").floatValue = 1.1f;
            motionSerialized.ApplyModifiedPropertiesWithoutUndo();

            RectTransform panel = NewRect("Panel", root);
            Anchor(panel, new Vector2(1f, 0f), new Vector2(-EdgeMargin * s, (EdgeMargin + ButtonSize + PanelGap) * s), new Vector2(PanelWidth * s * PanelScale, PanelHeight * s * PanelScale));
            CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            TMP_Text[] headings = new TMP_Text[3];
            TMP_Text[] bodies = new TMP_Text[3];
            BuildPanel(panel, font, s * PanelScale, circle, stylus, fill, border, headings, bodies);

            StylusTutorial tutorial = button.gameObject.AddComponent<StylusTutorial>();
            SerializedObject tutorialSerialized = new SerializedObject(tutorial);
            tutorialSerialized.FindProperty("panel").objectReferenceValue = group;
            SetTexts(tutorialSerialized.FindProperty("headings"), headings);
            SetTexts(tutorialSerialized.FindProperty("bodies"), bodies);
            SetStrings(tutorialSerialized.FindProperty("homeHeadings"), HomeHeadings);
            SetStrings(tutorialSerialized.FindProperty("homeBodies"), HomeBodies);
            tutorialSerialized.FindProperty("alwaysHome").boolValue = IsHomeScreen(scenePath, canvas);
            Transform launcher = FindDeep(canvas, "Launcher");
            tutorialSerialized.FindProperty("homeIndicator").objectReferenceValue = launcher != null ? launcher.gameObject : null;
            tutorialSerialized.ApplyModifiedPropertiesWithoutUndo();
            StylusTutorialPanel hover = panel.gameObject.AddComponent<StylusTutorialPanel>();
            SerializedObject hoverSerialized = new SerializedObject(hover);
            hoverSerialized.FindProperty("owner").objectReferenceValue = tutorial;
            hoverSerialized.ApplyModifiedPropertiesWithoutUndo();
            SetLayerLike(root, canvas.gameObject.layer);
            EditorUtility.SetDirty(canvas);
            return true;
        }

        private static void SetTexts(SerializedProperty list, TMP_Text[] texts) {
            list.arraySize = texts.Length;
            for (int i = 0; i < texts.Length; i++) {
                list.GetArrayElementAtIndex(i).objectReferenceValue = texts[i];
            }
        }

        private static void SetStrings(SerializedProperty list, string[] values) {
            list.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) {
                list.GetArrayElementAtIndex(i).stringValue = values[i];
            }
        }

        /// <summary>A home screen: the launcher and the category screens, and the PCB home screen. (Biology tells it at run time.)</summary>
        private static bool IsHomeScreen(string scenePath, RectTransform canvas) {
            if (scenePath.Contains("HomeScene") || scenePath.Contains("KmaxLauncherScene")) {
                return true;
            }

            Transform screen = canvas.parent;
            return screen != null && screen.name == "Screen_Home";
        }

        private static Transform FindDeep(Transform root, string objectName) {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++) {
                if (all[i].name == objectName && all[i] != root) {
                    return all[i];
                }
            }

            return null;
        }

        private static void BuildPanel(RectTransform panel, TMP_FontAsset font, float s, Sprite circle, Sprite stylus, Sprite fill, Sprite border,
            TMP_Text[] headings, TMP_Text[] bodies) {
            Image back = panel.gameObject.AddComponent<Image>();
            back.sprite = fill;
            back.type = Image.Type.Sliced;
            back.color = PanelFill;
            back.raycastTarget = true;
            RectTransform outline = NewRect("Outline", panel);
            Stretch(outline);
            Image outlineImage = outline.gameObject.AddComponent<Image>();
            outlineImage.sprite = border;
            outlineImage.type = Image.Type.Sliced;
            outlineImage.color = PanelBorder;
            outlineImage.raycastTarget = false;

            AddText(panel, font, s, "Title", "How to use the stylus", 22f, FontStyles.Bold, Cyan, TextAlignmentOptions.TopLeft, 32f, 20f, 500f, 30f);

            float width = ImageHeight * ImageAspect;
            float left = PanelWidth * 0.5f - width * 0.5f;
            RectTransform picture = NewRect("Stylus", panel);
            AnchorTopLeft(picture, new Vector2(left * s, -ImageTop * s), new Vector2(width * s, ImageHeight * s));
            Image pictureImage = picture.gameObject.AddComponent<Image>();
            pictureImage.sprite = stylus;
            pictureImage.preserveAspect = true;
            pictureImage.raycastTarget = false;

            Vector2 top = Point(left, width, TopButton);
            Vector2 leftOval = Point(left, width, LeftButton);
            Vector2 rightOval = Point(left, width, RightButton);
            float rightColumn = PanelWidth - 32f - TextWidth;

            Line(panel, s, top, new Vector2(rightColumn - 14f, 190f));
            Line(panel, s, rightOval, new Vector2(rightColumn - 14f, 330f));
            Line(panel, s, leftOval, new Vector2(32f + TextWidth + 14f, 300f));

            Marker(panel, font, s, circle, "0", top);
            Marker(panel, font, s, circle, "1", leftOval);
            Marker(panel, font, s, circle, "2", rightOval);

            Entry(panel, font, s, "0", "Zoom and click", headings, bodies,
                "Tap to press a button on the screen. Hold and push the stylus away to zoom in, pull it back to zoom out.",
                rightColumn, 150f, TextAlignmentOptions.TopLeft);
            Entry(panel, font, s, "2", "Reset the view", headings, bodies,
                "Tap once to bring the view back to the start. It also presses a button on the screen.",
                rightColumn, 290f, TextAlignmentOptions.TopLeft);
            Entry(panel, font, s, "1", "Click, Drag, Move", headings, bodies,
                "Tap to select or press a button. Press on a part and drag to move it. Press on empty space and turn your wrist to rotate the view.",
                32f, 250f, TextAlignmentOptions.TopRight);
        }

        private static Vector2 Point(float left, float width, Vector2 normalised) {
            return new Vector2(left + width * normalised.x, ImageTop + ImageHeight * normalised.y);
        }

        /// <summary>A text block of the tutorial: the number, a short heading and what the button does, with its top left at (x, y) of the panel.</summary>
        private static void Entry(RectTransform panel, TMP_FontAsset font, float s, string number, string heading, TMP_Text[] headings, TMP_Text[] bodies,
            string body, float x, float y, TextAlignmentOptions alignment) {
            int index = int.Parse(number);
            headings[index] = AddText(panel, font, s, "Heading" + number, number + ". " + heading, 19f, FontStyles.Bold, Cyan, alignment, x, y, TextWidth, 28f);
            bodies[index] = AddText(panel, font, s, "Body" + number, body, 16f, FontStyles.Normal, Light, alignment, x, y + 32f, TextWidth, 110f);
        }

        private static void Marker(RectTransform panel, TMP_FontAsset font, float s, Sprite circle, string number, Vector2 centre) {
            RectTransform marker = NewRect("Marker" + number, panel);
            AnchorTopLeft(marker, new Vector2((centre.x - MarkerSize * 0.5f) * s, -(centre.y - MarkerSize * 0.5f) * s), new Vector2(MarkerSize * s, MarkerSize * s));
            Image image = marker.gameObject.AddComponent<Image>();
            image.sprite = circle;
            image.color = Cyan;
            image.raycastTarget = false;
            TMP_Text label = NewText("Number", marker, font, number, 16f * s, FontStyles.Bold, new Color(0.03f, 0.05f, 0.09f, 1f), TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
        }

        /// <summary>A thin line from one point of the panel to another (both measured from the panel's top left, downwards).</summary>
        private static void Line(RectTransform panel, float s, Vector2 from, Vector2 to) {
            RectTransform line = NewRect("Line", panel);
            Vector2 delta = new Vector2(to.x - from.x, -(to.y - from.y));
            line.anchorMin = new Vector2(0f, 1f);
            line.anchorMax = new Vector2(0f, 1f);
            line.pivot = new Vector2(0f, 0.5f);
            line.anchoredPosition = new Vector2(from.x * s, -from.y * s);
            line.sizeDelta = new Vector2(delta.magnitude * s, LineWidth * s);
            line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            Image image = line.gameObject.AddComponent<Image>();
            image.color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.85f);
            image.raycastTarget = false;
        }

        private static TMP_Text AddText(RectTransform parent, TMP_FontAsset font, float s, string objectName, string text, float size, FontStyles style, Color colour,
            TextAlignmentOptions alignment, float x, float y, float width, float height) {
            TMP_Text label = NewText(objectName, parent, font, text, size * s, style, colour, alignment);
            AnchorTopLeft(label.rectTransform, new Vector2(x * s, -y * s), new Vector2(width * s, height * s));
            label.textWrappingMode = TextWrappingModes.Normal;
            return label;
        }

        private static TMP_Text NewText(string objectName, RectTransform parent, TMP_FontAsset font, string text, float size, FontStyles style, Color colour,
            TextAlignmentOptions alignment) {
            GameObject host = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            host.transform.SetParent(parent, false);
            TextMeshProUGUI label = host.GetComponent<TextMeshProUGUI>();
            if (font != null) {
                label.font = font;
            }

            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = colour;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private static TMP_FontAsset FindFont(RectTransform canvas) {
            TMP_Text[] texts = canvas.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++) {
                if (texts[i].font != null) {
                    return texts[i].font;
                }
            }

            return TMP_Settings.defaultFontAsset;
        }

        private static RectTransform NewRect(string objectName, Transform parent) {
            GameObject host = new GameObject(objectName, typeof(RectTransform));
            host.transform.SetParent(parent, false);
            return (RectTransform)host.transform;
        }

        private static void Stretch(RectTransform rect) {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Anchors a rectangle to a bottom corner of its parent: the corner is the pivot, and the offset points into the screen.</summary>
        private static void Anchor(RectTransform rect, Vector2 corner, Vector2 offset, Vector2 size) {
            rect.anchorMin = corner;
            rect.anchorMax = corner;
            rect.pivot = corner;
            rect.anchoredPosition = offset;
            rect.sizeDelta = size;
        }

        private static void AnchorTopLeft(RectTransform rect, Vector2 offset, Vector2 size) {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = offset;
            rect.sizeDelta = size;
        }

        private static void SetLayerLike(Transform root, int layer) {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++) {
                all[i].gameObject.layer = layer;
            }
        }

        /// <summary>Makes the round sprites of the button and the markers, and sets the stylus picture up as a sprite.</summary>
        private static void EnsureSprites() {
            if (!File.Exists(CirclePath)) {
                WriteCircle(CirclePath, false);
            }

            if (!File.Exists(RingPath)) {
                WriteCircle(RingPath, true);
            }

            AssetDatabase.Refresh();
            SetSpriteImport(CirclePath);
            SetSpriteImport(RingPath);
            SetSpriteImport(StylusImagePath);
        }

        private static void SetSpriteImport(string path) {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) {
                return;
            }

            if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single && !importer.mipmapEnabled
                && importer.alphaIsTransparency) {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void WriteCircle(string path, bool ringOnly) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            Texture2D texture = new Texture2D(CircleSize, CircleSize, TextureFormat.RGBA32, false);
            float centre = (CircleSize - 1) * 0.5f;
            float radius = CircleSize * 0.5f - 1f;
            for (int y = 0; y < CircleSize; y++) {
                for (int x = 0; x < CircleSize; x++) {
                    float distance = Mathf.Sqrt((x - centre) * (x - centre) + (y - centre) * (y - centre));
                    float outer = Mathf.Clamp01(radius - distance + 0.5f);
                    float alpha = outer;
                    if (ringOnly) {
                        float inner = Mathf.Clamp01(distance - (radius - RingThickness) + 0.5f);
                        alpha = Mathf.Min(outer, inner);
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }
    }
}
