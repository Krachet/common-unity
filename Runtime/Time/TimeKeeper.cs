using System;
using Com.Krackhet.Runtime.Pattern.Singleton;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com.Krackhet.Runtime.TimeKeeper
{
    public enum Weekday
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6
    }

    public class TimeKeeper : Singleton<TimeKeeper>
    {
        #region Events & Delegates
        public event Action DayChanged;
        #endregion

        #region Interfaces & Properties
        public TimeProvider TimeProvider => _timeProvider;

        public DateTime Today => _timeProvider != null
            ? _timeProvider.today
            : DateTime.Today;

        public DateTime Now => _timeProvider != null
            ? _timeProvider.now
            : DateTime.Now;

        public bool IsNewDay { get; private set; }

        private string CurrentDateKey => Today.ToString("yyyy-MM-dd");
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Assign RealTimeProvider or MockTimeProvider component from this GameObject")]
#endif
        [SerializeField]
        private TimeProvider _timeProvider;
        #endregion

        #region Private Fields
        private string _lastKnownDate;
        private bool _initialized;
        #endregion

        #region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();
            if (_timeProvider == null)
                _timeProvider = GetComponent<TimeProvider>();
        }
        #endregion

        #region Public Methods
        public async UniTask<bool> InitializeAsync(string lastKnownDate)
        {
            _lastKnownDate = string.IsNullOrEmpty(lastKnownDate)
                ? string.Empty
                : lastKnownDate;

            string todayKey = CurrentDateKey;
            IsNewDay = !string.IsNullOrEmpty(_lastKnownDate)
                && _lastKnownDate != todayKey;

            _initialized = true;

            await UniTask.Yield();

            if (IsNewDay)
            {
                DayChanged?.Invoke();
            }

            return IsNewDay;
        }

        public string GetCurrentDateString()
        {
            return CurrentDateKey;
        }

        public string GetCurrentWeekKey(Weekday resetDay)
        {
            return GetWeekStart(Today, resetDay).ToString("yyyy-MM-dd");
        }

        public bool CheckNewDay()
        {
            if (!_initialized)
                return false;

            string todayKey = CurrentDateKey;
            if (!string.IsNullOrEmpty(_lastKnownDate) && _lastKnownDate != todayKey)
            {
                IsNewDay = true;
                DayChanged?.Invoke();
                return true;
            }
            return false;
        }

        public static DateTime GetWeekStart(DateTime date, Weekday resetDay)
        {
            DayOfWeek target = (DayOfWeek)resetDay;
            int diff = (7 + (date.DayOfWeek - target)) % 7;
            return date.AddDays(-diff).Date;
        }
        #endregion
    }
}
