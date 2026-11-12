using System;
using Com.Krackhet.Runtime.Save.Cycles;
using Com.Krackhet.Runtime.TimeKeeping;
using Newtonsoft.Json;

namespace Com.Krackhet.Runtime.Save
{
    /// <summary>Save module that resets every week on <see cref="ResetDay"/>.</summary>
    [Serializable]
    public abstract class WeeklySaveModule : CycledSaveModule
    {
        #region Interfaces & Properties
        /// <summary>Base reset day, assigned by SaveManagerBase before every cycle check.</summary>
        [JsonIgnore]
        public Weekday DefaultResetDay { get; internal set; } = Weekday.Monday;

        /// <summary>Override to use a module-specific reset day instead of the manager default.</summary>
        [JsonIgnore]
        protected virtual Weekday ResetDay => DefaultResetDay;

        [JsonIgnore]
        protected override IResetCycle ResetCycle
        {
            get
            {
                if (_resetCycle == null || _resetCycle.ResetDay != ResetDay)
                {
                    _resetCycle = new WeeklyResetCycle(ResetDay);
                }

                return _resetCycle;
            }
        }
        #endregion

        #region Private Fields
        [NonSerialized]
        private WeeklyResetCycle _resetCycle;
        #endregion
    }
}
