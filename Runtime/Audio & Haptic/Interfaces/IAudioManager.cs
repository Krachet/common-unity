using Com.Krackhet.Runtime.Managers;
using UnityEngine;

namespace Com.Krackhet.Runtime.Audio
{
    public interface IAudioManager: IGameInternal
    {
        void PlayAudio(AudioClip audioClip, AudioClipType audioType, float volume = 1f);
        void PlayMusic(AudioClip audioClip, float volume = 1f);
        void PlaySound(string clipName, float volume = 1f);
        void StopAudio();
    }
}