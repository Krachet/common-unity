using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.Achievements
{
    [CreateAssetMenu(menuName = "Krommon/Achievements/Maximum Task")]
    public class MaximumAchievementTask : AchievementTask
    {
        internal override long Combine(long current, long reported) => Math.Max(current, reported);

        public static MaximumAchievementTask Create(string id)
            => AchievementTask.Create<MaximumAchievementTask>(id);
    }
}
