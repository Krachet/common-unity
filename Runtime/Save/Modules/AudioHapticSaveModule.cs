using System;
using Newtonsoft.Json;

namespace Com.Krackhet.Runtime.Save.Modules
{
    /// <summary>
    /// Persists music, sound and vibration toggles.
    /// Mirrors AudioManagerBase.EnableMusic/EnableSound and HapticManagerBase.IsVibrateEnabled.
    /// </summary>
    [Serializable]
    public class AudioHapticSaveModule : SaveModule
    {
        #region Constants
        private const string SaveKey = "AudioHapticSettings";
        #endregion

        #region Interfaces & Properties
        [JsonIgnore]
        public override string Key => SaveKey;
        #endregion

        #region Serialized Fields
        [JsonProperty("music_enabled")]
        public bool isMusicEnabled = true;

        [JsonProperty("sound_enabled")]
        public bool isSoundEnabled = true;

        [JsonProperty("vibrate_enabled")]
        public bool isVibrateEnabled = true;
        #endregion
    }
}
