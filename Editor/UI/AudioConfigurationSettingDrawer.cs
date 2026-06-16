using UnityEditor;
using UnityEngine;
using Com.Krackhet.Runtime.Audio;

namespace Com.Krackhet.Editor.Audio
{
    public static class AudioConfigurationSettingDrawer
    {
        private const string SettingsPath = "Project/Krackhet/Audio Configuration";
        private const string ConfigAssetPath = "Assets/__TrickyQuest/Configs/AudioConfig.asset";

        private static SerializedObject _serializedConfig;
        private static Vector2 _scrollPosition;

        [SettingsProvider]
        public static SettingsProvider CreateAudioSettingsProvider()
        {
            return new SettingsProvider(SettingsPath, SettingsScope.Project)
            {
                label = "Audio Configuration",
                guiHandler = DrawSettingsGUI,
                keywords = new System.Collections.Generic.HashSet<string>(
                    new[] { "Audio", "Sound", "Music", "Clips", "Krackhet" }
                ),
            };
        }

        private static void DrawSettingsGUI(string searchContext)
        {
            AudioConfig config = LoadConfig();
            if (config == null)
            {
                EditorGUILayout.HelpBox(
                    $"AudioConfig asset not found at:\n{ConfigAssetPath}",
                    MessageType.Warning
                );
                return;
            }

            if (_serializedConfig == null || _serializedConfig.targetObject != config)
                _serializedConfig = new SerializedObject(config);

            _serializedConfig.Update();

            EditorGUI.BeginChangeCheck();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Audio Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Config Asset", config, typeof(AudioConfig), false);
            }

            EditorGUILayout.Space();

            SerializedProperty audioClipsProp = _serializedConfig.FindProperty("AudioClips");
            EditorGUILayout.PropertyField(audioClipsProp, true);

            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                _serializedConfig.ApplyModifiedProperties();
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
        }

        private static AudioConfig LoadConfig()
        {
            return AssetDatabase.LoadAssetAtPath<AudioConfig>(ConfigAssetPath);
        }
    }
}