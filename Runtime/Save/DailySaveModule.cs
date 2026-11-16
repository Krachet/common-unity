using System;
using Com.Krackhet.Runtime.Save.Cycles;
using Newtonsoft.Json;

namespace Com.Krackhet.Runtime.Save
{
    /// <summary>Save module that resets every local day.</summary>
    [Serializable]
    public abstract class DailySaveModule : CycledSaveModule
    {
        #region Interfaces & Properties
        [JsonIgnore]
        protected override IResetCycle ResetCycle => DailyResetCycle.Shared;
        #endregion
    }
}
