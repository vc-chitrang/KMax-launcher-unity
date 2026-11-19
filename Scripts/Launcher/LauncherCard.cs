using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ViitorCloud.KMaxLauncher.Ui;

namespace ViitorCloud.KMaxLauncher {
    public class LauncherCard : MonoBehaviour {
        [SerializeField, Tooltip("The slot the layout positions; shown or hidden as a whole.")]
        private GameObject slot;
        [SerializeField] private Button button;
        [SerializeField] private UiButtonMotion motion;
        [SerializeField] private Image thumbnail;
        [SerializeField] private TMP_Text label;
        [SerializeField, Tooltip("The cyan bar along the top of the chosen card.")]
        private Image selectBar;
        [SerializeField, Tooltip("The glass outline, which brightens on the chosen card.")]
        private Image outline;
        [SerializeField] private Color restFill = new Color(0.059f, 0.09f, 0.165f, 0.82f);
        [SerializeField] private Color chosenFill = new Color(0.031f, 0.337f, 0.408f, 0.94f);
        [SerializeField] private Color restOutline = new Color(0.2f, 0.255f, 0.333f, 0.6f);
        [SerializeField] private Color chosenOutline = new Color(0.133f, 0.827f, 0.933f, 1f);
        [SerializeField] private Color restLabel = new Color(0.886f, 0.91f, 0.941f, 1f);
        [SerializeField] private Color chosenLabel = Color.white;

        private int index = -1;

        public event Action<int> Clicked;

        public bool IsVisible {
            get { return slot != null && slot.activeSelf; }
        }

        private void Awake() {
            if (slot == null || button == null || motion == null || thumbnail == null || label == null || selectBar == null || outline == null) {
                Debug.LogError($"{nameof(LauncherCard)} on '{name}' is missing one of its references.", this);
                enabled = false;
            }
        }

        private void OnEnable() {
            if (button != null) {
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDisable() {
            if (button != null) {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        public void Bind(int cardIndex, string text, Sprite picture) {
            index = cardIndex;
            label.text = text;
            thumbnail.sprite = picture;
            thumbnail.enabled = picture != null;
            slot.SetActive(true);
            SetSelected(false);
        }

        public void Hide() {
            index = -1;
            slot.SetActive(false);
        }

        public void SetSelected(bool selected) {
            selectBar.enabled = selected;
            outline.color = selected ? chosenOutline : restOutline;
            label.color = selected ? chosenLabel : restLabel;
            motion.SetRestTint(selected ? chosenFill : restFill);
        }

        private void HandleClicked() {
            if (index >= 0 && Clicked != null) {
                Clicked(index);
            }
        }
    }
}
