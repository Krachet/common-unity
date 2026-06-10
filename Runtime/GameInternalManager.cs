using Com.Krackhet.Runtime.Audio;
using Com.Krackhet.Runtime.UI;
using UnityEngine;

namespace Com.Krackhet.Runtime.Managers
{
    public static class GameInternalManager
    {
        public static IAudioManager AudioManager { get; private set; }

        public static IUIManager UIManager { get; private set; }

        public static void RegisterAudioManager(IAudioManager audioManager)
        {
            if (AudioManager == null)
            {
                AudioManager = audioManager;
            }
            else
            {
                Debug.LogWarning("AudioManager is already registered. Ignoring new registration.");
            }
        }

        public static void RegisterUIManager(IUIManager uiManager)
        {
            if (UIManager == null)
            {
                UIManager = uiManager;
            }
            else
            {
                Debug.LogWarning("UIManager is already registered. Ignoring new registration.");
            }
        }
    }
}