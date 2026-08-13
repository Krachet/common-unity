using System;

namespace Com.Krackhet.Runtime.Services.Ads
{
    public interface IAdsManager
    {
        string DeviceAdsId { get; }
        AdsManagerStatus Status { get; }

        void ShowInterstitialAds(Action<bool> onShown, string placement);
        void ShowRewardAds(Action<bool> onShown, string placement);
        void ShowBanner();
        void HideBanner();
    }
}