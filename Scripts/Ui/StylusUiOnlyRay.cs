using KmaxXR;
using UnityEngine;

namespace ViitorCloud.KMaxLauncher.Ui {
    /// <summary>
    /// On the launcher and the home screens nothing in the 3D scene can be used, only the interface. While this is active the interface
    /// is put on the UI layer and the stylus ray is limited to that layer, so the beam and its tip do not land on the turntable models
    /// or the backdrop, and the tip only touches the controls. The ray's own mask is put back when this is disabled or destroyed.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class StylusUiOnlyRay : MonoBehaviour {
        private const string UiLayerName = "UI";

        private KmaxStylus _stylus;
        private LayerMask _savedMask;
        private bool _applied;

        private void LateUpdate() {
            if (_applied) {
                return;
            }

            if (_stylus == null) {
                _stylus = KmaxPointer.PointerById(KmaxStylus.UniqueId) as KmaxStylus;
                if (_stylus == null) {
                    return;
                }
            }

            int uiLayer = LayerMask.NameToLayer(UiLayerName);
            if (uiLayer < 0) {
                Debug.LogError($"{nameof(StylusUiOnlyRay)} on '{name}': the '{UiLayerName}' layer does not exist; the ray is left as it was.", this);
                enabled = false;
                return;
            }

            SetLayerRecursively(transform, uiLayer);
            _savedMask = _stylus.layer;
            _stylus.layer = 1 << uiLayer;
            _applied = true;
        }

        private static void SetLayerRecursively(Transform root, int layer) {
            root.gameObject.layer = layer;
            for (int i = 0; i < root.childCount; i++) {
                SetLayerRecursively(root.GetChild(i), layer);
            }
        }

        private void OnDisable() {
            Restore();
        }

        private void OnDestroy() {
            Restore();
        }

        private void Restore() {
            if (!_applied) {
                return;
            }

            if (_stylus != null) {
                _stylus.layer = _savedMask;
            }

            _applied = false;
        }
    }
}
