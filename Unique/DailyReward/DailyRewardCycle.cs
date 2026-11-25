using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Unique.DailyReward
{
    /// <summary>
    /// One full run of daily gifts. The player claims these in order, one per
    /// day they open the game, before moving on to the next cycle.
    /// </summary>
    [Serializable]
    public class DailyRewardCycle
    {
        #region Interfaces & Properties
        public string Id => string.IsNullOrEmpty(_id) ? "Unnamed cycle" : _id;

        public int DayCount => _prizes == null ? 0 : _prizes.Count;
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Editor-only label used to tell cycles apart.")]
#endif
        [SerializeField]
        private string _id;

#if UNITY_EDITOR
        [Tooltip("Gifts of this cycle, in claim order. Holds game-specific prize subclasses.")]
#endif
        [SerializeReference]
        private List<DailyRewardPrize> _prizes = new List<DailyRewardPrize>();
        #endregion

        #region Public Methods
        public DailyRewardPrize GetPrize(int dayIndex)
        {
            if (_prizes == null) return null;
            if (dayIndex < 0 || dayIndex >= _prizes.Count) return null;

            return _prizes[dayIndex];
        }
        #endregion
    }
}
