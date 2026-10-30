using System;
using TimeKeeperService = Com.Krackhet.Runtime.TimeKeeper.TimeKeeper;

namespace Com.Krackhet.Runtime.Persistence
{
    /// <summary>
    /// Time source for every reset rule. Uses the TimeKeeper's provider when one is in
    /// the scene (so MockTimeProvider can move the clock), otherwise the system clock.
    /// </summary>
    public static class UserDataClock
    {
        public static DateTime UtcNow
        {
            get
            {
                TimeKeeperService keeper = TimeKeeperService.Instance;
                if (keeper != null && keeper.TimeProvider != null)
                    return keeper.TimeProvider.nowUtc;

                return DateTime.UtcNow;
            }
        }
    }
}
