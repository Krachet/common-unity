using System;
using Com.Krackhet.Runtime.Save;
using Newtonsoft.Json;

namespace Com.Krackhet.Unique.DailyReward
{
    /// <summary>
    /// Daily reward progress. Deliberately a plain module instead of a
    /// <see cref="DailySaveModule"/>: availability is derived by comparing the
    /// stored claim day against today, which stays correct across midnight
    /// while the app is running and needs no reset hook.
    /// </summary>
    [Serializable]
    public sealed class DailyRewardSaveModule : SaveModule
    {
        #region Constants
        private const string SaveKey = "DailyReward";
        #endregion

        #region Interfaces & Properties
        [JsonIgnore]
        public override string Key => SaveKey;

        [JsonIgnore]
        protected override int CurrentDataVersion => 1;
        #endregion

        #region Serialized Fields
        public int cycleIndex;
        public int dayIndex;
        public string lastClaimedDayKey;
        public string lastOpenedDayKey;
        #endregion

        #region Public Methods
        public void ResetProgress()
        {
            cycleIndex = 0;
            dayIndex = 0;
            lastClaimedDayKey = null;
            lastOpenedDayKey = null;
        }
        #endregion
    }
}
