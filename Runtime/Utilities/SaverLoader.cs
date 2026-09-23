using Newtonsoft.Json;
using UnityEngine;

namespace WA.Runtime.Utilities
{
    public static class SaverLoader
    {
        public static void SaveData<T>(string key, T data) where T : class, new()
        {
            string jsonData = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, jsonData);
            PlayerPrefs.Save();
        }

        public static T LoadData<T>(string key) where T : class, new()
        {
            if (PlayerPrefs.HasKey(key))
            {
                string jsonData = PlayerPrefs.GetString(key);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }
            else
            {
                return null;
            }
        }
    }
}