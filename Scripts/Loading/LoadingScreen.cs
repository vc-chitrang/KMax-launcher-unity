using System.Collections;
using KmaxXR;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ViitorCloud.KMaxLauncher {
    /// <summary>
    /// The one loading screen of the whole application: a slate backdrop that fills the whole view, with an animated ring and the text
    /// "Loading...". It lives in the project's Resources (<see cref="LoadingScreenBootstrap"/> creates the single instance before the
    /// first scene and keeps it across scene loads), fades in, loads the scene without freezing the picture, then fades out. It is a world-space canvas kept at the screen plane in front of the camera (not a screen-space overlay), so on the stereo display it is drawn once per eye like every other interface and reads as one picture in 3D. Every
    /// scene change in the launcher and in the hosted projects goes through <see cref="Load"/>.
    /// </summary>
    public class LoadingScreen : MonoBehaviour {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text label;
        [SerializeField] private string message = "Loading...";
        [SerializeField, Tooltip("Seconds the screen takes to cover the scene that is about to be left.")]
        private float fadeInSeconds = 0.25f;
        [SerializeField, Tooltip("Seconds the screen takes to reveal the scene that has loaded.")]
        private float fadeOutSeconds = 0.45f;
        [SerializeField, Tooltip("Least seconds the screen stays fully shown, so a fast load does not flash.")]
        private float minimumShownSeconds = 0.6f;

        [SerializeField, Tooltip("Metres from the camera to the screen plane, where every other interface of the display sits.")]
        private float screenDistance = 0.5f;

        private bool isLoading;
        private Camera viewCamera;
        private float currentDistance;

        public static LoadingScreen Instance { get; private set; }

        /// <summary>True from the start of the fade in until the fade out has finished.</summary>
        public static bool IsLoading {
            get { return Instance != null && Instance.isLoading; }
        }

        /// <summary>Loads a scene (build-settings name or path) behind the loading screen. Without a loading screen it loads directly.</summary>
        public static void Load(string sceneNameOrPath) {
            if (Instance == null) {
                SceneManager.LoadScene(sceneNameOrPath);
                return;
            }

            Instance.Begin(sceneNameOrPath);
        }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            if (canvas == null || group == null || label == null) {
                Debug.LogError($"{nameof(LoadingScreen)} on '{name}' is missing one of its references.", this);
                enabled = false;
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentDistance = screenDistance;
            label.text = message;
            group.alpha = 0f;
            group.blocksRaycasts = false;
            canvas.enabled = false;
        }

        private void LateUpdate() {
            if (!canvas.enabled) {
                return;
            }

            if (viewCamera == null || !viewCamera.isActiveAndEnabled) {
                viewCamera = FindCamera();
                if (viewCamera == null) {
                    return;
                }
            }

            Transform view = viewCamera.transform;
            canvas.transform.SetPositionAndRotation(view.position + view.forward * currentDistance, view.rotation);
        }

        private static Camera FindCamera() {
            Camera main = Camera.main;
            if (main != null && main.isActiveAndEnabled) {
                return main;
            }

            Camera[] cameras = Camera.allCameras;
            for (int i = 0; i < cameras.Length; i++) {
                if (cameras[i] != null && cameras[i].isActiveAndEnabled) {
                    return cameras[i];
                }
            }

            return null;
        }

        private void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }

        private void Begin(string sceneNameOrPath) {
            if (isLoading) {
                return;
            }

            MatchSceneInterfaceScale();
            StartCoroutine(Run(sceneNameOrPath));
        }

        /// <summary>
        /// Sizes and places the loading canvas so it is the same picture on screen in every scene: it sits at the screen plane (the
        /// rig's distance from the camera, whatever scale the scene is built at) and is exactly as wide as the view is there, taken from
        /// the camera's own projection. Text, spinner and backdrop therefore always take the same share of the view.
        /// </summary>
        private void MatchSceneInterfaceScale() {
            currentDistance = screenDistance;
            Camera camera = FindCamera();
            if (camera == null) {
                return;
            }

            XRRig rig = FindFirstObjectByType<XRRig>();
            if (rig != null) {
                float distance = Vector3.Dot(rig.transform.position - camera.transform.position, camera.transform.forward);
                if (distance > 0.05f) {
                    currentDistance = distance;
                }
            }

            Camera eye = camera;
            Camera[] eyes = camera.GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < eyes.Length; i++) {
                if (eyes[i] != camera && eyes[i].name == "left") {
                    eye = eyes[i];
                    break;
                }
            }

            float horizontal = Mathf.Abs(eye.projectionMatrix.m00);
            if (horizontal < 0.01f) {
                return;
            }

            float widthMetres = 2f * currentDistance / horizontal;
            float referenceWidth = ((RectTransform)canvas.transform).sizeDelta.x;
            canvas.transform.localScale = Vector3.one * (widthMetres / referenceWidth);
        }

        private IEnumerator Run(string sceneNameOrPath) {
            isLoading = true;
            canvas.enabled = true;
            group.blocksRaycasts = true;
            yield return Fade(0f, 1f, fadeInSeconds);

            float shownAt = Time.unscaledTime;
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneNameOrPath);
            if (operation == null) {
                Debug.LogError($"{nameof(LoadingScreen)}: scene '{sceneNameOrPath}' could not be loaded (is it in the build settings?).", this);
            } else {
                while (!operation.isDone) {
                    yield return null;
                }

                yield return null;
            }

            viewCamera = null;
            MatchSceneInterfaceScale();

            float remaining = minimumShownSeconds - (Time.unscaledTime - shownAt);
            if (remaining > 0f) {
                yield return new WaitForSecondsRealtime(remaining);
            }

            yield return Fade(1f, 0f, fadeOutSeconds);
            group.blocksRaycasts = false;
            canvas.enabled = false;
            isLoading = false;
        }

        private IEnumerator Fade(float from, float to, float seconds) {
            float elapsed = 0f;
            while (elapsed < seconds) {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.SmoothStep(from, to, Mathf.Clamp01(elapsed / seconds));
                yield return null;
            }

            group.alpha = to;
        }
    }
}
