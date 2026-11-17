using System;
using Newtonsoft.Json;

namespace Com.Krackhet.Runtime.Save
{
    /// <summary>
    /// Base type for a block of persisted data. Add serializable fields in derived classes.
    /// </summary>
    [Serializable]
    public abstract class SaveModule
    {
        #region Interfaces & Properties
        [JsonIgnore]
        public virtual string Key => GetType().Name;

        [JsonProperty("dataVersion")]
        public int DataVersion { get; private set; }

        [JsonIgnore]
        protected virtual int CurrentDataVersion => 1;
        #endregion

        #region Public Methods
        /// <summary>Called when no saved data exists and a fresh instance is created.</summary>
        public virtual void OnCreated()
        {
        }

        /// <summary>Called after the module was restored from storage.</summary>
        public virtual void OnLoaded()
        {
        }

        /// <summary>Called right before the module is written to storage.</summary>
        public virtual void OnBeforeSave()
        {
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Migrates data from the stored version to the current version.
        /// </summary>
        protected virtual void MigrateData(int fromVersion)
        {
        }
        #endregion

        #region Internal Methods
        internal bool HasUnsupportedDataVersion => DataVersion > CurrentDataVersion;

        internal bool UpgradeDataVersion()
        {
            if (HasUnsupportedDataVersion || DataVersion == CurrentDataVersion)
            {
                return false;
            }

            MigrateData(DataVersion);
            DataVersion = CurrentDataVersion;
            return true;
        }

        internal void InitializeDataVersion()
        {
            DataVersion = CurrentDataVersion;
        }
        #endregion
    }
}
