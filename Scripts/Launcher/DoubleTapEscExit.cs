using UnityEngine;

namespace ViitorCloud.KMaxLauncher {
    /// <summary>
    /// Exits the application when the Escape key is double-tapped within a short window, from any scene: <see cref="LoadingScreenBootstrap"/>
    /// creates the one instance before the first scene and it survives scene loads. Stops play mode in the Unity Editor.
    /// </summary>
    public class DoubleTapEscExit : MonoBehaviour {
        [SerializeField, Tooltip("Maximum seconds between two Escape presses to count as a double-tap.")]
        private float doubleTapWindow = 0.45f;

        private float lastEscTime = -10f;

        public static DoubleTapEscExit Instance { get; private set; }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }

            Instance = this;
            if (transform.parent == null && GetComponents<Component>().Length == 2) {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                float now = Time.unscaledTime;
                if (now - lastEscTime <= doubleTapWindow) {
                    ExitApp();
                } else {
                    lastEscTime = now;
                }
            }
        }

        private void ExitApp() {
            Debug.Log("[DoubleTapEscExit] Escape double-tapped. Exiting application.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
