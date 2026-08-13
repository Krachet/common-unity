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

        public void HideBanner()
        {
            throw new NotImplementedException();
        }

        public void ShowAppOpen()
        {
            throw new NotImplementedException();
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
    }
}