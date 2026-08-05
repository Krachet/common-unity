using System;

namespace Com.Krackhet.Runtime.Services.Ads
{
    public interface IAdsManager
    {
        string DeviceAdsId { get; }
        AdsManagerStatus Status { get; }

        void ShowInterstitialAds(Action<bool> onShown);
        void ShowRewardedAds(Action<bool> onShown);
        void ShowBanner();
        void HideBanner();
    }
}