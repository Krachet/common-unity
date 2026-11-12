using System;

namespace Com.Krackhet.Runtime.Save.Cycles
{
    /// <summary>
    /// Maps a point in time to a cycle key. Data resets whenever the key changes.
    /// Implement this for new cycles (e.g. monthly).
    /// </summary>
    public interface IResetCycle
    {
        string GetCycleKey(DateTime now);
    }
}
