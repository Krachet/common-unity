using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Audio
{
    [CreateAssetMenu(fileName = "AudioConfiguration", menuName = "Krackhet/Audio/AudioConfiguration")]
    public class AudioConfig : ScriptableObject
    {
        public List<AudioClipInfo> AudioClips;

        public AudioClip GetAudioClip(string clipName, int clipType)
        {
            var audioClipInfo = AudioClips.Find(info => info.ClipType == clipType && info.Clips.Exists(clip => clip.name == clipName));
            if (audioClipInfo != null)
            {
                return audioClipInfo.Clips.Find(clip => clip.name == clipName);
            }
            return null;
        }
    }

    [Serializable]
    public class AudioClipInfo
    {
        public int ClipType; // 0 for SFX, 1 for Music, etc.
        public List<AudioClip> Clips;
    }
}