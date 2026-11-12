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
    }
}
