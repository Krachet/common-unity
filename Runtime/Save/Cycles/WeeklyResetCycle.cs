using System;
using System.Globalization;
using Com.Krackhet.Runtime.TimeKeeping;

namespace Com.Krackhet.Runtime.Save.Cycles
{
    public class WeeklyResetCycle : IResetCycle
    {
        #region Constants
        private const string KeyPrefix = "W:";
        #endregion

        #region Interfaces & Properties
        public Weekday ResetDay { get; }
        #endregion

        #region Public Methods
        public WeeklyResetCycle(Weekday resetDay)
        {
            ResetDay = resetDay;
        }

        public string GetCycleKey(DateTime now)
        {
            DateTime weekStart = TimeKeeper.GetWeekStart(now, ResetDay);
            return KeyPrefix + weekStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        #endregion
    }
}
