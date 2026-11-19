using System;
using UnityEngine;

namespace ViitorCloud.KMaxLauncher {
    [CreateAssetMenu(fileName = "LauncherCatalog", menuName = "KMax/Launcher Catalog")]
    public class LauncherCatalog : ScriptableObject {
        [SerializeField] private string title = "EXPLORE IN 3D";
        [SerializeField, Tooltip("Wording of the Load button. {0} is the chosen project's name.")]
        private string loadFormat = "Load {0}";
        [SerializeField] private LauncherProjectEntry[] entries = new LauncherProjectEntry[0];

        public string Title {
            get { return title; }
        }

        public string LoadFormat {
            get { return loadFormat; }
        }

        /// <summary>The number of projects the launcher shows; entries marked hidden are kept in the catalog but not counted.</summary>
        public int Count {
            get {
                int visible = 0;
                for (int i = 0; i < entries.Length; i++) {
                    if (!entries[i].Hidden) {
                        visible++;
                    }
                }

                return visible;
            }
        }

        /// <summary>The index-th project the launcher shows, skipping hidden entries.</summary>
        public LauncherProjectEntry Get(int index) {
            int seen = -1;
            for (int i = 0; i < entries.Length; i++) {
                if (entries[i].Hidden) {
                    continue;
                }

                seen++;
                if (seen == index) {
                    return entries[i];
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index), $"The catalog shows {seen + 1} projects, not {index + 1}.");
        }
    }
}
