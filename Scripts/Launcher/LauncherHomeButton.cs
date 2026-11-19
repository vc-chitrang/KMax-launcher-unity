using UnityEngine;
using UnityEngine.UI;

namespace ViitorCloud.KMaxLauncher {
    /// <summary>
    /// The Home button of a topic's home screen: opens the launcher scene behind the loading screen, so a visitor can go back to the list
    /// of topics. It hides itself when that scene is not in the build.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LauncherHomeButton : MonoBehaviour {
        [SerializeField, Tooltip("Name of the scene to open, as listed in the build settings.")]
        private string sceneName = "KmaxLauncherScene";

        private Button button;

        private void Awake() {
            button = GetComponent<Button>();
        }

        private void OnEnable() {
            if (!Application.CanStreamedLevelBeLoaded(sceneName)) {
                gameObject.SetActive(false);
                return;
            }

            button.onClick.AddListener(HandleClicked);
        }

        private void OnDisable() {
            button.onClick.RemoveListener(HandleClicked);
        }

        private void HandleClicked() {
            LoadingScreen.Load(sceneName);
        }
    }
}
