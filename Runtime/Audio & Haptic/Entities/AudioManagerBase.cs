using Com.Krackhet.Runtime.Managers;
using Com.Krackhet.Runtime.Pattern.Singleton;
using UnityEngine;
using UnityEngine.Audio;

namespace Com.Krackhet.Runtime.Audio
{
    public abstract class AudioManagerBase<T> : Singleton<T>, IAudioManager where T : AudioManagerBase<T> 
    {
        #region Serialized Fields
        [SerializeField]
        protected AudioMixer _audioMixer;

        [SerializeField]
        protected AudioSource _musicAudioSource;

        [SerializeField]
        protected AudioSource _sfxAudioSource;

        [SerializeField]
        protected AudioConfig _audioConfig;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            GameInternalManager.RegisterAudioManager(this);
            // Ensure that the audio sources are properly set up
            _musicAudioSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Music")[0];
            _sfxAudioSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Sound")[0];
        }

        public virtual void PlayAudio(AudioClip audioClip, AudioClipType audioType, float volume = 1f)
        {
            switch (audioType)
            {
                case AudioClipType.Sound:
                    _sfxAudioSource.PlayOneShot(audioClip, volume);
                    break;
                case AudioClipType.Music:
                    _musicAudioSource.clip = audioClip;
                    _musicAudioSource.volume = volume;
                    _musicAudioSource.Play();
                    break;
            }
        }

        public void PlayMusic(AudioClip audioClip, float volume = 1f)
        {
            var audioClipInfo = _audioConfig.GetAudioClip(audioClip.name, AudioClipType.Music);
            if (audioClipInfo != null)
            {
                PlayAudio(audioClipInfo, AudioClipType.Music, volume);
            }
            else
            {
                Debug.LogWarning($"Audio clip with name {audioClip.name} not found.");
            }
        }

        public void PlaySound(string clipName, float volume = 1f)
        {
            var audioClipInfo = _audioConfig.GetAudioClip(clipName, AudioClipType.Sound);
            if (audioClipInfo != null)
            {
                PlayAudio(audioClipInfo, AudioClipType.Sound, volume);
            }
            else
            {
                Debug.LogWarning($"Audio clip with name {clipName} not found.");
            }
        }

        public void StopAudio()
        {
            //Stop music and sound effects
            if (_musicAudioSource != null && _musicAudioSource.isPlaying)
            {
                _musicAudioSource.Stop();
            }
            if (_sfxAudioSource != null && _sfxAudioSource.isPlaying)
            {
                _sfxAudioSource.Stop();
            }
        }

        protected AudioClip GetAudioClipInfo(string clipName, AudioClipType clipType)
        {
            var audioClipInfo = _audioConfig.GetAudioClip(clipName, clipType);
            if (audioClipInfo != null)
            {
                return audioClipInfo;
            }
            else
            {
                Debug.LogWarning($"Audio clip info with name {clipName} not found.");
                return null;
            }
        }
    }
}