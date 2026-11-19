using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ViitorCloud.KMaxLauncher.Ui {
    /// <summary>
    /// Plays the interface hover and click cues for a button, as the anatomy exhibit's UiButtonSound does. The sounds go
    /// through <see cref="UiFeedback"/>, so this assembly does not need to know which audio player the game uses.
    /// </summary>
    public class UiButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler {
        private Selectable selectable;

        private void Awake() {
            selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (IsAvailable()) {
                UiFeedback.RaiseHover();
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (IsAvailable()) {
                UiFeedback.RaiseClick();
            }
        }

        private bool IsAvailable() {
            return selectable == null || selectable.IsInteractable();
        }
    }
}
