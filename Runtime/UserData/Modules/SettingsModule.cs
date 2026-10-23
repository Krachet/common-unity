using System;
using System.Collections.Generic;

namespace Com.Krackhet.Runtime.UserData
{
    /// <summary>
    /// Boolean toggles keyed by name plus the ads purchase flag. Derive from it to add game-specific settings
    /// and register with <c>Register&lt;SettingsModule, MySettings&gt;()</c>.
    /// </summary>
    [Serializable]
    [SaveModule("settings")]
    public class SettingsModule : SaveModule
    {
        public const string Sound = "sound";
        public const string Music = "music";
        public const string Haptic = "haptic";

        public Dictionary<string, bool> toggles = new Dictionary<string, bool>();
        public bool noAds;

        public bool GetToggle(string key, bool defaultValue = true)
        {
            return toggles.TryGetValue(key, out bool value) ? value : defaultValue;
        }

        public void SetToggle(string key, bool enabled)
        {
            toggles[key] = enabled;
            RequestSave();

            switch (key)
            {
                case Sound:
                    GameEvents.RaiseSoundToggle(enabled);
                    break;
                case Music:
                    GameEvents.RaiseMusicToggle(enabled);
                    break;
                case Haptic:
                    GameEvents.RaiseVibrationToggle(enabled);
                    break;
            }
        }

        public void SetNoAds(bool value)
        {
            noAds = value;
            RequestSave();
        }
    }
}
