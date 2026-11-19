using UnityEngine;

namespace Com.Krackhet.Runtime.Achievements
{
    [CreateAssetMenu(menuName = "Krommon/Achievements/Counter Task")]
    public class CounterAchievementTask : AchievementTask
    {
        internal override long Combine(long current, long reported) =>
            reported > long.MaxValue - current ? long.MaxValue : current + reported;

        public static CounterAchievementTask Create(string id)
            => AchievementTask.Create<CounterAchievementTask>(id);
    }
}
