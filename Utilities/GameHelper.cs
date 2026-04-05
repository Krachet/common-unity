using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Com.Krackhet.Runtime.Utilities
{
    public static class GameHelper
    {
        private static StringBuilder stringBuilder = new StringBuilder();
        private static List<RaycastResult> raycastResults = new List<RaycastResult>();
        private static PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        private static Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();
        private static Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealtime = new Dictionary<float, WaitForSecondsRealtime>();
        public static bool IsPointerOverGameObject(Vector2 screenPosition) => IsPointerOverGameObject(screenPosition, string.Empty);
        public static bool IsPointerOverGameObject(Vector2 screenPosition, string ignoreLayer)
        {
            raycastResults.Clear();
            if (EventSystem.current == null) return true;
            pointerEventData.position = screenPosition;
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            if (!string.IsNullOrWhiteSpace(ignoreLayer) && raycastResults.Count > 0)
            {
                int layer = LayerMask.NameToLayer(ignoreLayer);
                raycastResults.RemoveAll(item => item.gameObject.layer == layer);
            }
            return raycastResults.Count > 0;
        }
        public static WaitForSecondsRealtime GetWaitForSecondsRealtime(float seconds)
        {
            if (!waitForSecondsRealtime.ContainsKey(seconds))
                waitForSecondsRealtime.Add(seconds, new(seconds));
            return waitForSecondsRealtime[seconds];
        }
        public static WaitForSeconds GetWaitForSeconds(float seconds)
        {
            if (!waitForSeconds.ContainsKey(seconds))
                waitForSeconds.Add(seconds, new(seconds));
            return waitForSeconds[seconds];
        }
        public static Type[] GetAllDerivedClassesOf<T>() where T : class
        {
            Type inheritType = typeof(T);
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) foreach (Type type in assembly.GetTypes())
                if (type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition && inheritType.IsAssignableFrom(type)) types.Add(type);
            return types.ToArray();
        }
        public static string ConvertToDateTimeFormat(int totalSeconds)
        {
            stringBuilder.Clear();
            int days = totalSeconds / 86400;
            int hours = totalSeconds % 86400 / 3600;
            int seconds = totalSeconds % 60;
            int minutes = totalSeconds % 3600 / 60;
            if (days > 0) stringBuilder.Append($"{days}d");
            if (hours > 0) stringBuilder.Append($"{hours}h");
            if (minutes > 0)
            {
                if (hours > 0) stringBuilder.Append(" ");
                stringBuilder.Append($"{minutes}m");
            }
            if (seconds > 0)
            {
                if (minutes > 0) stringBuilder.Append(" ");
                stringBuilder.Append($"{seconds}s");
            }
            return stringBuilder.ToString();
        }
        public static string CreateText(string format, params object[] args)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            return stringBuilder.AppendFormat(format, args).ToString();
        }
        public static string CreateText(params object[] args)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            foreach (string arg in args) stringBuilder.Append(arg);
            return stringBuilder.ToString();
        }
        public static long Now()
        {
            long unixTimeSinceStartup = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            double adjustment = Time.realtimeSinceStartupAsDouble - TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds;
            return (long)(unixTimeSinceStartup - adjustment + Time.realtimeSinceStartupAsDouble);
        }

        public static string ConvertToDateTimeFormatTrim(int totalSeconds, int trimUnits)
        {
            stringBuilder.Clear();
            int days = totalSeconds / 86400;
            int hours = totalSeconds % 86400 / 3600;
            int seconds = totalSeconds % 60;
            int minutes = totalSeconds % 3600 / 60;
            int unitsAdded = 0;

            if (days > 0 && unitsAdded < trimUnits)
            {
                stringBuilder.Append($"{days}d");
                unitsAdded++;
            }
            if (hours > 0 && unitsAdded < trimUnits)
            {
                if (unitsAdded > 0) stringBuilder.Append(" ");
                stringBuilder.Append($"{hours}h");
                unitsAdded++;
            }
            if (minutes > 0 && unitsAdded < trimUnits)
            {
                if (unitsAdded > 0) stringBuilder.Append(" ");
                stringBuilder.Append($"{minutes}m");
                unitsAdded++;
            }
            if (seconds > 0 && unitsAdded < trimUnits)
            {
                if (unitsAdded > 0) stringBuilder.Append(" ");
                stringBuilder.Append($"{seconds}s");
                unitsAdded++;
            }
            return stringBuilder.ToString();
        }
    }
}