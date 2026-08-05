using Com.Krackhet.Runtime.Audio;
using Com.Krackhet.Runtime.UI;
using UnityEngine;

namespace Com.Krackhet.Runtime.Managers
{
    public static class GameInternalManager
    {
        public static IAudioManager AudioManager { get; private set; }

        public static IUIManager UIManager { get; private set; }

        internal static void RegisterManager<T>(T manager) where T : class
        {
            switch (manager)
            {
                case IAudioManager audioManager:
                    AudioManager = audioManager;
                    break;
                case IUIManager uiManager:
                    UIManager = uiManager;
                    break;
                default:
                    Debug.LogWarning($"Unknown manager type: {typeof(T).Name}. Registration ignored.");
                    break;
            }
        }

        internal static T GetManager<T>() where T : class
        {
            if (AudioManager        is T audioManager) return audioManager;
            if (UIManager           is T uiManager) return uiManager;
            return null;
        }
    }
}