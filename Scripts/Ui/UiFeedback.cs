using System;

namespace ViitorCloud.KMaxLauncher.Ui {
    /// <summary>
    /// Hover and click cues raised by <see cref="UiButtonMotion"/>. The game subscribes to play its own
    /// sounds, so this assembly does not need to reference the game assembly.
    /// </summary>
    public static class UiFeedback {
        public static event Action Hover;
        public static event Action Click;

        public static void RaiseHover() {
            if (Hover != null) {
                Hover();
            }
        }

        public static void RaiseClick() {
            if (Click != null) {
                Click();
            }
        }
    }
}
