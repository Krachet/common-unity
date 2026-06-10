using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Krackhet.Runtime.Utilities
{
    public static class StringHelper
    {
        #region Private Fields
        private static readonly StringBuilder _stringBuilder = new StringBuilder();

        private static readonly string[] _currencyFormats =
        {
            string.Empty, "K", "M", "B", "T"
        };

        private static readonly Dictionary<string, int> _romanNumbers = new()
        {
            { "M", 1000 }, { "CM", 900 }, { "D", 500 }, { "CD", 400 },
            { "C", 100 }, { "XC", 90 }, { "L", 50 }, { "XL", 40 },
            { "X", 10 }, { "IX", 9 }, { "V", 5 }, { "IV", 4 }, { "I", 1 }
        };
        #endregion

        #region Public Methods
        public static string ConvertToDateTimeFormat(int totalSeconds)
        {
            _stringBuilder.Clear();
            int days = totalSeconds / 86400;
            int hours = totalSeconds % 86400 / 3600;
            int minutes = totalSeconds % 3600 / 60;
            int seconds = totalSeconds % 60;
            if (days > 0) _stringBuilder.Append($"{days}d");
            if (hours > 0) _stringBuilder.Append($"{hours}h");
            if (minutes > 0)
            {
                if (hours > 0) _stringBuilder.Append(" ");
                _stringBuilder.Append($"{minutes}m");
            }
            if (seconds > 0)
            {
                if (minutes > 0) _stringBuilder.Append(" ");
                _stringBuilder.Append($"{seconds}s");
            }
            return _stringBuilder.ToString();
        }

        public static string ConvertToDateTimeFormatTrim(int totalSeconds, int trimUnits)
        {
            _stringBuilder.Clear();
            int days = totalSeconds / 86400;
            int hours = totalSeconds % 86400 / 3600;
            int minutes = totalSeconds % 3600 / 60;
            int seconds = totalSeconds % 60;
            int unitsAdded = 0;

            if (days > 0 && unitsAdded < trimUnits)
            {
                _stringBuilder.Append($"{days}d");
                unitsAdded++;
            }
            if (hours > 0 && unitsAdded < trimUnits)
            {
                if (unitsAdded > 0) _stringBuilder.Append(" ");
                _stringBuilder.Append($"{hours}h");
                unitsAdded++;
            }
            if (minutes > 0 && unitsAdded < trimUnits)
            {
                if (unitsAdded > 0) _stringBuilder.Append(" ");
                _stringBuilder.Append($"{minutes}m");
                unitsAdded++;
            }
            if (seconds > 0 && unitsAdded < trimUnits)
            {
                if (unitsAdded > 0) _stringBuilder.Append(" ");
                _stringBuilder.Append($"{seconds}s");
                unitsAdded++;
            }
            return _stringBuilder.ToString();
        }

        public static string CreateText(string format, params object[] args)
        {
            _stringBuilder.Remove(0, _stringBuilder.Length);
            return _stringBuilder.AppendFormat(format, args).ToString();
        }

        public static string CreateText(params object[] args)
        {
            _stringBuilder.Remove(0, _stringBuilder.Length);
            foreach (string arg in args) _stringBuilder.Append(arg);
            return _stringBuilder.ToString();
        }
        #endregion

        #region Extension Methods
        public static string ToCurrencyFormats(this double number)
        {
            return number.ToCurrencyFormats(_currencyFormats);
        }

        public static string ToCurrencyFormats(this double number, string[] customFormats)
        {
            if (number >= 1000)
            {
                for (int i = 1; i < customFormats.Length; i++)
                {
                    double value = number / Math.Pow(10, 3 * i);
                    if (value >= 1000) continue;
                    value = Math.Round(value, value >= 100 ? 1 : 2);
                    return CreateText("{0}{1}", value, customFormats[i]);
                }
            }
            return Math.Round(number, 0, MidpointRounding.AwayFromZero).ToString();
        }

        public static string ToRoman(this int number)
        {
            string roman = string.Empty;
            foreach (KeyValuePair<string, int> item in _romanNumbers)
            {
                if (number <= 0) break;
                while (number >= item.Value)
                {
                    roman += item.Key;
                    number -= item.Value;
                }
            }
            return roman;
        }
        #endregion
    }
}
