using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ViitorCloud.KMaxLauncher {
    /// <summary>
    /// The master launcher screen: the chosen project's model turns in front of the glass, a grid of cards lists every
    /// project in the catalog and a Load button opens the chosen project's scene. Pressing the chosen card again also loads
    /// it. Left alone, the launcher steps through the projects by itself; any press pauses that for a while.
    /// </summary>
    public class LauncherController : MonoBehaviour {
        private const string SubtitleSeparator = "  •  ";

        [SerializeField] private LauncherCatalog catalog;
        [SerializeField, Tooltip("The pool of cards, in reading order. Cards beyond the projects on offer are hidden.")]
        private LauncherCard[] cards = new LauncherCard[0];
        [SerializeField, Tooltip("One object per row of cards. A row with no visible card is switched off so the rest stay centred.")]
        private GameObject[] rows = new GameObject[0];
        [SerializeField, Min(1)] private int cardsPerRow = 5;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private Button loadButton;
        [SerializeField] private TMP_Text loadLabel;
        [SerializeField] private LauncherPreview preview;

        [Header("Showcase")]
        [SerializeField, Tooltip("Seconds each project stays on the turntable while nobody is choosing.")]
        private float advanceSeconds = 6f;
        [SerializeField, Tooltip("Seconds after a press before the showcase resumes.")]
        private float resumeAfterSeconds = 20f;

        private int selected = -1;
        private float nextAdvance;
        private bool isLoading;

        private void Awake() {
            if (catalog == null || titleText == null || subtitleText == null || loadButton == null || loadLabel == null || preview == null) {
                Debug.LogError($"{nameof(LauncherController)} on '{name}' is missing one of its references.", this);
                enabled = false;
                return;
            }

            if (catalog.Count == 0 || catalog.Count > cards.Length) {
                Debug.LogError($"{nameof(LauncherController)} on '{name}' has {catalog.Count} projects for {cards.Length} cards.", this);
                enabled = false;
            }
        }

        private void OnEnable() {
            loadButton.onClick.AddListener(HandleLoadClicked);
            for (int i = 0; i < cards.Length; i++) {
                cards[i].Clicked += HandleCardClicked;
            }
        }

        private void OnDisable() {
            loadButton.onClick.RemoveListener(HandleLoadClicked);
            for (int i = 0; i < cards.Length; i++) {
                cards[i].Clicked -= HandleCardClicked;
            }
        }

        private void Start() {
            titleText.text = catalog.Title;
            for (int i = 0; i < cards.Length; i++) {
                if (i < catalog.Count) {
                    LauncherProjectEntry entry = catalog.Get(i);
                    cards[i].Bind(i, entry.Label, entry.Thumbnail);
                    WarnIfNotInBuild(entry);
                } else {
                    cards[i].Hide();
                }
            }

            for (int row = 0; row < rows.Length; row++) {
                rows[row].SetActive(row * cardsPerRow < catalog.Count);
            }

            Select(0);
            nextAdvance = Time.time + advanceSeconds;
        }

        private void Update() {
            if (isLoading || catalog.Count < 2 || Time.time < nextAdvance) {
                return;
            }

            nextAdvance = Time.time + advanceSeconds;
            Select((selected + 1) % catalog.Count);
        }

        public void Select(int index) {
            if (index < 0 || index >= catalog.Count || index == selected) {
                return;
            }

            selected = index;
            LauncherProjectEntry entry = catalog.Get(index);
            for (int i = 0; i < catalog.Count; i++) {
                cards[i].SetSelected(i == index);
            }

            subtitleText.text = string.IsNullOrEmpty(entry.Subtitle) ? entry.Label : entry.Label + SubtitleSeparator + entry.Subtitle;
            loadLabel.text = string.Format(catalog.LoadFormat, entry.Label);
            preview.Show(index, entry.PreviewPrefab);
        }

        private void HandleCardClicked(int index) {
            nextAdvance = Time.time + resumeAfterSeconds;
            if (index == selected) {
                RequestLoad();
            } else {
                Select(index);
            }
        }

        private void HandleLoadClicked() {
            RequestLoad();
        }

        private void RequestLoad() {
            if (isLoading || selected < 0) {
                return;
            }

            LauncherProjectEntry entry = catalog.Get(selected);
            if (SceneUtility.GetBuildIndexByScenePath(entry.ScenePath) < 0) {
                Debug.LogError($"{nameof(LauncherController)}: scene '{entry.ScenePath}' of project '{entry.Label}' is not in the build settings.", this);
                return;
            }

            if (!string.IsNullOrEmpty(entry.LaunchKey)) {
                PlayerPrefs.SetString(entry.LaunchKey, entry.LaunchValue);
                PlayerPrefs.Save();
            }

            isLoading = true;
            LoadingScreen.Load(entry.ScenePath);
        }

        private void WarnIfNotInBuild(LauncherProjectEntry entry) {
            if (SceneUtility.GetBuildIndexByScenePath(entry.ScenePath) < 0) {
                Debug.LogWarning($"{nameof(LauncherController)}: scene '{entry.ScenePath}' of project '{entry.Label}' is not in the build settings.", this);
            }
        }
    }
}
