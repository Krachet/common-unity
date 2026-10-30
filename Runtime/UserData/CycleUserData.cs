using System;
using System.Globalization;
using Com.Krackhet.Runtime.TimeKeeper;
using Newtonsoft.Json;
using TimeKeeperService = Com.Krackhet.Runtime.TimeKeeper.TimeKeeper;

namespace Com.Krackhet.Runtime.Persistence
{
    public enum CycleType
    {
        Daily,
        Weekly
    }

    /// <summary>
    /// When a cycle starts. Defined in code and never saved, so changing it needs no migration.
    /// </summary>
    public readonly struct CyclePolicy
    {
        public readonly CycleType Type;
        public readonly float UtcOffsetHours;
        public readonly int ResetHour;
        public readonly Weekday WeeklyResetDay;

        public CyclePolicy(CycleType type, float utcOffsetHours = 0f, int resetHour = 0, Weekday weeklyResetDay = Weekday.Monday)
        {
            Type = type;
            UtcOffsetHours = utcOffsetHours;
            ResetHour = resetHour;
            WeeklyResetDay = weeklyResetDay;
        }

        public static CyclePolicy Daily(float utcOffsetHours = 0f, int resetHour = 0)
            => new CyclePolicy(CycleType.Daily, utcOffsetHours, resetHour);

        public static CyclePolicy Weekly(Weekday resetDay, float utcOffsetHours = 0f, int resetHour = 0)
            => new CyclePolicy(CycleType.Weekly, utcOffsetHours, resetHour, resetDay);

        /// <summary>Start date of the cycle that contains <paramref name="utcNow"/>, as yyyy-MM-dd.</summary>
        public string GetCycleKey(DateTime utcNow)
        {
            DateTime start = utcNow.AddHours(UtcOffsetHours - ResetHour).Date;

            if (Type == CycleType.Weekly)
                start = TimeKeeperService.GetWeekStart(start, WeeklyResetDay);

            return start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Save section that resets every day or week (see <see cref="Policy"/>).
    /// </summary>
    [Serializable]
    public abstract class CycleUserData : UserData
    {
        [JsonProperty("cycleKey")]
        private string _cycleKey;

        [JsonIgnore]
        public string CycleKey => _cycleKey;

        protected abstract CyclePolicy Policy { get; }

        internal override bool RefreshExpiration(DateTime utcNow)
        {
            string key = Policy.GetCycleKey(utcNow);
            if (key == _cycleKey)
                return false;

            OnReset();
            _cycleKey = key;
            return true;
        }
    }
}
