using UnityEngine;
using ViitorCloud.KMaxLauncher.Ui;

namespace ViitorCloud.KMaxLauncher {
    /// <summary>
    /// Plays the interface hover and click cues raised by <see cref="UiButtonMotion"/>, using clips synthesised at start so
    /// the launcher needs no audio assets.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class LauncherAudio : MonoBehaviour {
        private const int SampleRate = 44100;

        [SerializeField, Range(0f, 1f)] private float volume = 0.7f;
        [SerializeField] private float hoverFrequency = 900f;
        [SerializeField] private float hoverSeconds = 0.02f;
        [SerializeField] private float clickFrequency = 587.33f;
        [SerializeField] private float clickSeconds = 0.085f;

        private AudioSource source;
        private AudioClip hoverClip;
        private AudioClip clickClip;

        private void Awake() {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            hoverClip = CreateTone("LauncherHover", hoverFrequency, hoverSeconds);
            clickClip = CreateTone("LauncherClick", clickFrequency, clickSeconds);
        }

        private void OnEnable() {
            UiFeedback.Hover += HandleHover;
            UiFeedback.Click += HandleClick;
        }

        private void OnDisable() {
            UiFeedback.Hover -= HandleHover;
            UiFeedback.Click -= HandleClick;
        }

        private void HandleHover() {
            source.PlayOneShot(hoverClip, 0.3f * volume);
        }

        private void HandleClick() {
            source.PlayOneShot(clickClip, 0.6f * volume);
        }

        private static AudioClip CreateTone(string clipName, float frequency, float seconds) {
            int count = Mathf.Max(1, Mathf.RoundToInt(seconds * SampleRate));
            float[] samples = new float[count];
            for (int i = 0; i < count; i++) {
                float time = (float)i / SampleRate;
                float envelope = 1f - (float)i / count;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * envelope * envelope;
            }

            AudioClip clip = AudioClip.Create(clipName, count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
