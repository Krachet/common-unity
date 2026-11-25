using System;

namespace Com.Krackhet.Runtime.TimeKeeping
{
    public interface ITimeProvider
    {
        DateTime now { get; }
        DateTime nowUtc { get; }
        DateTime today { get; }
    }
}
