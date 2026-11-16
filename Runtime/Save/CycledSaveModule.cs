using System;
using Com.Krackhet.Runtime.Save.Cycles;
using Newtonsoft.Json;

namespace Com.Krackhet.Runtime.Save
{
    /// <summary>
    /// Save module whose data resets when its reset cycle rolls over.
    /// </summary>
    [Serializable]
    public abstract class CycledSaveModule : SaveModule
    {
        #region Interfaces & Properties
        [JsonIgnore]
        public string CycleKey => _cycleKey;

        [JsonIgnore]
        protected abstract IResetCycle ResetCycle { get; }
        #endregion

        #region Private Fields
        [JsonProperty("cycle_key")]
        private string _cycleKey;
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the cycle key for the given time. Returns true when the data was reset.
        /// The first stamp (fresh or legacy data) never resets.
        /// </summary>
        public bool RefreshCycle(DateTime now)
        {
            string currentKey = ResetCycle.GetCycleKey(now);
            if (_cycleKey == currentKey) return false;

            bool hasPreviousCycle = !string.IsNullOrEmpty(_cycleKey);
            _cycleKey = currentKey;
            if (!hasPreviousCycle) return false;

            OnCycleReset();
            return true;
        }
        #endregion

        #region Protected Methods
        /// <summary>Reset the module's fields back to their start-of-cycle values.</summary>
        protected abstract void OnCycleReset();
        #endregion
    }
}
