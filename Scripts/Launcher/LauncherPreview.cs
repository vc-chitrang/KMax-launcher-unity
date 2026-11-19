using System.Collections.Generic;
using UnityEngine;
using ViitorCloud.KMaxLauncher.Ui;

namespace ViitorCloud.KMaxLauncher {
    /// <summary>
    /// The model that hovers in front of the glass on the launcher, turning slowly on a turntable tilted toward
    /// the viewer, with a gentle bob. Choosing another project lets the old board shrink away while the new one springs up
    /// (the anatomy launcher preview). Each board is scaled so that, however far round it turns, it stays inside
    /// <see cref="maxRadius"/>.
    /// </summary>
    public class LauncherPreview : MonoBehaviour {
        [SerializeField, Tooltip("Rest position of the turntable's centre, in its parent's space.")]
        private Vector3 restPosition = new Vector3(0f, 0.012f, -0.03f);
        [SerializeField, Tooltip("Degrees a second the board turns.")]
        private float spinDegreesPerSecond = 18f;
        [SerializeField, Tooltip("Degrees the turntable leans toward the viewer, so the components are seen from above.")]
        private float tiltDegrees = 42f;
        [SerializeField, Tooltip("Largest reach of a model from its centre in any direction, in metres.")]
        private float maxRadius = 0.065f;
        [SerializeField, Tooltip("How far the board rises and falls as it hovers, in metres.")]
        private float bobMetres = 0.003f;
        [SerializeField, Tooltip("Radians a second of the bob.")]
        private float bobSpeed = 1.8f;
        [SerializeField, Range(0.2f, 0.95f), Tooltip("Share of full size a board springs up from when it is chosen.")]
        private float popStartScale = 0.6f;
        [SerializeField, Range(4f, 40f)] private float popFrequency = 16f;
        [SerializeField, Range(0.2f, 1.2f)] private float popDamping = 0.55f;
        [SerializeField, Tooltip("Seconds a board takes to shrink away when another is chosen.")]
        private float retireSeconds = 0.18f;

        private readonly Dictionary<int, Turntable> tables = new Dictionary<int, Turntable>();
        private readonly List<Turntable> retiring = new List<Turntable>();
        private Turntable current;

        /// <summary>Puts this project's model on the turntable and lets the one that was there shrink away.</summary>
        public void Show(int key, GameObject prefab) {
            if (current != null && current.Key == key) {
                return;
            }

            if (current != null) {
                retiring.Add(current);
                current = null;
            }

            Turntable next;
            if (!tables.TryGetValue(key, out next)) {
                next = Create(key, prefab);
                if (next == null) {
                    return;
                }

                tables[key] = next;
            }

            retiring.Remove(next);
            next.PopScale = popStartScale;
            next.PopVelocity = 0f;
            next.Spin = current != null ? current.Spin : next.Spin;
            next.Pivot.SetActive(true);
            current = next;
        }

        private void Update() {
            float delta = Time.deltaTime;
            transform.localPosition = restPosition + new Vector3(0f, Mathf.Sin(Time.time * bobSpeed) * bobMetres, 0f);

            if (current != null) {
                UiSpring.Step(ref current.PopScale, ref current.PopVelocity, 1f, popFrequency, popDamping, delta);
                Turn(current, delta);
            }

            for (int i = retiring.Count - 1; i >= 0; i--) {
                Turntable table = retiring[i];
                table.PopScale -= retireSeconds > 0f ? delta / retireSeconds : 1f;
                if (table.PopScale <= 0.02f) {
                    table.Pivot.SetActive(false);
                    retiring.RemoveAt(i);
                } else {
                    Turn(table, delta);
                }
            }
        }

        private void Turn(Turntable table, float delta) {
            table.Spin = Mathf.Repeat(table.Spin + spinDegreesPerSecond * delta, 360f);
            table.Pivot.transform.localRotation = Quaternion.Euler(-tiltDegrees, 0f, 0f) * Quaternion.AngleAxis(table.Spin, Vector3.up);
            table.Pivot.transform.localScale = Vector3.one * (table.FitScale * Mathf.Max(0f, table.PopScale));
        }

        private Turntable Create(int key, GameObject prefab) {
            if (prefab == null) {
                Debug.LogError($"{nameof(LauncherPreview)} on '{name}' was given no board for project {key}.", this);
                return null;
            }

            GameObject pivot = new GameObject("Preview_" + prefab.name);
            pivot.transform.SetParent(transform, false);
            GameObject model = Instantiate(prefab, pivot.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            Collider[] colliders = model.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) {
                colliders[i].enabled = false;
            }

            Vector3 centre;
            float radius;
            Measure(pivot.transform, model, out centre, out radius);
            model.transform.localPosition = -centre;

            Turntable table = new Turntable();
            table.Key = key;
            table.Pivot = pivot;
            table.FitScale = radius > 0.0001f ? maxRadius / radius : 1f;
            pivot.transform.localScale = Vector3.one * (table.FitScale * popStartScale);
            return table;
        }

        /// <summary>The middle of the board's drawn parts and its largest reach from that middle in any direction.</summary>
        private static void Measure(Transform pivot, GameObject model, out Vector3 centre, out float radius) {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(false);
            bool hasBounds = false;
            Vector3 low = Vector3.zero;
            Vector3 high = Vector3.zero;
            Matrix4x4 toPivot = pivot.worldToLocalMatrix;
            for (int i = 0; i < renderers.Length; i++) {
                MeshFilter filter = renderers[i].GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) {
                    continue;
                }

                Bounds local = filter.sharedMesh.bounds;
                Matrix4x4 matrix = toPivot * renderers[i].transform.localToWorldMatrix;
                for (int corner = 0; corner < 8; corner++) {
                    Vector3 point = local.center + Vector3.Scale(local.extents, new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f));
                    Vector3 inPivot = matrix.MultiplyPoint3x4(point);
                    if (!hasBounds) {
                        low = inPivot;
                        high = inPivot;
                        hasBounds = true;
                    } else {
                        low = Vector3.Min(low, inPivot);
                        high = Vector3.Max(high, inPivot);
                    }
                }
            }

            if (!hasBounds) {
                centre = Vector3.zero;
                radius = 0.05f;
                return;
            }

            centre = (low + high) * 0.5f;
            radius = (high - low).magnitude * 0.5f;
        }

        /// <summary>One board on the turntable, and where it is in its turn and its spring.</summary>
        private class Turntable {
            public int Key;
            public GameObject Pivot;
            public float FitScale;
            public float Spin;
            public float PopScale;
            public float PopVelocity;
        }
    }
}
