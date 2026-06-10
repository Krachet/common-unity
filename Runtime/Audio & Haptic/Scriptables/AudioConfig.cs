using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Common/Audio/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        public List<AudioClipInfo> AudioClips;

        public AudioClip GetAudioClip(string clipName, AudioClipType clipType)
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
        public AudioClipType ClipType;
        public List<AudioClip> Clips;
    }

    public enum AudioClipType
    {
        Sound,
        Music,
    }
}