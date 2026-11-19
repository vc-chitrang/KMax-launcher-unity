using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Lists, for every scene in the build, what finger touch needs on the interface: an event system with the Kmax input module,
    /// a graphic raycaster on every canvas that holds a control, and a raycast target on every control. Read-only.
    /// </summary>
    public static class TouchAudit {
        [MenuItem("KMax/Launcher/Audit Touch Setup")]
        public static void Run() {
            StringBuilder report = new StringBuilder();
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes) {
                EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                report.AppendLine("== " + System.IO.Path.GetFileName(entry.path));
                EventSystem[] systems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                report.Append("  eventSystems=" + systems.Length);
                for (int i = 0; i < systems.Length; i++) {
                    BaseInputModule[] modules = systems[i].GetComponents<BaseInputModule>();
                    for (int m = 0; m < modules.Length; m++) {
                        report.Append(" [" + modules[m].GetType().Name + " enabled=" + modules[m].enabled + "]");
                    }
                }

                report.AppendLine();
                Selectable[] selectables = Object.FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                int noGraphic = 0;
                int noRaycaster = 0;
                int notInteractable = 0;
                int small = 0;
                List<string> problems = new List<string>();
                for (int i = 0; i < selectables.Length; i++) {
                    Selectable control = selectables[i];
                    if (control is Scrollbar || control is Slider) {
                        continue;
                    }

                    Graphic graphic = control.targetGraphic != null ? control.targetGraphic : control.GetComponent<Graphic>();
                    if (graphic == null || !graphic.raycastTarget) {
                        noGraphic++;
                        problems.Add("no raycast target: " + Path(control.transform));
                    }

                    Canvas canvas = control.GetComponentInParent<Canvas>(true);
                    Canvas root = canvas != null ? canvas.rootCanvas : null;
                    if (root == null || root.GetComponent<GraphicRaycaster>() == null) {
                        noRaycaster++;
                        problems.Add("no GraphicRaycaster: " + Path(control.transform));
                    }

                    if (!control.interactable) {
                        notInteractable++;
                    }

                    RectTransform rect = control.transform as RectTransform;
                    if (rect != null && (rect.rect.height < 36f || rect.rect.width < 36f)) {
                        small++;
                        problems.Add("small " + rect.rect.width.ToString("0") + "x" + rect.rect.height.ToString("0") + ": " + Path(control.transform));
                    }
                }

                report.AppendLine("  controls=" + selectables.Length + " noRaycastTarget=" + noGraphic + " noGraphicRaycaster=" + noRaycaster + " notInteractable=" + notInteractable + " under36px=" + small);
                for (int i = 0; i < problems.Count && i < 12; i++) {
                    report.AppendLine("    " + problems[i]);
                }
            }

            Debug.Log(report.ToString());
        }

        private static string Path(Transform t) {
            string path = t.name;
            int depth = 0;
            while (t.parent != null && depth < 3) {
                t = t.parent;
                path = t.name + "/" + path;
                depth++;
            }

            return path;
        }
    }
}
