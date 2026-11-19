using System;
using UnityEngine;

namespace ViitorCloud.KMaxLauncher {
    [Serializable]
    public class LauncherProjectEntry {
        [SerializeField] private string label = "Project";
        [SerializeField, Tooltip("Shown in cyan under the title when the project is chosen.")]
        private string subtitle = "";
        [SerializeField, Tooltip("Asset path of the scene this project opens, for example Assets/Games/.../Home.unity. It must be in the build settings.")]
        private string scenePath = "";
        [SerializeField, Tooltip("Model shown on the turntable. It must have no behaviours that need their own scene.")]
        private GameObject previewPrefab;
        [SerializeField, Tooltip("Picture on the card. Optional: without it the card shows its name alone.")]
        private Sprite thumbnail;
        [SerializeField, Tooltip("Hides the card and the project from the launcher without removing the entry, its scene or its code. Untick to show it again.")]
        private bool hidden;

        [SerializeField, Tooltip("Optional PlayerPrefs key set to Launch Value just before the scene loads, so the scene can open on the card that was chosen. The scene reads it and deletes it.")]
        private string launchKey = "";
        [SerializeField, Tooltip("Value stored under Launch Key when this card loads.")]
        private string launchValue = "";

        public string LaunchKey {
            get { return launchKey; }
        }

        public string LaunchValue {
            get { return launchValue; }
        }

        public bool Hidden {
            get { return hidden; }
        }

        public string Label {
            get { return label; }
        }

        public string Subtitle {
            get { return subtitle; }
        }

        public string ScenePath {
            get { return scenePath; }
        }

        public GameObject PreviewPrefab {
            get { return previewPrefab; }
        }

        public Sprite Thumbnail {
            get { return thumbnail; }
        }
    }
}
