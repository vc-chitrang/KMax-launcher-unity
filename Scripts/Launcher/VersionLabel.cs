using TMPro;
using UnityEngine;

namespace ViitorCloud.KMaxLauncher {
    /// <summary>Prints the application version (set from the Game Info So at build time) on the text it sits on.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public class VersionLabel : MonoBehaviour {
        [SerializeField] private string prefix = "Version ";

        private void Awake() {
            GetComponent<TMP_Text>().text = prefix + Application.version;
        }
    }
}
