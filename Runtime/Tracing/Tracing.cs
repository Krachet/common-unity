using System;
using UnityEngine;

namespace Com.HorusGames.Untape.Runtime.Trace
{

    public static class Tracing
    {
        private static TracingSettings _settings;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_settings) return;
            try
            {
                _settings = Resources.Load<TracingSettings>("TracingSettings");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Tracing] Failed to load TracingSettings: {e}");
            }

            if (_settings == null)
            {
                _settings = ScriptableObject.CreateInstance<TracingSettings>();
                _settings.minimumSeverity = TracingSeverity.Info;
                _settings.EnsureSettings();
            }
        }

        // Resharper disable Unity.PerformanceAnalysis
        [HideInCallstack]
        private static void PushTrace(TracingSeverity severity, string message)
        {
#if !NO_LOGGING
            if (_settings == null)
            {
                Initialize();
            }

            if (severity < _settings.minimumSeverity)
            {
                return;
            }

#if UNITY_EDITOR
            TracingSettings.SeveritySettings severitySettings = _settings.severitySettings[(int)severity];
            var color = severitySettings != null ? severitySettings.color : Color.white;
            string logMessage = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>[{severity}]</color> {message}";
#else
            string logMessage = $"[{severity}] {message}";
#endif

            switch (severity)
            {
                case TracingSeverity.Trace:
                case TracingSeverity.Info:
                    UnityEngine.Debug.Log(logMessage);
                    break;
                case TracingSeverity.Warn:
                    UnityEngine.Debug.LogWarning(logMessage);
                    break;
                case TracingSeverity.Error:
                case TracingSeverity.Critical:
                case TracingSeverity.Fatal:
                    if (_settings.logWarningForErrorPriority)
                        UnityEngine.Debug.LogWarning(logMessage);
                    else
                        UnityEngine.Debug.LogError(logMessage);
                    break;
            }
#endif
        }

        [HideInCallstack]
        private static void PushTaggedTrace(TracingSeverity severity, string tag, string message)
        {
#if !NO_LOGGING
            PushTrace(severity, $"[{tag}] {message}");
#endif
        }

        [HideInCallstack]
        private static void PushTaggedTrace(TracingSeverity severity, string tag, string template, params object[] args)
        {
#if !NO_LOGGING
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] is not Func<string> func) continue;
                args[i] = func();
            }
            string message = string.Format(template, args);
            PushTaggedTrace(severity, tag, message);
#endif
        }

        [HideInCallstack]
        public static void Trace(string message) => PushTrace(TracingSeverity.Trace, message);

        [HideInCallstack]
        public static void Info(string message) => PushTrace(TracingSeverity.Info, message);

        [HideInCallstack]
        public static void Warn(string message) => PushTrace(TracingSeverity.Warn, message);

        [HideInCallstack]
        public static void Error(string message) => PushTrace(TracingSeverity.Error, message);

        [HideInCallstack]
        public static void Critical(string message) => PushTrace(TracingSeverity.Critical, message);

        [HideInCallstack]
        public static void Fatal(string message) => PushTrace(TracingSeverity.Fatal, message);


        [HideInCallstack]
        public static void Trace(string tag, string message) => PushTaggedTrace(TracingSeverity.Trace, tag, message);

        [HideInCallstack]
        public static void Info(string tag, string message) => PushTaggedTrace(TracingSeverity.Info, tag, message);

        [HideInCallstack]
        public static void Warn(string tag, string message) => PushTaggedTrace(TracingSeverity.Warn, tag, message);

        [HideInCallstack]
        public static void Error(string tag, string message) => PushTaggedTrace(TracingSeverity.Error, tag, message);

        [HideInCallstack]
        public static void Critical(string tag, string message) => PushTaggedTrace(TracingSeverity.Critical, tag, message);

        [HideInCallstack]
        public static void Fatal(string tag, string message) => PushTaggedTrace(TracingSeverity.Fatal, tag, message);


        [HideInCallstack]
        public static void Trace(string tag, string template, params object[] args) => PushTaggedTrace(TracingSeverity.Trace, tag, template, args);

        [HideInCallstack]
        public static void Info(string tag, string template, params object[] args) => PushTaggedTrace(TracingSeverity.Info, tag, template, args);

        [HideInCallstack]
        public static void Warn(string tag, string template, params object[] args) => PushTaggedTrace(TracingSeverity.Warn, tag, template, args);

        [HideInCallstack]
        public static void Error(string tag, string template, params object[] args) => PushTaggedTrace(TracingSeverity.Error, tag, template, args);

        [HideInCallstack]
        public static void Critical(string tag, string template, params object[] args) => PushTaggedTrace(TracingSeverity.Critical, tag, template, args);

        [HideInCallstack]
        public static void Fatal(string tag, string template, params object[] args) => PushTaggedTrace(TracingSeverity.Fatal, tag, template, args);


        [HideInCallstack]
        public static void Trace<T>(string template, params object[] args) => PushTaggedTrace(TracingSeverity.Trace, typeof(T).Name, template, args);

        [HideInCallstack]
        public static void Info<T>(string template, params object[] args) => PushTaggedTrace(TracingSeverity.Info, typeof(T).Name, template, args);

        [HideInCallstack]
        public static void Warn<T>(string template, params object[] args) => PushTaggedTrace(TracingSeverity.Warn, typeof(T).Name, template, args);

        [HideInCallstack]
        public static void Error<T>(string template, params object[] args) => PushTaggedTrace(TracingSeverity.Error, typeof(T).Name, template, args);

        [HideInCallstack]
        public static void Critical<T>(string template, params object[] args) => PushTaggedTrace(TracingSeverity.Critical, typeof(T).Name, template, args);

        [HideInCallstack]
        public static void Fatal<T>(string template, params object[] args) => PushTaggedTrace(TracingSeverity.Fatal, typeof(T).Name, template, args);


    }
}

