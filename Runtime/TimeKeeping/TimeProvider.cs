using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.TimeKeeping
{
    public abstract class TimeProvider : MonoBehaviour, ITimeProvider
    {
        public abstract DateTime now { get; }
        public abstract DateTime nowUtc { get; }
        public abstract DateTime today { get; }
    }
}
