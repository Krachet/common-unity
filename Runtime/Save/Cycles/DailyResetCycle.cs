using System;
using System.Globalization;

namespace Com.Krackhet.Runtime.Save.Cycles
{
    public class DailyResetCycle : IResetCycle
    {
        #region Constants
        private const string KeyPrefix = "D:";
        #endregion

        #region Interfaces & Properties
        public static DailyResetCycle Shared { get; } = new DailyResetCycle();
        #endregion

        #region Public Methods
        public string GetCycleKey(DateTime now)
        {
            return KeyPrefix + now.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        #endregion
    }
}
