using System;
using Newtonsoft.Json;

namespace Com.Krackhet.Runtime.Persistence
{
    /// <summary>
    /// Save section that lives for a fixed length of time from when <see cref="Begin"/> is called
    /// (events, boosters, trials). Once past its end it is reset and stops.
    /// </summary>
    [Serializable]
    public abstract class ExpireUserData : UserData
    {
        [JsonProperty("startUtc")]
        private DateTime? _startUtc;

        [JsonProperty("durationTicks")]
        private long _durationTicks;

        [JsonIgnore]
        public DateTime? StartUtc => _startUtc;

        [JsonIgnore]
        public TimeSpan Duration => TimeSpan.FromTicks(_durationTicks);

        [JsonIgnore]
        public DateTime? EndUtc => StartUtc?.Add(Duration);

        [JsonIgnore]
        public bool IsActive => EndUtc.HasValue && UserDataClock.UtcNow < EndUtc.Value;

        [JsonIgnore]
        public TimeSpan Remaining
        {
            get
            {
                if (!EndUtc.HasValue)
                    return TimeSpan.Zero;

                TimeSpan remaining = EndUtc.Value - UserDataClock.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
        }

        /// <summary>Starts (or restarts) the lifetime from now. Call Save on the manager afterwards.</summary>
        public void Begin(TimeSpan duration)
        {
            _startUtc = UserDataClock.UtcNow;
            _durationTicks = duration.Ticks;
        }

        internal override bool RefreshExpiration(DateTime utcNow)
        {
            if (!EndUtc.HasValue || utcNow < EndUtc.Value)
                return false;

            OnReset();
            _startUtc = null;
            _durationTicks = 0;
            return true;
        }
    }
}
