using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Lays the choice pictures of the exhibits out as a grid of three columns that grows by rows with the number of choices:
    /// the Retail showroom's variant pictures and the paint and interior swatches of the three car scenes. Each picture is 50 percent
    /// larger than the first design, and the sizes were then raised by another 20 percent; running this again sets the sizes to the current ones. In the cars the two groups sit in one vertical stack (caption, grid, name), so the interior group moves
    /// down by itself when the paint group needs a second row. Safe to run again; it skips a scene that already has its grid.
    /// </summary>
    public static class SwatchGridBuilder {
        private const int Columns = 3;
        private const string GlassFillPath = "Assets/Games/kmax-display-example/Materials/UI/GlassFill.png";
        private const float SwatchCornerMultiplier = 2.5f;
        private const string RetailScenePath = "Assets/Games/kmax-retail-products/Scenes/RetailShowroom.unity";
        private const string FinishGridName = "FinishGrid";
        private const float FinishCell = 108f;
        private const float FinishSpacing = 14.4f;

        private const string PanelName = "FinishPanel";
        private const float SwatchCell = 56.16f;
        private const float SwatchSpacing = 10.8f;
        private const float PanelWidth = 192f;
        private const float StackSpacing = 4f;
        private const float GroupGap = 12f;
        private const float CaptionHeight = 16f;
        private const float NameHeight = 18f;

        private static readonly string[] CarScenes = new string[] {
            "Assets/Games/kmax-display-example/Scenes/VolvoS90.unity",
            "Assets/Games/kmax-display-example/Scenes/VolvoC40.unity",
            "Assets/Games/kmax-display-example/Scenes/FordMustangGT.unity"
        };

        [MenuItem("KMax/Launcher/Arrange Swatch Grids")]
        public static void Run() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[Swatch Grid] Do not run in play mode.");
                return;
            }

            int changed = 0;
            for (int i = 0; i < CarScenes.Length; i++) {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(CarScenes[i], OpenSceneMode.Single);
                bool arranged = ArrangeCar(scene);
                bool rounded = RoundSwatches(scene);
                bool sized = ResizeCar(scene);
                bool wrapped = WrapSwatchesInSlots();
                if (arranged || rounded || sized || wrapped) {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    changed++;
                }
            }

            UnityEngine.SceneManagement.Scene retail = EditorSceneManager.OpenScene(RetailScenePath, OpenSceneMode.Single);
            bool retailArranged = ArrangeRetail(retail);
            bool retailSized = ResizeRetail(retail);
            bool retailWrapped = WrapSwatchesInSlots();
            if (retailArranged || retailSized || retailWrapped) {
                EditorSceneManager.MarkSceneDirty(retail);
                EditorSceneManager.SaveScene(retail);
                changed++;
            }

            Debug.Log("[Swatch Grid] " + changed + " scenes arranged.");
        }

        /// <summary>
        /// Design.md 7.1: a button that has a <c>UiButtonMotion</c> must not be positioned by a layout group, because the motion writes its
        /// anchored position every frame and the swatches then pile up in one place. Each swatch of a swatch grid is put inside a slot
        /// that the grid positions; the swatch stretches to fill its slot. Safe to run again.
        /// </summary>
        private static bool WrapSwatchesInSlots() {
            bool changed = false;
            GridLayoutGroup[] grids = Object.FindObjectsByType<GridLayoutGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int g = 0; g < grids.Length; g++) {
                string gridName = grids[g].name;
                if (gridName != "PaintGrid" && gridName != "TrimGrid" && gridName != FinishGridName) {
                    continue;
                }

                Transform gridTransform = grids[g].transform;
                System.Collections.Generic.List<Transform> children = new System.Collections.Generic.List<Transform>();
                for (int i = 0; i < gridTransform.childCount; i++) {
                    children.Add(gridTransform.GetChild(i));
                }

                for (int i = 0; i < children.Count; i++) {
                    Transform child = children[i];
                    if (child.name.StartsWith("Slot_") || child.GetComponent<Button>() == null) {
                        continue;
                    }

                    GameObject slot = new GameObject("Slot_" + child.name, typeof(RectTransform));
                    slot.transform.SetParent(gridTransform, false);
                    slot.transform.SetSiblingIndex(child.GetSiblingIndex());
                    child.SetParent(slot.transform, false);
                    RectTransform childRect = (RectTransform)child;
                    childRect.anchorMin = Vector2.zero;
                    childRect.anchorMax = Vector2.one;
                    childRect.pivot = new Vector2(0.5f, 0.5f);
                    childRect.offsetMin = Vector2.zero;
                    childRect.offsetMax = Vector2.zero;
                    childRect.localScale = Vector3.one;
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>Sets the Retail grid to the current cell size (absolute, so running it again changes nothing).</summary>
        private static bool ResizeRetail(UnityEngine.SceneManagement.Scene scene) {
            Transform ui = FindUi(scene);
            Transform gridTransform = ui != null ? ui.Find(FinishGridName) : null;
            if (gridTransform == null) {
                return false;
            }

            bool changed = SetCells(gridTransform, FinishCell, FinishSpacing);
            RectTransform grid = (RectTransform)gridTransform;
            Vector2 size = new Vector2(Columns * FinishCell + (Columns - 1) * FinishSpacing, 2f * FinishCell + FinishSpacing);
            if (grid.sizeDelta != size) {
                grid.sizeDelta = size;
                EditorUtility.SetDirty(grid);
                changed = true;
            }

            return changed;
        }

        /// <summary>Sets both swatch grids and the stack that holds them to the current sizes.</summary>
        private static bool ResizeCar(UnityEngine.SceneManagement.Scene scene) {
            Transform ui = FindUi(scene);
            Transform panelTransform = ui != null ? ui.Find(PanelName) : null;
            if (panelTransform == null) {
                return false;
            }

            bool changed = false;
            RectTransform panel = (RectTransform)panelTransform;
            if (!Mathf.Approximately(panel.sizeDelta.x, PanelWidth)) {
                panel.sizeDelta = new Vector2(PanelWidth, panel.sizeDelta.y);
                EditorUtility.SetDirty(panel);
                changed = true;
            }

            LayoutElement[] elements = panel.GetComponentsInChildren<LayoutElement>(true);
            for (int i = 0; i < elements.Length; i++) {
                if (elements[i].transform.parent == panel && !Mathf.Approximately(elements[i].preferredWidth, PanelWidth)) {
                    elements[i].preferredWidth = PanelWidth;
                    EditorUtility.SetDirty(elements[i]);
                    changed = true;
                }
            }

            string[] grids = new string[] { "PaintGrid", "TrimGrid" };
            for (int i = 0; i < grids.Length; i++) {
                Transform grid = panel.Find(grids[i]);
                if (grid != null && SetCells(grid, SwatchCell, SwatchSpacing)) {
                    changed = true;
                }
            }

            return changed;
        }

        private static bool SetCells(Transform gridTransform, float cell, float spacing) {
            GridLayoutGroup grid = gridTransform.GetComponent<GridLayoutGroup>();
            if (grid == null) {
                return false;
            }

            Vector2 cellSize = new Vector2(cell, cell);
            Vector2 gap = new Vector2(spacing, spacing);
            if (grid.cellSize == cellSize && grid.spacing == gap) {
                return false;
            }

            grid.cellSize = cellSize;
            grid.spacing = gap;
            EditorUtility.SetDirty(grid);
            return true;
        }

        /// <summary>The swatch squares (ring, colour and sheen) take the glass rounded rectangle, with a 5 unit corner, so they match the rounded panels.</summary>
        private static bool RoundSwatches(UnityEngine.SceneManagement.Scene scene) {
            Transform ui = FindUi(scene);
            Sprite glass = AssetDatabase.LoadAssetAtPath<Sprite>(GlassFillPath);
            if (ui == null || glass == null) {
                return false;
            }

            bool changed = false;
            string[] prefixes = new string[] { "PaintSwatch", "TrimSwatch" };
            string[] layers = new string[] { "Ring", "Fill", "Wash" };
            for (int p = 0; p < prefixes.Length; p++) {
                for (int i = 1; i <= 6; i++) {
                    Transform swatch = FindDeep(ui, prefixes[p] + i);
                    if (swatch == null) {
                        continue;
                    }

                    for (int l = 0; l < layers.Length; l++) {
                        Transform layer = swatch.Find(layers[l]);
                        Image image = layer != null ? layer.GetComponent<Image>() : null;
                        if (image == null || (image.sprite == glass && image.type == Image.Type.Sliced)) {
                            continue;
                        }

                        image.sprite = glass;
                        image.type = Image.Type.Sliced;
                        image.pixelsPerUnitMultiplier = SwatchCornerMultiplier;
                        EditorUtility.SetDirty(image);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static Transform FindDeep(Transform root, string childName) {
            Transform direct = root.Find(childName);
            if (direct != null) {
                return direct;
            }

            foreach (Transform child in root) {
                Transform found = FindDeep(child, childName);
                if (found != null) {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindUi(UnityEngine.SceneManagement.Scene scene) {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++) {
                if (roots[i].name == "UI") {
                    return roots[i].transform;
                }
            }

            return null;
        }

        private static bool ArrangeRetail(UnityEngine.SceneManagement.Scene scene) {
            Transform ui = FindUi(scene);
            if (ui == null || ui.Find(FinishGridName) != null) {
                return false;
            }

            Transform first = ui.Find("FinishButton1");
            if (first == null) {
                Debug.LogError("[Swatch Grid] The Retail variant buttons are missing.");
                return false;
            }

            RectTransform grid = CreateGrid(ui, FinishGridName, FinishCell, FinishSpacing);
            grid.anchorMin = new Vector2(0f, 1f);
            grid.anchorMax = new Vector2(0f, 1f);
            grid.pivot = new Vector2(0f, 1f);
            grid.anchoredPosition = new Vector2(32f, -427f);
            grid.sizeDelta = new Vector2(Columns * FinishCell + (Columns - 1) * FinishSpacing, 2f * FinishCell + FinishSpacing);
            for (int i = 1; i <= 4; i++) {
                Transform button = ui.Find("FinishButton" + i);
                if (button != null) {
                    button.SetParent(grid, false);
                }
            }

            Transform name = ui.Find("FinishName");
            if (name != null) {
                name.gameObject.SetActive(false);
            }

            return true;
        }

        private static bool ArrangeCar(UnityEngine.SceneManagement.Scene scene) {
            Transform ui = FindUi(scene);
            if (ui == null || ui.Find(PanelName) != null) {
                return false;
            }

            Transform paintCaption = ui.Find("PaintCaption");
            Transform trimCaption = ui.Find("TrimCaption");
            Transform paintName = ui.Find("PaintName");
            Transform trimName = ui.Find("TrimName");
            if (paintCaption == null || trimCaption == null || paintName == null || trimName == null) {
                Debug.LogError("[Swatch Grid] The paint or interior captions are missing in " + scene.name + ".");
                return false;
            }

            GameObject panelObject = new GameObject(PanelName, typeof(RectTransform), typeof(VerticalLayoutGroup));
            RectTransform panel = (RectTransform)panelObject.transform;
            panel.SetParent(ui, false);
            panel.anchorMin = new Vector2(0f, 1f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            panel.anchoredPosition = new Vector2(32f, -130f);
            panel.sizeDelta = new Vector2(PanelWidth, 420f);
            VerticalLayoutGroup stack = panelObject.GetComponent<VerticalLayoutGroup>();
            stack.spacing = StackSpacing;
            stack.childAlignment = TextAnchor.UpperLeft;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = false;
            stack.childForceExpandHeight = false;

            AddLine(paintCaption, panel, CaptionHeight);
            AddGrid(ui, panel, "PaintGrid", "PaintSwatch", 5);
            AddLine(paintName, panel, NameHeight);
            AddSpacer(panel, GroupGap);
            AddLine(trimCaption, panel, CaptionHeight);
            AddGrid(ui, panel, "TrimGrid", "TrimSwatch", 4);
            AddLine(trimName, panel, NameHeight);
            return true;
        }

        private static void AddLine(Transform line, RectTransform panel, float height) {
            line.SetParent(panel, false);
            LayoutElement element = line.GetComponent<LayoutElement>();
            if (element == null) {
                element = line.gameObject.AddComponent<LayoutElement>();
            }

            element.preferredWidth = PanelWidth;
            element.preferredHeight = height;
        }

        private static void AddSpacer(RectTransform panel, float height) {
            GameObject spacer = new GameObject("Gap", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(panel, false);
            LayoutElement element = spacer.GetComponent<LayoutElement>();
            element.preferredHeight = height;
            element.preferredWidth = PanelWidth;
        }

        private static void AddGrid(Transform ui, RectTransform panel, string gridName, string swatchPrefix, int count) {
            RectTransform grid = CreateGrid(panel, gridName, SwatchCell, SwatchSpacing);
            for (int i = 1; i <= count; i++) {
                Transform swatch = ui.Find(swatchPrefix + i);
                if (swatch != null) {
                    swatch.SetParent(grid, false);
                }
            }
        }

        private static RectTransform CreateGrid(Transform parent, string gridName, float cell, float spacing) {
            GameObject gridObject = new GameObject(gridName, typeof(RectTransform), typeof(GridLayoutGroup));
            RectTransform rect = (RectTransform)gridObject.transform;
            rect.SetParent(parent, false);
            GridLayoutGroup grid = gridObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(cell, cell);
            grid.spacing = new Vector2(spacing, spacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Columns;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            return rect;
        }
    }
}
