using UnityEngine;
using UnityEngine.EventSystems;

namespace ViitorCloud.KMaxLauncher.Ui {
    /// <summary>Tells the <see cref="StylusTutorial"/> when the pointer is on its panel, so an open panel is not closed while it is being read.</summary>
    public class StylusTutorialPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField] private StylusTutorial owner;

        public void OnPointerEnter(PointerEventData eventData) {
            if (owner != null) {
                owner.SetPanelHovered(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (owner != null) {
                owner.SetPanelHovered(false);
            }
        }
    }
}
