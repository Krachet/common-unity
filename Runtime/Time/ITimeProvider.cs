using System;

namespace Com.Krackhet.Runtime.TimeKeeper
{
    public interface ITimeProvider
    {
        DateTime now { get; }
        DateTime nowUtc { get; }
        DateTime today { get; }
    }
}
