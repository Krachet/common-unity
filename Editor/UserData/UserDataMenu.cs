using System.Linq;
using Com.Krackhet.Runtime.Persistence;
using UnityEditor;
using UnityEngine;

namespace Com.Krackhet.Editor.Persistence
{
    internal static class UserDataMenu
    {
        [MenuItem("Tools/User Data/Clear")]
        private static void Clear()
        {
            IUserDataManager[] managers = FindManagers();
            if (managers.Length == 0)
                return;

            foreach (IUserDataManager manager in managers)
            {
                if (Application.isPlaying)
                    manager.DeleteAll();
                else
                    manager.Storage.Delete(manager.Key);

                Debug.Log($"[UserData] Cleared '{manager.Key}'.");
            }
        }

        [MenuItem("Tools/User Data/Log JSON")]
        private static void LogJson()
        {
            foreach (IUserDataManager manager in FindManagers())
            {
                string json = Application.isPlaying && manager.IsLoaded
                    ? manager.ToJson()
                    : manager.Storage.Exists(manager.Key) ? manager.Storage.Read(manager.Key) : "<no save>";

                Debug.Log($"[UserData] '{manager.Key}':\n{json}");
            }
        }

        private static IUserDataManager[] FindManagers()
        {
            IUserDataManager[] managers = Object
                .FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IUserDataManager>()
                .ToArray();

            if (managers.Length == 0)
                Debug.LogWarning("[UserData] No user data manager found in the open scenes.");

            return managers;
        }
    }
}
