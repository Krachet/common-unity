using System;

namespace Com.Krackhet.Runtime.Achievements
{
    [Serializable] public sealed class AchievementSaveData
    {
        public AchievementTaskProgress[] Tasks = Array.Empty<AchievementTaskProgress>();
        public AchievementUnlockSaveEntry[] Achievements = Array.Empty<AchievementUnlockSaveEntry>();
    }
    [Serializable] public sealed class AchievementUnlockSaveEntry
    {
        public string Id; public bool Unlocked; public string UnlockedAtUtc;
        /// <summary>Rewards were claimed. Missing in older payloads, which therefore load as unclaimed.</summary>
        public bool Claimed;
    }

    public interface IAchievementStore { AchievementSaveData Load(); void Save(AchievementSaveData data); }

}
