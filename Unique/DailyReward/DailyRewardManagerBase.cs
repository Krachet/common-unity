using System;
using Com.Krackhet.Runtime.Pattern.Singleton;
using Com.Krackhet.Runtime.Save.Cycles;
using Com.Krackhet.Runtime.TimeKeeping;
using UnityEngine;

namespace Com.Krackhet.Unique.DailyReward
{
    /// <summary>
    /// Owns the daily reward cycle: one gift per calendar day the player opens
    /// the game, a cycle of gifts, then the next cycle in the catalog.
    /// Days need not be consecutive, so progress never resets: the day slot
    /// only advances when a gift is actually claimed.
    /// Games supply persistence and granting through the abstract hooks.
    /// </summary>
    public abstract class DailyRewardManagerBase<TSelf> : Singleton<TSelf>
        where TSelf : DailyRewardManagerBase<TSelf>
    {
        #region Events & Delegates
        public event Action<DailyRewardPrize> Claimed;

        public event Action Changed;
        #endregion

        #region Interfaces & Properties
        public DailyRewardCatalog Catalog => _catalog;

        public int CurrentCycleIndex => Progress == null ? 0 : Progress.cycleIndex;

        public int CurrentDayIndex => Progress == null ? 0 : Progress.dayIndex;

        public DailyRewardCycle CurrentCycle => _catalog?.GetCycle(CurrentCycleIndex);

        public DailyRewardPrize CurrentPrize => GetPrize(CurrentDayIndex);

        public int DayCount => CurrentCycle?.DayCount ?? 0;

        /// <summary>
        /// True when today's gift has not been claimed yet. A fresh save has no
        /// stored claim day, so the first day is immediately claimable.
        /// </summary>
        public bool IsRewardAvailable
        {
            get
            {
                DailyRewardSaveModule progress = Progress;
                if (progress == null) return false;

                return progress.lastClaimedDayKey != TodayKey;
            }
        }

        /// <summary>
        /// True when today's gift is claimable and the player has not opened
        /// the daily reward board yet today.
        /// </summary>
        public bool HasUnseenReward
        {
            get
            {
                DailyRewardSaveModule progress = Progress;
                if (progress == null) return false;

                string today = TodayKey;
                return progress.lastClaimedDayKey != today
                    && progress.lastOpenedDayKey != today;
            }
        }

        /// <summary>
        /// Loaded progress module, or null while the save layer is not ready.
        /// </summary>
        protected abstract DailyRewardSaveModule Progress { get; }

        private string TodayKey => DailyResetCycle.Shared.GetCycleKey(CurrentTime);

        private DateTime CurrentTime =>
            TimeKeeper.Instance != null ? TimeKeeper.Instance.Now : DateTime.Now;

        private string LogTag => $"[{GetType().Name}]";
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Daily reward cycles granted to the player.")]
#endif
        [SerializeField]
        private DailyRewardCatalog _catalog;
        #endregion

        #region Public Methods
        public DailyRewardPrize GetPrize(int dayIndex)
        {
            return _catalog?.GetPrize(CurrentCycleIndex, dayIndex);
        }

        public DailyRewardState GetDayState(int dayIndex)
        {
            if (dayIndex < CurrentDayIndex) return DailyRewardState.Received;

            if (dayIndex == CurrentDayIndex && IsRewardAvailable)
            {
                return DailyRewardState.Receivable;
            }

            return DailyRewardState.Locked;
        }

        /// <summary>
        /// Records that the player opened the daily reward board today, which
        /// clears the notification until the next day.
        /// </summary>
        public void MarkOpened()
        {
            DailyRewardSaveModule progress = Progress;
            if (progress == null) return;

            string today = TodayKey;
            if (progress.lastOpenedDayKey == today) return;

            progress.lastOpenedDayKey = today;
            PersistOpened();
            Changed?.Invoke();
        }

        /// <summary>
        /// Grants today's gift. Returns false when nothing is claimable or when
        /// the grant was refused, in which case the day is kept.
        /// </summary>
        public bool TryClaim()
        {
            if (!HasValidCatalog()) return false;
            if (!IsRewardAvailable) return false;

            DailyRewardPrize prize = CurrentPrize;
            if (!IsPrizeGrantable(prize)) return false;
            if (!GrantPrize(prize)) return false;

            ConsumeDay();
            PersistClaim();

            Claimed?.Invoke(prize);
            Changed?.Invoke();
            return true;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Grants the prize to the player. Return false to keep the day unclaimed.
        /// </summary>
        protected abstract bool GrantPrize(DailyRewardPrize prize);

        /// <summary>Persists the granted prize and the advanced progress.</summary>
        protected abstract void PersistClaim();

        /// <summary>
        /// Persists the opened-today marker. Defaults to <see cref="PersistClaim"/>;
        /// override to save only the progress module.
        /// </summary>
        protected virtual void PersistOpened()
        {
            PersistClaim();
        }
        #endregion

        #region Private Methods
        private bool HasValidCatalog()
        {
            if (_catalog != null && _catalog.IsValid) return true;

            Debug.LogError($"{LogTag} Catalog is missing or empty.", this);
            return false;
        }

        private bool IsPrizeGrantable(DailyRewardPrize prize)
        {
            if (prize != null && prize.IsValid) return true;

            Debug.LogError(
                $"{LogTag} Cycle {CurrentCycleIndex} day {CurrentDayIndex} has no valid prize.",
                this
            );
            return false;
        }

        private void ConsumeDay()
        {
            DailyRewardSaveModule progress = Progress;
            progress.lastClaimedDayKey = TodayKey;
            progress.dayIndex++;

            if (progress.dayIndex < DayCount) return;

            progress.dayIndex = 0;
            progress.cycleIndex = Mathf.Min(
                progress.cycleIndex + 1,
                _catalog.CycleCount - 1
            );
        }
        #endregion
    }
}
