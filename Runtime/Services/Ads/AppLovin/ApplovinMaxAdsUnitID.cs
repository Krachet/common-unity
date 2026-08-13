using System;
using Unity.Android.Gradle;
using UnityEngine;

namespace Com.Krackhet.Runtime.Services.Ads
{
    [Serializable]
    public class ApplovinMaxAdsUnitID
    {
        public AdsType adsType;

        public string unitId
        {
            get
            {
#if UNITY_EDITOR
                return androidUnitId;
#elif UNITY_ANDROID
                return androidUnitId;
#elif UNITY_IOS
                return iosUnitId;
#endif
            }
        }

        [SerializeField]
        private string androidUnitId;
        [SerializeField]
        private string iosUnitId;
    }
}