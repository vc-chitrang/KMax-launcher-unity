using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Puts the way out of every screen where the visitor expects it. The Menu button of the exhibit screens and the Home button of the home
    /// screens do the same thing (they go back), so both are now one "Back" button at the bottom left of the screen. On the launcher and the
    /// home screens the Load button sits between the turning model and the cards. Safe to run again; run it after rebuilding a scene whose
    /// builder still places the old layout.
    /// </summary>
    public static class BackButtonLayout {
        private const string GamesFolder = "Assets/Games";
        private const string BackText = "Back";
        private const string TryAnotherText = "Explore more";
        private const string HiddenCaptionName = "BuildCaption";
        private const string EngineCaptionCardName = "CaptionCard";
        private const string LoadSlotName = "LoadSlot";
        private const string CardsName = "Cards";
        private const float ReferenceWidth = 1920f;
        private const float BackMargin = 32f;
        private const float BackBottom = 32f;
        private const float BackWidth = 150f;
        private const float BackHeight = 42f;
        private const float BackFontSize = 17f;
        private const float TopGap = 40f;
        private const float DefaultStep = 55f;
        private const float MinCanvasWidth = 1000f;
        private const float StackGap = 9f;
        private const float CardsBottom = 26f;
        private const float LoadGap = 14f;
        private const float LoadPadding = 24f;
        private const float LoadMinWidth = 160f;
        private const float LoadHeight = 42f;
        private const float LoadFontSize = 17f;

        private static readonly string[] BackBehaviourNames = new string[] { "LauncherHomeButton", "SceneLinkButton" };
        private static readonly string[] NextExhibitTexts = new string[] { "Next exhibit", "Next Exhibit", "Next Scene", "Try another" };
        private static readonly string[] BackLabelTexts = new string[] { "Menu", "Home", "Back" };
        private static readonly string[] SkippedScenes = new string[] { "/Showcase/", "kmax-display-example/Scenes/Launcher.unity" };

        [MenuItem("KMax/Launcher/Apply Back Button And Load Layout")]
        public static void Run() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[BackLayout] Do not run in play mode.");
                return;
            }

            Scene active = SceneManager.GetActiveScene();
            if (active.isDirty) {
                Debug.LogError("[BackLayout] Save or discard the changes in the open scene first.");
                return;
            }

            string returnPath = active.path;
            string[] guids = AssetDatabase.FindAssets("t:Scene", new string[] { GamesFolder });
            int backCount = 0;
            int loadCount = 0;
            for (int g = 0; g < guids.Length; g++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[g]);
                if (IsSkipped(path)) {
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int backHere = 0;
                int loadHere = 0;
                GameObject[] roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++) {
                    backHere += PlaceBackButtons(roots[r]);
                    backHere += PlaceTopicMenus(roots[r]);
                    backHere += TidyLabels(roots[r]);
                    backHere += HideEngineCaptionCard(roots[r], path);
                    loadHere += PlaceLoadButtons(roots[r]);
                }

                if (backHere > 0 || loadHere > 0) {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[BackLayout] {path}: {backHere} back button(s), {loadHere} load button(s).");
                }

                backCount += backHere;
                loadCount += loadHere;
            }

            if (!string.IsNullOrEmpty(returnPath)) {
                EditorSceneManager.OpenScene(returnPath, OpenSceneMode.Single);
            }

            Debug.Log($"[BackLayout] Done: {backCount} back button(s) and {loadCount} load button(s) placed.");
        }

        private static bool IsSkipped(string path) {
            for (int i = 0; i < SkippedScenes.Length; i++) {
                if (path.Contains(SkippedScenes[i])) {
                    return true;
                }
            }

            return false;
        }

        private static int PlaceBackButtons(GameObject root) {
            int placed = 0;
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++) {
                if (behaviours[i] == null || System.Array.IndexOf(BackBehaviourNames, behaviours[i].GetType().Name) < 0) {
                    continue;
                }

                Button button = behaviours[i].GetComponent<Button>();
                if (button == null || !SetBackLabel(button)) {
                    continue;
                }

                PlaceBack(button);
                placed++;
            }

            return placed;
        }

        /// <summary>
        /// The button that leads on to the next exhibit is called "Explore more" on every screen, and the small BUILD caption of the
        /// Inline-Four engine, which sat behind the top-right buttons, is hidden.
        /// </summary>
        private static int TidyLabels(GameObject root) {
            int changed = 0;
            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++) {
                string current = texts[i].text == null ? string.Empty : texts[i].text.Trim();
                if (System.Array.IndexOf(NextExhibitTexts, current) >= 0) {
                    texts[i].text = TryAnotherText;
                    EditorUtility.SetDirty(texts[i]);
                    changed++;
                } else if (texts[i].name == HiddenCaptionName && texts[i].gameObject.activeSelf) {
                    texts[i].gameObject.SetActive(false);
                    changed++;
                }
            }

            return changed;
        }

        /// <summary>The two Pontiac scenes no longer show the bottom-centre caption card (the hint line): it is switched off.</summary>
        private static int HideEngineCaptionCard(GameObject root, string scenePath) {
            if (!scenePath.Contains("kmax-automobile-engine/Scenes/")) {
                return 0;
            }

            int changed = 0;
            RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++) {
                if (rects[i].name == EngineCaptionCardName && rects[i].gameObject.activeSelf) {
                    rects[i].gameObject.SetActive(false);
                    changed++;
                }
            }

            return changed;
        }

        /// <summary>
        /// The anatomy topic screens keep their Menu button in a pill beside Next exhibit at the top right. It leaves the pill, becomes the
        /// Back button at the bottom left of the topic interface and keeps what it does (back to the anatomy home screen).
        /// </summary>
        private static int PlaceTopicMenus(GameObject root) {
            int placed = 0;
            RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++) {
                if (rects[i].name != "HomeSlot" || rects[i].parent == null || rects[i].parent.name != "NavigationPill") {
                    continue;
                }

                RectTransform slot = rects[i];
                Transform interfaceRoot = slot.parent;
                while (interfaceRoot != null && interfaceRoot.name != "TopicInterface") {
                    interfaceRoot = interfaceRoot.parent;
                }

                Button button = slot.GetComponentInChildren<Button>(true);
                if (interfaceRoot == null || button == null || !SetBackLabel(button)) {
                    Debug.LogWarning("[BackLayout] A topic Menu button could not be moved: no TopicInterface above it or no Menu label.", slot);
                    continue;
                }

                slot.SetParent(interfaceRoot, false);
                slot.SetAsLastSibling();
                LayoutElement element = slot.GetComponent<LayoutElement>();
                if (element != null) {
                    Object.DestroyImmediate(element);
                }

                PlaceBack(button);
                placed++;
            }

            return placed;
        }

        private static bool SetBackLabel(Button button) {
            TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
            bool found = false;
            for (int i = 0; i < labels.Length; i++) {
                string current = labels[i].text == null ? string.Empty : labels[i].text.Trim();
                if (System.Array.IndexOf(BackLabelTexts, current) >= 0) {
                    labels[i].text = BackText;
                    found = true;
                }
            }

            return found;
        }

        /// <summary>The rectangle that moves with a button: its slot or controls holder when it has one, otherwise the button itself.</summary>
        private static RectTransform TargetOf(Button button) {
            RectTransform buttonRect = (RectTransform)button.transform;
            RectTransform parent = buttonRect.parent as RectTransform;
            bool holder = parent != null && parent.GetComponent<Canvas>() == null && parent.parent != null
                && (parent.name.EndsWith("Slot") || parent.name.EndsWith("Controls"));
            return holder ? parent : buttonRect;
        }

        private static float CanvasScale(RectTransform canvasRect) {
            float width = canvasRect.rect.width;
            return width < MinCanvasWidth ? 1f : width / ReferenceWidth;
        }

        private static void PlaceBack(Button button) {
            RectTransform target = TargetOf(button);
            Canvas canvas = target.GetComponentInParent<Canvas>(true);
            RectTransform canvasRect = canvas != null ? (RectTransform)canvas.rootCanvas.transform : target;
            float scale = CanvasScale(canvasRect);

            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.zero;
            target.pivot = Vector2.zero;
            target.sizeDelta = new Vector2(BackWidth * scale, BackHeight * scale);
            target.anchoredPosition = new Vector2(BackMargin * scale, BackBottom * scale);
            TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < labels.Length; i++) {
                labels[i].enableAutoSizing = false;
                labels[i].fontSize = BackFontSize * scale;
            }

            ReflowTopRight(target, canvasRect, scale);
            MakeRoomBeside(target, canvasRect);
            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// The Menu button used to open the controls column at the top right. With it gone, each column that now starts lower than it
        /// should moves up by its own row step, so the first button takes the old Menu spot.
        /// </summary>
        private static void ReflowTopRight(RectTransform back, RectTransform canvasRect, float scale) {
            Button[] all = canvasRect.GetComponentsInChildren<Button>(true);
            List<RectTransform> column = new List<RectTransform>();
            for (int i = 0; i < all.Length; i++) {
                RectTransform rect = TargetOf(all[i]);
                if (rect == back || column.Contains(rect)) {
                    continue;
                }

                if (rect.anchorMin == Vector2.one && rect.anchorMax == Vector2.one) {
                    column.Add(rect);
                }
            }

            List<float> columns = new List<float>();
            for (int i = 0; i < column.Count; i++) {
                bool known = false;
                for (int c = 0; c < columns.Count; c++) {
                    known = known || Mathf.Abs(columns[c] - column[i].anchoredPosition.x) < 1f;
                }

                if (!known) {
                    columns.Add(column[i].anchoredPosition.x);
                }
            }

            for (int c = 0; c < columns.Count; c++) {
                ReflowColumn(column, columns[c], scale);
            }
        }

        private static void ReflowColumn(List<RectTransform> column, float x, float scale) {
            List<RectTransform> members = new List<RectTransform>();
            List<float> rows = new List<float>();
            float topY = float.MinValue;
            float height = 0f;
            for (int i = 0; i < column.Count; i++) {
                if (Mathf.Abs(column[i].anchoredPosition.x - x) >= 1f) {
                    continue;
                }

                members.Add(column[i]);
                float y = column[i].anchoredPosition.y;
                if (!rows.Contains(y)) {
                    rows.Add(y);
                }

                if (y > topY) {
                    topY = y;
                    height = column[i].sizeDelta.y;
                }
            }

            if (topY >= -(height + TopGap * scale)) {
                return;
            }

            rows.Sort();
            float step = DefaultStep * scale;
            float smallest = float.MaxValue;
            for (int i = 1; i < rows.Count; i++) {
                smallest = Mathf.Min(smallest, rows[i] - rows[i - 1]);
            }

            if (smallest < float.MaxValue && smallest > 1f) {
                step = smallest;
            }

            for (int i = 0; i < members.Count; i++) {
                Vector2 position = members[i].anchoredPosition;
                position.y += step;
                members[i].anchoredPosition = position;
                EditorUtility.SetDirty(members[i]);
            }
        }

        /// <summary>Other buttons anchored at the bottom left that now sit under the Back button move up above it, in order.</summary>
        private static void MakeRoomBeside(RectTransform back, RectTransform canvasRect) {
            Button[] all = canvasRect.GetComponentsInChildren<Button>(true);
            List<RectTransform> others = new List<RectTransform>();
            for (int i = 0; i < all.Length; i++) {
                RectTransform rect = (RectTransform)all[i].transform;
                if (rect == back || rect.IsChildOf(back) || back.IsChildOf(rect)) {
                    continue;
                }

                if (rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.zero) {
                    others.Add(rect);
                }
            }

            others.Sort(delegate(RectTransform a, RectTransform b) {
                return a.anchoredPosition.y.CompareTo(b.anchoredPosition.y);
            });

            List<Rect> taken = new List<Rect>();
            taken.Add(CanvasRect(back, canvasRect));
            float gap = StackGap * CanvasScale(canvasRect);
            for (int i = 0; i < others.Count; i++) {
                Rect current = CanvasRect(others[i], canvasRect);
                for (int t = 0; t < taken.Count; t++) {
                    if (Overlaps(current, taken[t])) {
                        float shift = taken[t].yMax + gap - current.yMin;
                        Vector2 position = others[i].anchoredPosition;
                        position.y += shift;
                        others[i].anchoredPosition = position;
                        current = CanvasRect(others[i], canvasRect);
                        EditorUtility.SetDirty(others[i]);
                    }
                }

                taken.Add(current);
            }
        }

        private static bool Overlaps(Rect a, Rect b) {
            return a.xMin < b.xMax && b.xMin < a.xMax && a.yMin < b.yMax && b.yMin < a.yMax;
        }

        private static Rect CanvasRect(RectTransform rect, RectTransform canvasRect) {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector3 min = canvasRect.InverseTransformPoint(corners[0]);
            Vector3 max = canvasRect.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static int PlaceLoadButtons(GameObject root) {
            int placed = 0;
            RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++) {
                if (rects[i].name != LoadSlotName) {
                    continue;
                }

                Transform cardsTransform = rects[i].parent != null ? rects[i].parent.Find(CardsName) : null;
                if (cardsTransform == null && rects[i].parent != null && rects[i].parent.name == CardsName) {
                    cardsTransform = rects[i].parent;
                }

                if (cardsTransform == null) {
                    Debug.LogWarning($"[BackLayout] '{LoadSlotName}' under '{rects[i].parent}' has no '{CardsName}' next to it; left as it is.", rects[i]);
                    continue;
                }

                if (PlaceLoad(rects[i], (RectTransform)cardsTransform)) {
                    placed++;
                }
            }

            return placed;
        }

        /// <summary>
        /// With a vertical stack of card rows the Load slot becomes the first child, so it sits on top of the rows that are shown; with a
        /// single row it is lifted above the row. Either way the button is as wide as its text.
        /// </summary>
        private static bool PlaceLoad(RectTransform slot, RectTransform cards) {
            VerticalLayoutGroup stack = cards.GetComponent<VerticalLayoutGroup>();
            bool changed = false;
            if (stack != null) {
                if (slot.parent != cards) {
                    float height = slot.sizeDelta.y;
                    slot.SetParent(cards, false);
                    slot.SetAsFirstSibling();
                    LayoutElement element = slot.GetComponent<LayoutElement>();
                    if (element == null) {
                        element = slot.gameObject.AddComponent<LayoutElement>();
                    }

                    element.preferredHeight = height;
                    cards.sizeDelta = new Vector2(cards.sizeDelta.x, cards.sizeDelta.y + height + stack.spacing);
                    cards.anchoredPosition = new Vector2(cards.anchoredPosition.x, CardsBottom);
                    EditorUtility.SetDirty(cards);
                    changed = true;
                }
            } else if (slot.anchoredPosition.y < cards.anchoredPosition.y + cards.sizeDelta.y) {
                cards.anchoredPosition = new Vector2(cards.anchoredPosition.x, CardsBottom);
                slot.anchoredPosition = new Vector2(slot.anchoredPosition.x, CardsBottom + cards.sizeDelta.y + LoadGap);
                EditorUtility.SetDirty(cards);
                changed = true;
            }

            changed |= FitLoadWidth(slot, stack != null);
            EditorUtility.SetDirty(slot);
            return changed;
        }

        /// <summary>
        /// Makes the Load button as wide as its label (padding 24 each side, at least 160), so "Load Heart" is not as wide as
        /// "Load Blink an LED". The button fits itself and sits in the middle of its slot, so it stays centred whatever the slot's
        /// width is and whenever the cards are laid out.
        /// </summary>
        internal static bool FitLoadWidth(RectTransform slot, bool inStack) {
            Button button = slot.GetComponentInChildren<Button>(true);
            if (button == null) {
                Debug.LogWarning("[BackLayout] The Load slot has no button to fit.", slot);
                return false;
            }

            RectTransform host = (RectTransform)button.transform;
            if (host.GetComponent<ContentSizeFitter>() != null) {
                return false;
            }

            HorizontalLayoutGroup oldGroup = slot.GetComponent<HorizontalLayoutGroup>();
            if (oldGroup != null) {
                Object.DestroyImmediate(oldGroup);
            }

            ContentSizeFitter oldFitter = slot.GetComponent<ContentSizeFitter>();
            if (oldFitter != null) {
                Object.DestroyImmediate(oldFitter);
            }

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null) {
                label.enableAutoSizing = false;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                label.fontSize = LoadFontSize;
            }

            Transform outline = host.Find("Outline");
            if (outline != null) {
                LayoutElement ignore = outline.GetComponent<LayoutElement>();
                if (ignore == null) {
                    ignore = outline.gameObject.AddComponent<LayoutElement>();
                }

                ignore.ignoreLayout = true;
            }

            HorizontalLayoutGroup inner = host.GetComponent<HorizontalLayoutGroup>();
            if (inner == null) {
                inner = host.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            inner.padding = new RectOffset((int)LoadPadding, (int)LoadPadding, 0, 0);
            inner.childAlignment = TextAnchor.MiddleCenter;
            inner.childControlWidth = true;
            inner.childControlHeight = true;
            inner.childForceExpandWidth = false;
            inner.childForceExpandHeight = false;
            LayoutElement hostElement = host.GetComponent<LayoutElement>();
            if (hostElement == null) {
                hostElement = host.gameObject.AddComponent<LayoutElement>();
            }

            hostElement.minWidth = LoadMinWidth;
            ContentSizeFitter fitter = host.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            host.anchorMin = new Vector2(0.5f, 0.5f);
            host.anchorMax = new Vector2(0.5f, 0.5f);
            host.pivot = new Vector2(0.5f, 0.5f);
            host.anchoredPosition = Vector2.zero;
            host.sizeDelta = new Vector2(host.sizeDelta.x, LoadHeight);

            LayoutElement slotElement = slot.GetComponent<LayoutElement>();
            if (inStack) {
                if (slotElement == null) {
                    slotElement = slot.gameObject.AddComponent<LayoutElement>();
                }

                slotElement.preferredWidth = -1f;
                slotElement.preferredHeight = LoadHeight;
            }

            return true;
        }
    }
}
