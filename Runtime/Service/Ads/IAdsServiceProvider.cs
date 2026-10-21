using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.Services.Ads
{
    public interface IAdsServiceProvider
    {
        void ShowInterstitialAds(Action<bool> onShown);
        void ShowRewardAds(Action<bool> onShown);
        void ShowBanner();
        void HideBanner();
        void ShowAppOpen();
    }
}