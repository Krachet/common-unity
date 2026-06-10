using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Com.Krackhet.Runtime.UI;

namespace Com.Krackhet.Editor.UI
{
    /// <summary>
    /// Project Settings provider that directly edits <see cref="BaseUIManagerConfiguration.layerGroupConfigurations"/>
    /// on the UIManagerConfiguration asset. Changes are written back to the ScriptableObject.
    /// Accessible via Edit > Project Settings > Krackhet > UI Layer Configuration.
    /// </summary>
    public class BaseUILayerGroupConfigurationSettingDrawer
    {
        #region Constants
        private const string SettingsPath = "Project/Krackhet/UI Layer Configuration";
        private const string DefaultAssetPath = "Assets/__TrickyQuest/Configs/UIManagerConfiguration.asset";
        private const string IgnoredPrefabPath = "Assets/Plugins/Krackhet/Runtime/UI/Prefabs/BaseUILayer.prefab";
        private const float FoldoutIndent = 15f;
        private const float LineHeight = 18f;
        #endregion

        #region Private Fields
        private static BaseUIManagerConfiguration _configuration;
        private static SerializedObject _serializedObject;
        private static ReorderableList _groupList;
        private static Vector2 _scrollPosition;
        private static string _assetPath;
        private static string _scanFolder = "Assets/";
        #endregion

        #region Settings Provider Registration
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(SettingsPath, SettingsScope.Project)
            {
                label = "UI Layer Configuration",
                guiHandler = DrawSettingsGUI,
                keywords = new HashSet<string>(
                    new[] { "UI", "Layer", "Krackhet", "Canvas", "Sorting", "Order" }
                ),
                deactivateHandler = HandleDeactivate,
            };
        }
        #endregion

