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

        private AdsManagerStatus _status;
        #endregion

        #region Interfaces
        public string DeviceAdsId => _deviceAdsId;

        public AdsManagerStatus Status => _status;
        #endregion

        public void HideBanner()
        {
        }

        public void ShowBanner()
        {
        }

        public void ShowInterstitialAds(Action<bool> onShown)
        {
        }

        public void ShowRewardedAds(Action<bool> onShown)
        {
        }
    }
}