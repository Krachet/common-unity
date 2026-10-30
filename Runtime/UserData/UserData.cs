using System;

namespace Com.Krackhet.Runtime.Persistence
{
    internal interface IUserDataOwner
    {
        void Save();
        void MarkDirty();
    }

    /// <summary>
    /// Persistent save section: one field on the <see cref="UserDataRoot"/>. It is never reset
    /// automatically. Use <see cref="CycleUserData"/> or <see cref="ExpireUserData"/> for data
    /// that expires.
    /// Public fields (or members marked [JsonProperty]) are saved.
    /// </summary>
    [Serializable]
    public abstract class UserData
    {
        private IUserDataOwner _owner;

        /// <summary>Puts every field back to its default. Also runs when the section is first created.</summary>
        protected abstract void OnReset();

        /// <summary>Saves the whole user data now.</summary>
        protected void RequestSave() => _owner?.Save();

        /// <summary>Saves the whole user data on the next pause, focus loss, or quit.</summary>
        protected void MarkDirty() => _owner?.MarkDirty();

        internal void Bind(IUserDataOwner owner) => _owner = owner;

        internal void ResetToDefaults() => OnReset();

        /// <summary>Applies the section's reset rule. Returns true when the data was reset.</summary>
        internal virtual bool RefreshExpiration(DateTime utcNow) => false;
    }
}
