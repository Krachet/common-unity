using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Com.HorusGames.Runtime.Avatar
{
    [CreateAssetMenu(fileName = "AvatarConfig", menuName = "Common Config/User/Avatar Config", order = 1)]
    public class AvatarConfig : ScriptableObject
    {
        public List<AvatarIconConfig> avaIconSprites;
        public List<AvatarFrameConfig> avaFrameSprites;

        public const string k_ConfigName = "AvatarConfig"; 

#if UNITY_EDITOR
        [Sirenix.OdinInspector.PropertySpace(10)]
        [Sirenix.OdinInspector.Button("Load Ava Icon Sprites")]
        public void LoadAvaIconSprites(DefaultAsset folder)
        {
            string folderPath = AssetDatabase.GetAssetPath(folder);
            string[] assetGUIDs = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

            avaIconSprites.Clear();
            foreach (string guid in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                {
                    avaIconSprites.Add(new AvatarIconConfig { iconSprite = sprite, cost = 0, initialUnlocked = false });
                }
            }
        }

        [Sirenix.OdinInspector.Button("Load Ava Frame Sprites")]
        public void LoadAvaFrameSprites(DefaultAsset folder)
        {
            string folderPath = AssetDatabase.GetAssetPath(folder);
            string[] assetGUIDs = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

            avaFrameSprites.Clear();
            foreach (string guid in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                {
                    avaFrameSprites.Add(new AvatarFrameConfig { frameSprite = sprite, cost = 0, initialUnlocked = false });
                }
            }

        }
#endif

        public Sprite GetAvatarIcon(int avaIndex)
        {
            return avaIndex >= 0 && avaIndex < avaIconSprites.Count ? avaIconSprites[avaIndex].iconSprite : null;
        }

        public Sprite GetAvatarFrame(int avaIndex)
        {
            return avaIndex >= 0 && avaIndex < avaFrameSprites.Count ? avaFrameSprites[avaIndex].frameSprite : null;
        }
    }

    [Serializable]
    public struct AvatarIconConfig
    {
        public Sprite iconSprite;
        public int cost;
        public bool initialUnlocked;
    }

    [Serializable]
    public struct AvatarFrameConfig
    {
        public Sprite frameSprite;
        public int cost;
        public bool initialUnlocked;

    }
}