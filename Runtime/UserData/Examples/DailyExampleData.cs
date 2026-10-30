using System;

namespace Com.Krackhet.Runtime.Persistence.Examples
{
    /// <summary>
    /// Reference only, not used by any game. Resets every day at 00:00 UTC.
    /// To use one like it, add a field of its type to your UserDataRoot.
    /// </summary>
    [Serializable]
    public sealed class DailyExampleData : CycleUserData
    {
        public int rewardsClaimed;
        public bool spinUsed;

        protected override CyclePolicy Policy => CyclePolicy.Daily(utcOffsetHours: 0f, resetHour: 0);

        protected override void OnReset()
        {
            rewardsClaimed = 0;
            spinUsed = false;
        }
    }
}
