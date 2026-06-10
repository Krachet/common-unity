using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Com.Krackhet.Runtime.Utilities
{
    public static class GameHelper
    {
        #region Public Methods
        /// <summary>
        /// Returns the current local Unix timestamp adjusted for timezone and realtime.
        /// </summary>
        public static long Now()
        {
            long unixTimeSinceStartup = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            double adjustment = Time.realtimeSinceStartupAsDouble
                - TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds;
            return (long)(unixTimeSinceStartup - adjustment + Time.realtimeSinceStartupAsDouble);
        }

        /// <summary>
        /// Returns all non-abstract, non-generic classes deriving from <typeparamref name="T"/>
        /// across all loaded assemblies.
        /// </summary>
        public static Type[] GetAllDerivedClassesOf<T>() where T : class
        {
            Type inheritType = typeof(T);
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (Type type in assembly.GetTypes())
                if (type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition
                    && inheritType.IsAssignableFrom(type))
                    types.Add(type);
            return types.ToArray();
        }
        #endregion
    }
}
