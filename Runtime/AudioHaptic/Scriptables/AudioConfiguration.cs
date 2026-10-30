using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Audio
{
    [CreateAssetMenu(fileName = "AudioConfiguration", menuName = "Krackhet/Audio/AudioConfiguration")]
    public class AudioConfiguration : ScriptableObject
    {
        public List<AudioClipInfo> AudioClips;

        public AudioClip GetAudioClip(string clipName, int clipType)
        {
            return GetAudioClip(clipName, (AudioClipType)clipType);
        }

        public AudioClip GetAudioClip(string clipName, AudioClipType type)
        {
            var audioClipInfo = AudioClips.Find(info => 
                info.ClipType == type && info.Clips.Exists(clip => clip.name == clipName));

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
        public AudioClipType ClipType; // 0 for Music, 1 for SoundFX, etc.
        public List<AudioClip> Clips;
    }

    public enum AudioClipType
    {
        Music,
        SoundFX
    }
}