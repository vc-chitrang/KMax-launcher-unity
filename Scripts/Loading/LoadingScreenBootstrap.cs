using UnityEngine;

namespace ViitorCloud.KMaxLauncher {
    /// <summary>
    /// Creates the single <see cref="LoadingScreen"/> before the first scene loads, from the prefab in Resources. Running here, and not
    /// from a scene, guarantees there is exactly one loading screen whichever scene the application starts in.
    /// </summary>
    public static class LoadingScreenBootstrap {
        private const string PrefabName = "KMaxLoadingScreen";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create() {
            if (DoubleTapEscExit.Instance == null) {
                GameObject exit = new GameObject("DoubleTapEscExit");
                exit.AddComponent<DoubleTapEscExit>();
            }

            if (LoadingScreen.Instance != null) {
                return;
            }

            GameObject prefab = Resources.Load<GameObject>(PrefabName);
            if (prefab == null) {
                Debug.LogError($"{nameof(LoadingScreenBootstrap)}: Resources/{PrefabName}.prefab is missing; scenes will load without a loading screen. Run KMax/Launcher/Build Loading Screen.");
                return;
            }

            GameObject instance = Object.Instantiate(prefab);
            instance.name = PrefabName;
        }
    }
}
