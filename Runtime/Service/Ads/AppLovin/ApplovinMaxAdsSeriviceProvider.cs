using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Services.Ads
{
    [CreateAssetMenu(fileName = "ApplovinMaxAdsServiceProvider", menuName = "Krackhet/Services/Ads/ApplovinMaxAdsServiceProvider")]
    public class ApplovinMaxAdsSeriviceProvider : ScriptableObject, IAdsServiceProvider
    {
        [SerializeField]
        private List<ApplovinMaxAdsUnitID> _adsUnitIds;

        void Awake()
        {
        }

        public void HideBanner()
        {
            throw new NotImplementedException();
        }

        public void ShowAppOpen()
        {

        }

        public void ShowBanner()
        {
            throw new NotImplementedException();
        }

        public void ShowInterstitialAds(Action<bool> onShown)
        {
            throw new NotImplementedException();
        }

        public void ShowRewardAds(Action<bool> onShown)
        {
            throw new NotImplementedException();
        }

        private string TryGetStringByAdsType(AdsType type)
        {
            switch (type)
            {
                case AdsType.Banner:
                    return _adsUnitIds.Find(unitId => unitId.adsType == AdsType.Banner).unitId;
                case AdsType.Interstitial:
                    return _adsUnitIds.Find(unitId => unitId.adsType == AdsType.Interstitial).unitId;
                case AdsType.Reward:
                    return _adsUnitIds.Find(unitId => unitId.adsType == AdsType.Reward).unitId;
                case AdsType.AppOpen:
                    return _adsUnitIds.Find(unitId => unitId.adsType == AdsType.AppOpen).unitId;
                default:
                    return string.Empty;
            }
        }
    }
}