        #region GUI Drawing
        private static void DrawSettingsGUI(string searchContext)
        {
            EnsureConfigurationLoaded();

            if (_configuration == null)
            {
                DrawMissingConfigurationGUI();
                return;
            }

            if (_serializedObject == null)
                _serializedObject = new SerializedObject(_configuration);

            _serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            DrawScanFolder();
            DrawGroupList();

            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_configuration);
                SyncLayerIndices(_configuration.layerGroupConfigurations);
            }
        }

        private static void DrawHeader()
        {
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Configuration Asset", EditorStyles.boldLabel, GUILayout.Width(140));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(_assetPath);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Locate", GUILayout.Width(60)))
                EditorGUIUtility.PingObject(_configuration);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
        }

        private static void DrawScanFolder()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scan Folder", EditorStyles.boldLabel, GUILayout.Width(140));
            _scanFolder = EditorGUILayout.TextField(_scanFolder);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Scan Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selected))
                {
                    string dataPath = System.IO.Path.GetFullPath(Application.dataPath);
                    string fullSelected = System.IO.Path.GetFullPath(selected);
                    if (fullSelected.StartsWith(dataPath))
                        _scanFolder = "Assets" + fullSelected.Substring(dataPath.Length).Replace("\\", "/") + "/";
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
        }

        private static void DrawMissingConfigurationGUI()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox(
                $"UIManagerConfiguration asset not found at:\n{DefaultAssetPath}\n\n"
                + "Create one via Assets > Create > Krackhet > UI > UIManagerConfiguration.",
                MessageType.Warning
            );

            if (GUILayout.Button("Create Configuration Asset", GUILayout.Height(36)))
                CreateConfigurationAsset();
        }

        private static void DrawGroupList()
        {
            if (_groupList == null)
                BuildGroupList();

            if (_groupList.count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No layer groups defined. Click \"Add Layer Group\" below to create one.",
                    MessageType.None
                );
            }

            _groupList.DoLayoutList();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Layer Group", GUILayout.Width(140), GUILayout.Height(26)))
            {
                _groupList.serializedProperty.arraySize++;
                _groupList.index = _groupList.count - 1;
                SerializedProperty newElement = _groupList.serializedProperty.GetArrayElementAtIndex(
                    _groupList.count - 1
                );
                newElement.FindPropertyRelative("groupOrder").intValue = _groupList.count - 1;
                newElement.FindPropertyRelative("layerPrefab").ClearArray();
                _serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_configuration);
            }

            GUILayout.Space(8);

            if (GUILayout.Button("Auto-Sort Layers into Groups", GUILayout.Width(200), GUILayout.Height(26)))
            {
                AutoSortLayersIntoGroups();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Reorderable List — Groups
        private static void BuildGroupList()
        {
            SerializedProperty groupsProperty =
                _serializedObject.FindProperty("layerGroupConfigurations");

            _groupList = new ReorderableList(
                _serializedObject,
                groupsProperty,
                draggable: true,
                displayHeader: true,
                displayAddButton: false,
                displayRemoveButton: true
            );

            _groupList.drawHeaderCallback = DrawGroupListHeader;
            _groupList.drawElementCallback = DrawGroupListElement;
            _groupList.elementHeightCallback = GetGroupElementHeight;
            _groupList.onRemoveCallback = HandleRemoveGroup;
        }

        private static void DrawGroupListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Layer Groups (drag to reorder)", EditorStyles.boldLabel);
        }

        private static void DrawGroupListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = _groupList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty groupOrderProp = element.FindPropertyRelative("groupOrder");
            SerializedProperty layerPrefabProp = element.FindPropertyRelative("layerPrefab");

            float y = rect.y + 2f;
            float width = rect.width;

            // Foldout header
            Rect foldoutRect = new Rect(rect.x, y, width, EditorGUIUtility.singleLineHeight);
            element.isExpanded = EditorGUI.Foldout(
                foldoutRect,
                element.isExpanded,
                GetGroupSummary(index, groupOrderProp, layerPrefabProp),
                true
            );

            if (!element.isExpanded) return;

            EditorGUI.indentLevel++;

            // Group Order field
            y += LineHeight + 2f;
            Rect orderRect = new Rect(rect.x, y, width, LineHeight);
            EditorGUI.PropertyField(orderRect, groupOrderProp, new GUIContent("Group Order"));

            // Layer prefabs — custom draw with Move buttons
            y += LineHeight + 4f;

            int arraySize = layerPrefabProp.arraySize;
            for (int i = 0; i < arraySize; i++)
            {
                SerializedProperty layerEntry = layerPrefabProp.GetArrayElementAtIndex(i);

                Rect entryRect = new Rect(rect.x + FoldoutIndent, y, width - FoldoutIndent, LineHeight);

                // Layer object field
                Rect objRect = new Rect(entryRect.x, entryRect.y, entryRect.width - 60f, LineHeight);
                EditorGUI.PropertyField(objRect, layerEntry, GUIContent.none, false);

                // Remove button
                Rect removeRect = new Rect(entryRect.x + entryRect.width - 55f, entryRect.y, 25f, LineHeight);
                if (GUI.Button(removeRect, "✕"))
                {
                    layerEntry.objectReferenceValue = null;
                    layerPrefabProp.DeleteArrayElementAtIndex(i);
                    _serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_configuration);
                    break; // array modified, stop iterating
                }

                // Move button
                Rect moveRect = new Rect(entryRect.x + entryRect.width - 25f, entryRect.y, 25f, LineHeight);
                if (GUI.Button(moveRect, "▾"))
                {
                    ShowMoveLayerMenu(index, i);
                }

                y += LineHeight + 2f;
            }

            // Add layer button
            Rect addRect = new Rect(rect.x + FoldoutIndent, y, 100f, LineHeight);
            if (GUI.Button(addRect, "+ Add Layer"))
            {
                layerPrefabProp.arraySize++;
                layerPrefabProp.GetArrayElementAtIndex(layerPrefabProp.arraySize - 1)
                    .objectReferenceValue = null;
                _serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_configuration);
            }

            EditorGUI.indentLevel--;
        }

        private static float GetGroupElementHeight(int index)
        {
            SerializedProperty element = _groupList.serializedProperty.GetArrayElementAtIndex(index);

            float height = LineHeight + 4f; // foldout header

            if (element.isExpanded)
            {
                SerializedProperty layerPrefabProp = element.FindPropertyRelative("layerPrefab");
                height += LineHeight + 4f; // group order field

                int arraySize = layerPrefabProp.arraySize;
                for (int i = 0; i < arraySize; i++)
                    height += LineHeight + 2f; // each layer row

                height += LineHeight + 4f; // add button row
            }

            return height;
        }

        private static void HandleRemoveGroup(ReorderableList list)
        {
            if (list.index < 0 || list.index >= list.count) return;

            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.index);
            string groupName = GetGroupSummary(
                list.index,
                element.FindPropertyRelative("groupOrder"),
                element.FindPropertyRelative("layerPrefab")
            );

            bool confirmed = EditorUtility.DisplayDialog(
                "Remove Layer Group",
                $"Are you sure you want to remove \"{groupName}\"?",
                "Remove",
                "Cancel"
            );

            if (confirmed)
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }
        #endregion

        #region Cross-Group Layer Movement
        private static void ShowMoveLayerMenu(int fromGroupIndex, int fromLayerIndex)
        {
            SerializedProperty groupsProp =
                _serializedObject.FindProperty("layerGroupConfigurations");

            GenericMenu menu = new GenericMenu();

            for (int g = 0; g < groupsProp.arraySize; g++)
            {
                if (g == fromGroupIndex) continue; // skip current group

                SerializedProperty targetGroup = groupsProp.GetArrayElementAtIndex(g);
                int targetOrder = targetGroup.FindPropertyRelative("groupOrder").intValue;
                int targetIndex = g; // capture for closure

                menu.AddItem(
                    new GUIContent($"Group {g} (Order {targetOrder})"),
                    false,
                    () => MoveLayerToGroup(fromGroupIndex, fromLayerIndex, targetIndex)
                );
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("No other groups available"));
            }

            menu.ShowAsContext();
        }

        private static void MoveLayerToGroup(
            int fromGroupIndex, int fromLayerIndex, int toGroupIndex)
        {
            SerializedProperty groupsProp =
                _serializedObject.FindProperty("layerGroupConfigurations");

            SerializedProperty fromGroup = groupsProp.GetArrayElementAtIndex(fromGroupIndex);
            SerializedProperty fromPrefabList = fromGroup.FindPropertyRelative("layerPrefab");
            SerializedProperty fromEntry = fromPrefabList.GetArrayElementAtIndex(fromLayerIndex);

            Object movedLayer = fromEntry.objectReferenceValue;

            // Remove from source
            fromEntry.objectReferenceValue = null;
            fromPrefabList.DeleteArrayElementAtIndex(fromLayerIndex);

            // Add to destination
            SerializedProperty toGroup = groupsProp.GetArrayElementAtIndex(toGroupIndex);
            SerializedProperty toPrefabList = toGroup.FindPropertyRelative("layerPrefab");
            toPrefabList.arraySize++;
            toPrefabList.GetArrayElementAtIndex(toPrefabList.arraySize - 1)
                .objectReferenceValue = movedLayer;

            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_configuration);
        }
        #endregion

        #region Asset Management
        private static void EnsureConfigurationLoaded()
        {
            if (_configuration != null) return;

            _configuration = AssetDatabase.LoadAssetAtPath<BaseUIManagerConfiguration>(
                DefaultAssetPath
            );
            if (_configuration != null)
            {
                _assetPath = DefaultAssetPath;
                _serializedObject = null;
                _groupList = null;
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:BaseUIManagerConfiguration");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _configuration = AssetDatabase.LoadAssetAtPath<BaseUIManagerConfiguration>(path);
                if (_configuration != null)
                {
                    _assetPath = path;
                    _serializedObject = null;
                    _groupList = null;
                    return;
                }
            }

            _configuration = null;
            _serializedObject = null;
            _groupList = null;
            _assetPath = null;
        }

        private static void CreateConfigurationAsset()
        {
            string directory = System.IO.Path.GetDirectoryName(DefaultAssetPath);
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            string fullPath = AssetDatabase.GenerateUniqueAssetPath(DefaultAssetPath);

            BaseUIManagerConfiguration newConfig =
                ScriptableObject.CreateInstance<BaseUIManagerConfiguration>();
            AssetDatabase.CreateAsset(newConfig, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _configuration = newConfig;
            _assetPath = fullPath;
            _serializedObject = null;
            _groupList = null;

            EditorGUIUtility.PingObject(newConfig);
        }

        private static void HandleDeactivate()
        {
            if (_serializedObject != null)
            {
                _serializedObject.ApplyModifiedProperties();
                SyncLayerIndices(_configuration.layerGroupConfigurations);
                _serializedObject.Dispose();
                _serializedObject = null;
            }

            _groupList = null;
        }
        #endregion

        #region Helpers
        private static string GetGroupSummary(
            int index,
            SerializedProperty groupOrderProp,
            SerializedProperty layerPrefabProp)
        {
            int order = groupOrderProp?.intValue ?? 0;
            int prefabCount = layerPrefabProp?.arraySize ?? 0;
            return $"Group {index}  —  Order: {order}  ·  Prefabs: {prefabCount}";
        }

        private static void SyncLayerIndices(
            List<BaseUILayerGroupConfiguration> groups)
        {
            if (groups == null) return;

            foreach (BaseUILayerGroupConfiguration group in groups)
            {
                if (group.layerPrefab == null) continue;

                foreach (BaseUILayer layer in group.layerPrefab)
                {
                    if (layer == null) continue;

                    string prefabPath = AssetDatabase.GetAssetPath(layer);
                    if (string.IsNullOrEmpty(prefabPath)) continue;

                    using (PrefabUtility.EditPrefabContentsScope scope =
                        new PrefabUtility.EditPrefabContentsScope(prefabPath))
                    {
                        SerializedObject prefabSo =
                            new SerializedObject(scope.prefabContentsRoot);
                        SerializedProperty indexProp = prefabSo.FindProperty("layerIndex");
                        if (indexProp != null)
                            indexProp.intValue = group.groupOrder;
                        prefabSo.ApplyModifiedProperties();
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }

        private static void AutoSortLayersIntoGroups()
        {
            Dictionary<int, List<BaseUILayer>> groupedLayers =
                new Dictionary<int, List<BaseUILayer>>();

            string scanFilter = string.IsNullOrWhiteSpace(_scanFolder)
                ? "Assets/" : _scanFolder.TrimEnd('/') + "/";

            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith(scanFilter)) continue;
                if (path == IgnoredPrefabPath) continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                BaseUILayer layer = prefab.GetComponent<BaseUILayer>();
                if (layer == null) continue;

                int index = layer.LayerIndex;
                if (!groupedLayers.ContainsKey(index))
                    groupedLayers[index] = new List<BaseUILayer>();
                groupedLayers[index].Add(layer);
            }

            if (groupedLayers.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Auto-Sort",
                    $"No BaseUILayer prefabs found in \"{scanFilter}\".",
                    "OK"
                );
                return;
            }

            _serializedObject.Update();
            SerializedProperty groupsProp =
                _serializedObject.FindProperty("layerGroupConfigurations");
            groupsProp.ClearArray();

            int groupIndex = 0;
            foreach (KeyValuePair<int, List<BaseUILayer>> kvp in groupedLayers)
            {
                groupsProp.arraySize = groupIndex + 1;
                SerializedProperty element = groupsProp.GetArrayElementAtIndex(groupIndex);
                element.FindPropertyRelative("groupOrder").intValue = kvp.Key;

                SerializedProperty prefabListProp =
                    element.FindPropertyRelative("layerPrefab");
                prefabListProp.ClearArray();
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    prefabListProp.arraySize = i + 1;
                    prefabListProp.GetArrayElementAtIndex(i).objectReferenceValue =
                        kvp.Value[i];
                }

                groupIndex++;
            }

            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_configuration);
            _groupList = null;

            int layerCount = 0;
            foreach (var list in groupedLayers.Values) layerCount += list.Count;

            EditorUtility.DisplayDialog(
                "Auto-Sort Complete",
                $"Sorted {layerCount} layer(s) into {groupedLayers.Count} group(s).",
                "OK"
            );
        }
        #endregion
    }
}
