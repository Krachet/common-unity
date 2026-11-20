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
        [SerializeField] private string _title;
        [SerializeField] private string _description;
        public AchievementTask Task => _task;
        public long Target => _target;
        public string Title => _title;
        public string Description => _description;

        public AchievementRequirement(AchievementTask task, long target)
        {
            _task = task;
            _target = target;
            _title = string.Empty;
            _description = string.Empty;
        }

        public AchievementRequirement(AchievementTask task, long target, string title, string description)
        {
            _task = task;
            _target = target;
            _title = title;
            _description = description;
        }
    }
}
