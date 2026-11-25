using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Unique.DailyReward
{
    /// <summary>
    /// Ordered set of daily reward cycles. Each completed cycle advances to the
    /// next one; the last cycle repeats forever.
    /// </summary>
    [CreateAssetMenu(
        menuName = "Krackhet/Daily Reward/Catalog",
        fileName = "DailyRewardCatalog"
    )]
    public class DailyRewardCatalog : ScriptableObject
    {
        #region Constants
        public const int DaysPerCycle = 7;
        #endregion

        #region Interfaces & Properties
        public int CycleCount => _cycles == null ? 0 : _cycles.Count;

        public bool IsValid => CycleCount > 0;
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Reward cycles in play order. The last entry repeats forever.")]
#endif
        [SerializeField]
        private List<DailyRewardCycle> _cycles = new List<DailyRewardCycle>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the cycle at the given index, clamped to the last cycle so
        /// players past the final cycle keep receiving it.
        /// </summary>
        public DailyRewardCycle GetCycle(int cycleIndex)
        {
            if (!IsValid) return null;

            int clamped = Mathf.Clamp(cycleIndex, 0, _cycles.Count - 1);
            return _cycles[clamped];
        }

        public DailyRewardPrize GetPrize(int cycleIndex, int dayIndex)
        {
            return GetCycle(cycleIndex)?.GetPrize(dayIndex);
        }
        #endregion

        #region Editor Code
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_cycles == null) return;

            for (int i = 0; i < _cycles.Count; i++)
            {
                WarnOnDayCountMismatch(i, _cycles[i]);
            }
        }

        private void WarnOnDayCountMismatch(int index, DailyRewardCycle cycle)
        {
            if (cycle == null || cycle.DayCount == DaysPerCycle) return;

            Debug.LogWarning(
                $"[{name}] Cycle {index} ({cycle.Id}) has {cycle.DayCount} "
                    + $"gifts, expected {DaysPerCycle}.",
                this
            );
        }
#endif
        #endregion
    }
}
