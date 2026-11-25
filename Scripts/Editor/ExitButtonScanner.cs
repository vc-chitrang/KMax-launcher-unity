using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Scans all scenes in EditorBuildSettings.scenes to inspect and measure exit / navigation buttons.
    /// Emits a structured markdown report identifying button names, visibility, visible labels,
    /// anchored positions, load targets, and mismatches against the proposed Rule R-7.
    /// </summary>
    public static class ExitButtonScanner {
        private static readonly Regex ButtonFilter = new Regex("(Home|HomeSlot|HomeButton|BackToHomeButton|BackButton|MenuButton)", RegexOptions.IgnoreCase);

        [MenuItem("KMax/QA/Scan Exit Buttons")]
        public static void RunFromMenu() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[ExitScanner] Do not run in play mode.");
                return;
            }

            Scene active = SceneManager.GetActiveScene();
            string returnPath = active.path;

            string report = GenerateReport();
            Debug.Log("[ExitScanner] Scan Complete:\n" + report);

            if (!string.IsNullOrEmpty(returnPath) && File.Exists(returnPath)) {
                EditorSceneManager.OpenScene(returnPath, OpenSceneMode.Single);
            }
        }

        public static string GenerateReport() {
            StringBuilder sb = new StringBuilder();
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            sb.AppendLine("| Index | Scene | Found Button | Active | Label Text | Anchored Pos & Size | Load Target / Action | Status vs Proposed R-7 |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|");

            for (int i = 0; i < scenes.Length; i++) {
                string scenePath = scenes[i].path;
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                List<ButtonRecord> records = new List<ButtonRecord>();
                for (int b = 0; b < buttons.Length; b++) {
                    Button btn = buttons[b];
                    if (!ButtonFilter.IsMatch(btn.name)) {
                        continue;
                    }

                    ButtonRecord rec = InspectButton(btn);
                    records.Add(rec);
                }

                if (records.Count == 0) {
                    string role = scenePath.Contains("/Showcase/") || sceneName == "KmaxLauncherScene" ? "Exempt (Tech Demo / Root)" : "MISMATCH (Missing Exit Button)";
                    sb.AppendLine(string.Format("| {0:D2} | `{1}` | None | N/A | None | N/A | None | {2} |", i, sceneName, role));
                } else {
                    for (int r = 0; r < records.Count; r++) {
                        ButtonRecord rec = records[r];
                        string mismatchNote = EvaluateMismatch(sceneName, scenePath, rec);
                        sb.AppendLine(string.Format("| {0:D2} | `{1}` | `{2}` | {3} | \"{4}\" | {5} | `{6}` | {7} |",
                            i,
                            sceneName,
                            rec.name,
                            rec.activeInHierarchy ? "Yes" : "No",
                            rec.label,
                            rec.rectTransformString,
                            rec.targetAction,
                            mismatchNote));
                    }
                }
            }

            return sb.ToString();
        }

        private static ButtonRecord InspectButton(Button btn) {
            ButtonRecord rec = new ButtonRecord();
            rec.name = btn.name;
            rec.activeInHierarchy = btn.gameObject.activeInHierarchy;

            // Extract visible label text
            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null && !string.IsNullOrEmpty(tmp.text)) {
                rec.label = tmp.text.Trim().Replace("\n", " ");
            } else {
                Text txt = btn.GetComponentInChildren<Text>(true);
                if (txt != null && !string.IsNullOrEmpty(txt.text)) {
                    rec.label = txt.text.Trim().Replace("\n", " ");
                } else {
                    rec.label = "(no text)";
                }
            }

            // Extract position and size (check parent if inside a Slot)
            RectTransform rt = btn.GetComponent<RectTransform>();
            RectTransform parentRt = btn.transform.parent as RectTransform;
            if (rt != null) {
                if (parentRt != null && parentRt.name.EndsWith("Slot")) {
                    rec.rectTransformString = string.Format("({0:F0},{1:F0}) [{2:F0}x{3:F0}] via {4}", parentRt.anchoredPosition.x, parentRt.anchoredPosition.y, parentRt.sizeDelta.x, parentRt.sizeDelta.y, parentRt.name);
                } else {
                    rec.rectTransformString = string.Format("({0:F0},{1:F0}) [{2:F0}x{3:F0}]", rt.anchoredPosition.x, rt.anchoredPosition.y, rt.sizeDelta.x, rt.sizeDelta.y);
                }
            } else {
                rec.rectTransformString = "No RectTransform";
            }

            // Extract target script / onClick calls
            StringBuilder targetSb = new StringBuilder();
            MonoBehaviour[] behaviours = btn.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++) {
                if (behaviours[i] == null) {
                    continue;
                }
                string typeName = behaviours[i].GetType().Name;
                if (typeName == "LauncherHomeButton") {
                    SerializedObject so = new SerializedObject(behaviours[i]);
                    SerializedProperty sp = so.FindProperty("sceneName");
                    string dest = sp != null ? sp.stringValue : "";
                    targetSb.Append("LauncherHomeButton(" + dest + "); ");
                } else if (typeName.Contains("SceneLink") || typeName.Contains("HomeButton")) {
                    SerializedObject so = new SerializedObject(behaviours[i]);
                    SerializedProperty sp = so.FindProperty("sceneName") ?? so.FindProperty("targetScene");
                    string dest = sp != null ? sp.stringValue : "";
                    targetSb.Append(typeName + "(" + dest + "); ");
                }
            }

            SerializedObject btnSo = new SerializedObject(btn);
            SerializedProperty calls = btnSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            if (calls != null) {
                for (int c = 0; c < calls.arraySize; c++) {
                    SerializedProperty call = calls.GetArrayElementAtIndex(c);
                    string method = call.FindPropertyRelative("m_MethodName").stringValue;
                    UnityEngine.Object targetObj = call.FindPropertyRelative("m_Target").objectReferenceValue;
                    string targetName = targetObj != null ? targetObj.name : "null";
                    string strArg = call.FindPropertyRelative("m_Arguments.m_StringArgument").stringValue;
                    targetSb.Append(string.Format("{0}.{1}({2}); ", targetName, method, strArg));
                }
            }

            if (targetSb.Length == 0) {
                rec.targetAction = "No persistent target";
            } else {
                rec.targetAction = targetSb.ToString().TrimEnd(' ', ';');
            }

            return rec;
        }

        private static string EvaluateMismatch(string sceneName, string scenePath, ButtonRecord rec) {
            if (!rec.activeInHierarchy) {
                return "Inactive legacy button";
            }

            if (sceneName.EndsWith("HomeScene")) {
                if (rec.label == "Back" && rec.targetAction.Contains("KmaxLauncherScene")) {
                    return "Label mismatch: Button loads root launcher but displays label 'Back' instead of 'Home'";
                }
                if (rec.targetAction.Contains("KmaxLauncherScene")) {
                    return "Matches Proposed R-7 (Category Hub Home exit)";
                }
            }

            if (sceneName == "EyeAnatomy" || sceneName == "Launcher" || sceneName == "MissionControl") {
                if (rec.targetAction.Contains("KmaxLauncherScene")) {
                    return "Exits to KmaxLauncherScene (Hub bypass / Direct Root exit)";
                }
            }

            if (rec.name == "BackButton" && rec.label == "Back to the car") {
                return "Internal sub-view toggle (not scene way-out)";
            }

            if (rec.targetAction.Contains("HomeScene")) {
                return "Matches Proposed R-7 (Sub-Scene Back exit to Category Hub)";
            }

            return "Custom exit action";
        }

        private class ButtonRecord {
            public string name;
            public bool activeInHierarchy;
            public string label;
            public string rectTransformString;
            public string targetAction;
        }
    }
}
