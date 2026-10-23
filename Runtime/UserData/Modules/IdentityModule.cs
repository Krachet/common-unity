using System;

namespace Com.Krackhet.Runtime.UserData
{
    [Serializable]
    [SaveModule("identity")]
    public class IdentityModule : SaveModule
    {
        public string userId = string.Empty;
        public string version = string.Empty;

        /// <summary>
        /// True on first launch or when the app version differs from the saved one (the saved version is then updated).
        /// </summary>
        public bool CheckNewVersion(string currentVersion, bool force = false)
        {
            bool isNew = version != currentVersion;
            if (isNew)
            {
                version = currentVersion;
                RequestSave();
            }

            return isNew || force;
        }
    }
}
