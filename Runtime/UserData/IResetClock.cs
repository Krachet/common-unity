using System;
using TimeKeeperComponent = Com.Krackhet.Runtime.TimeKeeper.TimeKeeper;
using WeekdayType = Com.Krackhet.Runtime.TimeKeeper.Weekday;

namespace Com.Krackhet.Runtime.UserData
{
    /// <summary>
    /// Tells <see cref="SaveService"/> which period a scope is currently in. A scope resets when its key changes.
    /// </summary>
    public interface IResetClock
    {
        /// <returns>An opaque key for the current period (e.g. the date), or null if the scope never resets.</returns>
        string GetPeriodKey(ResetScope scope);
    }

    /// <summary>Default clock backed by <see cref="TimeKeeperComponent"/>, so mock time providers apply.</summary>
    public class TimeKeeperResetClock : IResetClock
    {
        public WeekdayType WeeklyResetDay { get; set; }

        public TimeKeeperResetClock(WeekdayType weeklyResetDay = WeekdayType.Monday)
        {
            WeeklyResetDay = weeklyResetDay;
        }

        public string GetPeriodKey(ResetScope scope)
        {
            TimeKeeperComponent timeKeeper = TimeKeeperComponent.Instance;
            switch (scope)
            {
                case ResetScope.Daily:
                    return timeKeeper != null
                        ? timeKeeper.GetCurrentDateString()
                        : DateTime.Today.ToString("yyyy-MM-dd");
                case ResetScope.Weekly:
                    return timeKeeper != null
                        ? timeKeeper.GetCurrentWeekKey(WeeklyResetDay)
                        : TimeKeeperComponent.GetWeekStart(DateTime.Today, WeeklyResetDay).ToString("yyyy-MM-dd");
                default:
                    return null;
            }
        }
    }
}
