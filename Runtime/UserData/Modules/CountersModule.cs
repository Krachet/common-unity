using System;
using System.Collections.Generic;

namespace Com.Krackhet.Runtime.UserData
{
    /// <summary>
    /// Named integer counters for one-off values that don't deserve their own class.
    /// Use <see cref="CountersModule"/> for permanent counters, <see cref="DailyCountersModule"/> and
    /// <see cref="WeeklyCountersModule"/> for ones that reset (e.g. ads watched today).
    /// </summary>
    [Serializable]
    [SaveModule("counters")]
    public class CountersModule : SaveModule
    {
        public Dictionary<string, int> values = new Dictionary<string, int>();

        public int Get(string key)
        {
            return values.TryGetValue(key, out int value) ? value : 0;
        }

        public int Add(string key, int amount = 1)
        {
            int value = Get(key) + amount;
            values[key] = value;
            RequestSave();
            return value;
        }

        public void Set(string key, int value)
        {
            values[key] = value;
            RequestSave();
        }

        /// <summary>Increments only while the counter is below <paramref name="max"/>.</summary>
        public bool TryIncrement(string key, int max)
        {
            if (Get(key) >= max) return false;

            Add(key);
            return true;
        }
    }

    [Serializable]
    [SaveModule("counters_daily", ResetScope.Daily)]
    public class DailyCountersModule : CountersModule
    {
    }

    [Serializable]
    [SaveModule("counters_weekly", ResetScope.Weekly)]
    public class WeeklyCountersModule : CountersModule
    {
    }
}
