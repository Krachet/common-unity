using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.Achievements
{
    public enum AchievementRequirementPolicy { All, Any }

    [Serializable]
    public sealed class AchievementRequirement
    {
        [SerializeField] private AchievementTask _task;
        [SerializeField] private long _target = 1;
        public AchievementTask Task => _task;
        public long Target => _target;

        public AchievementRequirement(AchievementTask task, long target)
        {
            _task = task;
            _target = target;
        }
    }
}
