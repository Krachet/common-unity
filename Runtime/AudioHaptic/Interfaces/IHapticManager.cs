using Com.Krackhet.Runtime.Haptic.Vibrate;
using UnityEngine;

namespace Com.Krackhet.Runtime.Haptic
{
    public interface IHapticManager
    {
        public void PlayVibrate(VibrationPresetType preset);
    }
}