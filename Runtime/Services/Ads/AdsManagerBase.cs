using System;
using Com.Krackhet.Runtime.Pattern.Singleton;
using UnityEngine;

namespace Com.Krackhet.Runtime.Services.Ads
{
    public enum AdsManagerStatus
    {
        NotInitialized,
        Initializing,
        Ready
    }

    public class AdsManagerBase<T> : Singleton<T>, IAdsManager where T : MonoBehaviour
    {
        #region Private Fields
        private string _deviceAdsId;

        private float _interstitialAdsInterval;

        private float _interstitialAdsCooldownProgress;

        private bool _noAds;

        private IAdsServiceProvider _currentProvider;

        private AdsManagerStatus _status;
        #endregion

        #region Interfaces
        public bool NoAds => _noAds;

        public string DeviceAdsId => _deviceAdsId;

        public bool InterstitialAdsReadyToShow => _interstitialAdsCooldownProgress >= _interstitialAdsInterval;

        public AdsManagerStatus Status => _status;
        #endregion

        #region Serialize Fields
        [SerializeField]
        private ScriptableObject _adsServiceProvider;
        #endregion

        #region Unity Methods
        protected override void Awake()
        {

        }

        protected virtual void Update()
        {
            if (_status == AdsManagerStatus.Ready)
                _interstitialAdsCooldownProgress += Time.unscaledDeltaTime;
        }
        #endregion

        #region Private Methods
        private void ResetProvider()
        {
            if (_adsServiceProvider == null) return;

            if (_adsServiceProvider is not IAdsServiceProvider provider) return;

            RegisterProvider(provider);
        }
        #endregion

        #region Internal Methods
        internal void RegisterProvider(IAdsServiceProvider adsServiceProvider)
        {
            if (adsServiceProvider != null)
                _currentProvider = adsServiceProvider;
        }
        #endregion

        #region Service Methods
        public void HideBanner()
        {
        }

        public void ShowBanner()
        {
        }

        public void ShowInterstitialAds(Action<bool> callback, string placement)
        {
        }

        public void ShowRewardAds(Action<bool> callback, string placement)
        {
        }

        public void ShowAppOpen()
        {

        }
        #endregion
    }
}