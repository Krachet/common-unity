using Com.Krackhet.Runtime.Managers;
using UnityEngine;

namespace Com.Krackhet.Runtime.Audio
{
    public interface IAudioManager: IGameInternal
    {
        void PlayAudio(AudioClip audioClip, int audioType, float volume = 1f);
        void PlayAudio(string audioName, int audioType, float volume = 1f);
        void StopAudio();
    }
}