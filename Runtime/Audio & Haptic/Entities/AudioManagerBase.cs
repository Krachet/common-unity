using Com.Krackhet.Runtime.Managers;
using Com.Krackhet.Runtime.Pattern.Singleton;
using UnityEngine;
using UnityEngine.Audio;

namespace Com.Krackhet.Runtime.Audio
{
    public enum AudioManagerStatus
    {
        NotInitialized,
        Initializing,
        Ready
    }

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

        protected AudioManagerStatus _status;
        protected bool _isMusicEnabled;
        protected bool _isSoundEnabled;

        public AudioManagerStatus Status => _status;

        protected override void Awake()
        {
            base.Awake();
            _status = AudioManagerStatus.Initializing;
            GameInternalManager.RegisterManager(this);
            _musicAudioSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Music")[0];
            _sfxAudioSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Sound")[0];
            _status = AudioManagerStatus.Ready;
        }

        public virtual void PlayAudio(AudioClip audioClip, int audioType, float volume = 1f)
        {
            switch (audioType)
            {
                case 0: // Sound
                    _sfxAudioSource.PlayOneShot(audioClip, volume);
                    break;
                case 1: // Music
                    _musicAudioSource.clip = audioClip;
                    _musicAudioSource.volume = volume;
                    _musicAudioSource.loop = true;
                    _musicAudioSource.Play();
                    break;
            }
        }

        public virtual void PlayAudio(string audioName, int audioType, float volume = 1f)
        {
            var audioClip = GetAudioClipInfo(audioName, audioType);
            if (audioClip != null)
            {
                PlayAudio(audioClip, audioType, volume);
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

        public void EnableMusic(bool isEnabled)
        {
            _isMusicEnabled = isEnabled;
            _musicAudioSource.mute = !isEnabled;
        }

        public void EnableSound(bool isEnabled)
        {
            _isSoundEnabled = isEnabled;
            _sfxAudioSource.mute = !isEnabled;
        }

        protected AudioClip GetAudioClipInfo(string clipName, int clipType)
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