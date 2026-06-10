using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Trace
{
    [CreateAssetMenu(fileName = "TracingSettings", menuName = "Common/Tracing Settings")]
    public class TracingSettings : ScriptableObject
    {
        public TracingSeverity minimumSeverity = TracingSeverity.Info;
        public bool logWarningForErrorPriority = false;
        public SeveritySettings[] severitySettings;

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureSettings();
        }
#endif

        public void EnsureSettings()
        {
            SeveritySettings[] newSettings = new SeveritySettings[Enum.GetValues(typeof(TracingSeverity)).Length];
            Dictionary<TracingSeverity, SeveritySettings> existingSettingsMap = new Dictionary<TracingSeverity, SeveritySettings>();
            if (severitySettings != null)
            {
                foreach (var setting in severitySettings)
                {
                    existingSettingsMap[setting.severity] = setting;
                }
            }

            foreach (TracingSeverity severity in Enum.GetValues(typeof(TracingSeverity)))
            {
                if (existingSettingsMap.ContainsKey(severity))
                {
                    newSettings[(int)severity] = existingSettingsMap[severity];
                }
                else
                {
                    newSettings[(int)severity] = new SeveritySettings
                    {
                        severity = severity,
                        color = severity switch
                        {
                            TracingSeverity.Trace => Color.gray,
                            TracingSeverity.Info => Color.white,
                            TracingSeverity.Warn => Color.yellow,
                            TracingSeverity.Error => Color.red,
                            TracingSeverity.Critical => new Color(1f, 0.5f, 0f), // Pinkish Orange
                            TracingSeverity.Fatal => new Color(0.5f, 0f, 0f), // Purple Red
                            _ => Color.white
                        }
                    };
                }
            }
            severitySettings = newSettings;
        }

        [Serializable]
        public class SeveritySettings
        {
            public TracingSeverity severity;
            public Color color;
        }
    }
}

