using System;

namespace Com.Krackhet.Runtime.Persistence.Examples
{
    /// <summary>
    /// Reference only, not used by any game. A 7-day event: call <c>Begin(TimeSpan.FromDays(7))</c>
    /// when the player joins, then Save. It resets by itself once the 7 days are over.
    /// </summary>
    [Serializable]
    public sealed class EventExampleData : ExpireUserData
    {
        public static readonly TimeSpan EventLength = TimeSpan.FromDays(7);

        public int points;

        protected override void OnReset()
        {
            points = 0;
        }
    }
}
