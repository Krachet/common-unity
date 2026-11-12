using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Com.Krackhet.Runtime.Save
{
    public class PlayerPrefsSaveService : ISaveService
    {
        #region Constants
        public const string DefaultKeyPrefix = "Save.";
        #endregion

        #region Private Fields
        private readonly string _keyPrefix;
        #endregion

        #region Public Methods
        public PlayerPrefsSaveService(string keyPrefix = DefaultKeyPrefix)
        {
            _keyPrefix = keyPrefix ?? string.Empty;
        }

        public bool Has(string key)
        {
            return PlayerPrefs.HasKey(ToPrefsKey(key));
        }

        public void Save<T>(string key, T data)
        {
            string json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(ToPrefsKey(key), json);
        }

        public bool TryLoad<T>(string key, out T data)
        {
            data = default;
            string prefsKey = ToPrefsKey(key);
            if (!PlayerPrefs.HasKey(prefsKey)) return false;

            try
            {
                data = JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(prefsKey));
                return data != null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[PlayerPrefsSaveService] Failed to load '{prefsKey}': {exception.Message}");
                data = default;
                return false;
            }
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(ToPrefsKey(key));
        }

        public void Flush()
        {
            PlayerPrefs.Save();
        }
        #endregion

        #region Private Methods
        private string ToPrefsKey(string key)
        {
            return _keyPrefix + key;
        }
        #endregion
    }
}
