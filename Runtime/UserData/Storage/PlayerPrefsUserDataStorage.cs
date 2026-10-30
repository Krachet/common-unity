using UnityEngine;

namespace Com.Krackhet.Runtime.Persistence
{
    public sealed class PlayerPrefsUserDataStorage : IUserDataStorage
    {
        public bool Exists(string key) => PlayerPrefs.HasKey(key);

        public string Read(string key) => PlayerPrefs.GetString(key, null);

        public void Write(string key, string data)
        {
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
