using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.TimeKeeping
{
    public class RealTimeProvider : TimeProvider
    {
        public override DateTime now => DateTime.Now;
        public override DateTime nowUtc => DateTime.UtcNow;
        public override DateTime today => DateTime.Today;
    }
}
