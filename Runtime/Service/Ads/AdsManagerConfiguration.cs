using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.Services.Ads
{
    [CreateAssetMenu(
        fileName = "ApplovinMaxAdsSeriviceProvider", 
        menuName = "Krackhet/Services/Ads/AdsConfiguration")]

    public class AdsManagerConfiguration : ScriptableObject
    {
        public float interstitialAdsInterval;
    }
}