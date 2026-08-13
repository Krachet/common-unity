using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.TimeKeeper
{
    public class MockTimeProvider : TimeProvider
    {
        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Mock year (e.g., 2026)")]
#endif
        [SerializeField]
        private int _year;

#if UNITY_EDITOR
        [Tooltip("Mock month (1-12)")]
#endif
        [SerializeField]
        [Range(1, 12)]
        private int _month;

#if UNITY_EDITOR
        [Tooltip("Mock day (1-31)")]
#endif
        [SerializeField]
        [Range(1, 31)]
        private int _day;

#if UNITY_EDITOR
        [Tooltip("Mock hour (0-23)")]
#endif
        [SerializeField]
        [Range(0, 23)]
        private int _hour;

#if UNITY_EDITOR
        [Tooltip("Mock minute (0-59)")]
#endif
        [SerializeField]
        [Range(0, 59)]
        private int _minute;

#if UNITY_EDITOR
        [Tooltip("Mock second (0-59)")]
#endif
        [SerializeField]
        [Range(0, 59)]
        private int _second;
        #endregion

        #region Unity Callbacks
#if UNITY_EDITOR
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button("SetCurrentDate")]
        private void SetCurrentDate()
        {
            SetTime(DateTime.Now);
        }
#endif
#endif
        #endregion

        #region Interfaces & Properties
        public override DateTime now => BuildDateTime();
        public override DateTime nowUtc => BuildDateTime().ToUniversalTime();
        public override DateTime today => BuildDateTime().Date;
        #endregion

        #region Public Methods
        public void SetTime(DateTime dateTime)
        {
            _year = dateTime.Year;
            _month = dateTime.Month;
            _day = dateTime.Day;
            _hour = dateTime.Hour;
            _minute = dateTime.Minute;
            _second = dateTime.Second;
        }

        public void SetTime(
            int year,
            int month,
            int day,
            int hour = 0,
            int minute = 0,
            int second = 0
        )
        {
            _year = year;
            _month = month;
            _day = day;
            _hour = hour;
            _minute = minute;
            _second = second;
        }
        #endregion

        #region Private Methods
        private DateTime BuildDateTime()
        {
            int y = Mathf.Max(_year, 1);
            int m = Mathf.Clamp(_month, 1, 12);
            int d = Mathf.Clamp(_day, 1, DateTime.DaysInMonth(y, m));
            int h = Mathf.Clamp(_hour, 0, 23);
            int min = Mathf.Clamp(_minute, 0, 59);
            int s = Mathf.Clamp(_second, 0, 59);
            return new DateTime(y, m, d, h, min, s, DateTimeKind.Local);
        }
        #endregion
    }
}
