using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ViitorCloud.KMaxLauncher.Editor {
    /// <summary>
    /// Keeps every pen in the build on the application standard (Rules.md R-11): the front button clicks, the end point is smoothed
    /// as in the Inline-Four Engine scene. Also saves that scene's pen as the reference prefab.
    /// </summary>
    public static class StylusAudit {
        private const string ReferenceScene = "Assets/Games/kmax-display-example/Scenes/VirtualExhibition WR.unity";
        private const string ReferencePen = "XRRig/AnatomyPen";
        private const string PrefabPath = "Assets/Games/kmax-display-example/Prefabs/Stylus.prefab";
        private const int PrimaryLeft = 0;
        private const float EndPointSmoothTime = 0.02f;

        [MenuItem("KMax/Launcher/Apply Stylus Standard")]
        public static void Apply() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[Stylus] Do not run in play mode.");
                return;
            }

            SaveReferencePrefab();
            StringBuilder report = new StringBuilder();
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes) {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                bool changed = false;
                MonoBehaviour[] all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < all.Length; i++) {
                    if (all[i] == null || all[i].GetType().Name != "KmaxStylus") {
                        continue;
                    }

                    SerializedObject so = new SerializedObject(all[i]);
                    SerializedProperty primary = so.FindProperty("PrimaryKey");
                    SerializedProperty smooth = so.FindProperty("smoothEndPoint");
                    SerializedProperty smoothTime = so.FindProperty("EndPointSmoothTime");
                    if (primary == null || smooth == null || smoothTime == null) {
                        Debug.LogError("[Stylus] KmaxStylus fields not found in " + entry.path);
                        continue;
                    }

                    if (primary.enumValueIndex != PrimaryLeft || !smooth.boolValue || !Mathf.Approximately(smoothTime.floatValue, EndPointSmoothTime)) {
                        primary.enumValueIndex = PrimaryLeft;
                        smooth.boolValue = true;
                        smoothTime.floatValue = EndPointSmoothTime;
                        so.ApplyModifiedProperties();
                        changed = true;
                    }
                }

                if (changed) {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    report.AppendLine("  changed " + entry.path);
                }
            }

            Debug.Log("[Stylus] Standard applied.\n" + report);
        }

        [MenuItem("KMax/Launcher/Apply Stylus Beam")]
        public static void ApplyBeam() {
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                Debug.LogError("[Stylus] Do not run in play mode.");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) {
                Debug.LogError("[Stylus] Run 'Apply Stylus Standard' first: " + PrefabPath + " is missing.");
                return;
            }

            Transform referenceHolder = null;
            MonoBehaviour[] referenceBehaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < referenceBehaviours.Length; i++) {
                if (referenceBehaviours[i] != null && referenceBehaviours[i].GetType().Name == "AnatomyStylusBeam") {
                    referenceHolder = referenceBehaviours[i].transform;
                }
            }

            if (referenceHolder == null) {
                Debug.LogError("[Stylus] Reference prefab has no beam.");
                return;
            }

            StringBuilder report = new StringBuilder();
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes) {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                MonoBehaviour[] all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                bool changed = false;
                for (int i = 0; i < all.Length; i++) {
                    if (all[i] == null || all[i].GetType().Name != "KmaxStylus") {
                        continue;
                    }

                    changed |= ApplyBeamToPen(all[i], referenceHolder, report, entry.path);
                }

                if (changed) {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }

            Debug.Log("[Stylus] Beam applied.\n" + report);
        }

        private static bool ApplyBeamToPen(MonoBehaviour pen, Transform referenceHolder, StringBuilder report, string scenePath) {
            SerializedObject penObject = new SerializedObject(pen);
            SerializedProperty stylusProperty = penObject.FindProperty("stylus");
            Transform holder = stylusProperty != null ? stylusProperty.objectReferenceValue as Transform : null;
            if (holder == null) {
                report.AppendLine("  SKIPPED (no beam holder) " + scenePath);
                return false;
            }

            MonoBehaviour[] onHolder = holder.GetComponents<MonoBehaviour>();
            bool alreadyReference = false;
            for (int i = 0; i < onHolder.Length; i++) {
                if (onHolder[i] != null && onHolder[i].GetType().Name == "AnatomyStylusBeam") {
                    alreadyReference = true;
                }
            }

            if (alreadyReference) {
                bool synced = SyncBeamValues(holder.gameObject, referenceHolder.gameObject);
                report.AppendLine((synced ? "  values synced " : "  already identical ") + scenePath);
                return synced;
            }

            List<string> outside = FindReferencesInto(holder, pen.gameObject.scene);
            if (outside.Count > 0) {
                report.AppendLine("  SKIPPED " + scenePath + ": other objects use the old beam: " + string.Join("; ", outside));
                return false;
            }

            GameObject fresh = Object.Instantiate(referenceHolder.gameObject, pen.transform);
            fresh.name = referenceHolder.name;
            fresh.transform.localPosition = Vector3.zero;
            fresh.transform.localRotation = Quaternion.identity;
            fresh.transform.localScale = Vector3.one;
            stylusProperty.objectReferenceValue = fresh.transform;
            penObject.ApplyModifiedProperties();
            RetargetPenTracker(pen.gameObject, holder, fresh.transform);
            Object.DestroyImmediate(holder.gameObject);
            report.AppendLine("  beam replaced " + scenePath);
            return true;
        }

        private static void RetargetPenTracker(GameObject pen, Transform oldHolder, Transform newHolder) {
            MonoBehaviour[] behaviours = pen.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++) {
                if (behaviours[i] == null || behaviours[i].GetType().Name != "PenTracker") {
                    continue;
                }

                SerializedObject so = new SerializedObject(behaviours[i]);
                SerializedProperty it = so.GetIterator();
                while (it.NextVisible(true)) {
                    if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue == oldHolder) {
                        it.objectReferenceValue = newHolder;
                    }
                }

                so.ApplyModifiedProperties();
            }
        }

        private static List<string> FindReferencesInto(Transform holder, UnityEngine.SceneManagement.Scene scene) {
            List<string> found = new List<string>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++) {
                Component[] components = roots[r].GetComponentsInChildren<Component>(true);
                for (int c = 0; c < components.Length; c++) {
                    Component component = components[c];
                    if (component == null || component is Transform || component.transform.IsChildOf(holder)) {
                        continue;
                    }

                    SerializedObject so = new SerializedObject(component);
                    SerializedProperty it = so.GetIterator();
                    while (it.NextVisible(true)) {
                        if (it.propertyType != SerializedPropertyType.ObjectReference || it.objectReferenceValue == null) {
                            continue;
                        }

                        Object target = it.objectReferenceValue;
                        Transform targetTransform = null;
                        if (target is Component) {
                            targetTransform = ((Component)target).transform;
                        } else if (target is GameObject) {
                            targetTransform = ((GameObject)target).transform;
                        }

                        bool followsPen = component.GetType().Name == "PenTracker" && targetTransform == holder;
                        if (targetTransform != null && targetTransform.IsChildOf(holder) && component.GetType().Name != "KmaxStylus" && !followsPen) {
                            found.Add(component.GetType().Name + "." + it.name + " on " + component.name);
                        }
                    }
                }
            }

            return found;
        }

        private static bool SyncBeamValues(GameObject target, GameObject reference) {
            bool changed = false;
            MonoBehaviour[] targets = target.GetComponents<MonoBehaviour>();
            MonoBehaviour[] references = reference.GetComponents<MonoBehaviour>();
            for (int t = 0; t < targets.Length; t++) {
                if (targets[t] == null || targets[t].GetType().Name != "AnatomyStylusBeam") {
                    continue;
                }

                for (int r = 0; r < references.Length; r++) {
                    if (references[r] == null || references[r].GetType() != targets[t].GetType()) {
                        continue;
                    }

                    SerializedObject to = new SerializedObject(targets[t]);
                    SerializedObject from = new SerializedObject(references[r]);
                    SerializedProperty it = from.GetIterator();
                    while (it.NextVisible(true)) {
                        if (it.name == "m_Script" || it.propertyType == SerializedPropertyType.ObjectReference) {
                            continue;
                        }

                        SerializedProperty destination = to.FindProperty(it.propertyPath);
                        if (destination != null && !SerializedProperty.DataEquals(destination, it)) {
                            to.CopyFromSerializedProperty(it);
                            changed = true;
                        }
                    }

                    to.ApplyModifiedProperties();
                }
            }

            return changed;
        }

        private static void SaveReferencePrefab() {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(ReferenceScene, OpenSceneMode.Single);
            GameObject pen = GameObject.Find(ReferencePen);
            if (pen == null) {
                Debug.LogError("[Stylus] Reference pen not found.");
                return;
            }

            PrefabUtility.SaveAsPrefabAsset(pen, PrefabPath);
            Debug.Log("[Stylus] Saved " + PrefabPath + " from " + scene.name);
        }
    }
}
