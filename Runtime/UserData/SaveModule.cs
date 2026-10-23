using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Com.Krackhet.Runtime.UserData
{
    /// <summary>
    /// Base class of every saved block. Public fields and properties are serialized with Newtonsoft.
    /// Needs a public parameterless constructor and a <see cref="SaveModuleAttribute"/> (or explicit registration).
    /// </summary>
    [Serializable]
    public abstract class SaveModule
    {
        [JsonIgnore]
        internal SaveService Owner { get; set; }

        /// <summary>Bump when the serialized shape changes and implement <see cref="Migrate"/>.</summary>
        [JsonIgnore]
        public virtual int Version => 1;

        /// <summary>Called on the raw stored json when it was written by an older <see cref="Version"/>.</summary>
        public virtual void Migrate(int fromVersion, JObject raw)
        {
        }

        /// <summary>Called after the module was filled from storage.</summary>
        public virtual void OnLoaded()
        {
        }

        /// <summary>Persists all modules. Call after mutating data.</summary>
        protected void RequestSave()
        {
            Owner?.Save();
        }
    }
}
