using UnityEngine;

namespace Com.Krackhet.Runtime.Audio
{
    public interface IAudioManager
    {
        AudioManagerStatus Status { get; }

        void PlayAudio(AudioClip audioClip, int audioType, float volume = 1f);
        void PlayAudio(string audioName, int audioType, float volume = 1f);
        void StopAudio();
    }
}