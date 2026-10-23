namespace Com.Krackhet.Runtime.UserData
{
    /// <summary>
    /// When a <see cref="SaveModule"/> is wiped back to its defaults.
    /// To add a scope (monthly, season...) add a value here and handle it in <see cref="IResetClock.GetPeriodKey"/>.
    /// </summary>
    public enum ResetScope
    {
        Never = 0,
        Daily = 1,
        Weekly = 2,
        /// <summary>Lives in memory only, never written to disk.</summary>
        Session = 3
    }
}
