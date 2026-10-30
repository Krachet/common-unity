using System;
using Newtonsoft.Json;

namespace Com.Krackhet.Runtime.Persistence
{
    /// <summary>
    /// The single object that holds every save section. Each section is a field whose type
    /// derives from <see cref="UserData"/>. Adding a section only means adding a field.
    /// </summary>
    [Serializable]
    public abstract class UserDataRoot
    {
        [JsonProperty("version")]
        internal int version;

        [JsonProperty("lastSavedUtc")]
        internal DateTime lastSavedUtc;

        [JsonIgnore]
        public int Version => version;

        [JsonIgnore]
        public DateTime LastSavedUtc => lastSavedUtc;
    }
}
