using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ViitorCloud.KMaxLauncher.Ui {
    /// <summary>
    /// The (i) button at the bottom right of a screen: a tap opens the panel that shows how the stylus is used, and a tap closes it again.
    /// An open panel closes by itself after <see cref="autoCloseSeconds"/> unless the pointer is on the (i) button or on the panel, so a kiosk
    /// is never left showing it. The panel is a canvas group that stays active, so the interface treatment of the canvas applies to it from
    /// the start. On a home screen (cards and a Load button, no model to turn) the panel says what the buttons do there.
    /// </summary>
    public class StylusTutorial : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
        [SerializeField, Tooltip("The panel that shows the stylus and what each button does.")]
        private CanvasGroup panel;
        [SerializeField, Min(0.01f), Tooltip("Seconds the panel takes to fade in or out.")]
        private float fadeSeconds = 0.15f;
        [SerializeField, Min(1f), Tooltip("Seconds an open panel stays open while the pointer is not on the button or the panel.")]
        private float autoCloseSeconds = 8f;
        [SerializeField, Tooltip("The three headings of the panel, for buttons 0, 1 and 2. Their text on an exhibit screen is the one they are built with.")]
        private TMP_Text[] headings = new TMP_Text[0];
        [SerializeField, Tooltip("The three descriptions of the panel, for buttons 0, 1 and 2. Their text on an exhibit screen is the one they are built with.")]
        private TMP_Text[] bodies = new TMP_Text[0];
        [SerializeField, Tooltip("Headings shown on a home screen.")]
        private string[] homeHeadings = new string[0];
        [SerializeField, Tooltip("Descriptions shown on a home screen.")]
        private string[] homeBodies = new string[0];
        [SerializeField, Tooltip("True on a screen that is always a home screen.")]
        private bool alwaysHome;
        [SerializeField, Tooltip("Optional. When this object is active the screen is a home screen (the Biology scene shows its home screen and its topics in one canvas).")]
        private GameObject homeIndicator;

        private string[] exhibitHeadings;
        private string[] exhibitBodies;
        private bool open;
        private bool buttonHovered;
        private bool panelHovered;
        private float idleTimer;

        private void Awake() {
            if (panel == null) {
                Debug.LogError($"{nameof(StylusTutorial)} on '{name}' needs its panel.", this);
                enabled = false;
                return;
            }

            exhibitHeadings = ReadTexts(headings);
            exhibitBodies = ReadTexts(bodies);
            Hide();
        }

        private void OnDisable() {
            open = false;
            buttonHovered = false;
            panelHovered = false;
            if (panel != null) {
                Hide();
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            buttonHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            buttonHovered = false;
        }

        public void OnPointerClick(PointerEventData eventData) {
            open = !open;
            idleTimer = 0f;
            if (open) {
                ShowContent();
            }
        }

        /// <summary>Called by the panel when the pointer enters or leaves it.</summary>
        public void SetPanelHovered(bool hovered) {
            panelHovered = hovered;
        }

        private void Update() {
            if (open) {
                if (buttonHovered || panelHovered) {
                    idleTimer = 0f;
                } else {
                    idleTimer += Time.unscaledDeltaTime;
                    if (idleTimer >= autoCloseSeconds) {
                        open = false;
                    }
                }
            }

            if (!open) {
                panelHovered = false;
            }

            panel.blocksRaycasts = open;
            panel.alpha = Mathf.MoveTowards(panel.alpha, open ? 1f : 0f, Time.unscaledDeltaTime / fadeSeconds);
        }

        private void ShowContent() {
            bool home = alwaysHome || (homeIndicator != null && homeIndicator.activeInHierarchy);
            for (int i = 0; i < 3; i++) {
                if (i < headings.Length && headings[i] != null) {
                    headings[i].text = home && i < homeHeadings.Length ? homeHeadings[i] : exhibitHeadings[i];
                }

                if (i < bodies.Length && bodies[i] != null) {
                    bodies[i].text = home && i < homeBodies.Length ? homeBodies[i] : exhibitBodies[i];
                }
            }
        }

        private static string[] ReadTexts(TMP_Text[] texts) {
            string[] result = new string[texts.Length];
            for (int i = 0; i < texts.Length; i++) {
                result[i] = texts[i] != null ? texts[i].text : string.Empty;
            }

            return result;
        }

        private void Hide() {
            panel.alpha = 0f;
            panel.blocksRaycasts = false;
            panel.interactable = false;
        }
    }
}
