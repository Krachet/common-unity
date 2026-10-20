using UnityEngine;

namespace Com.Krackhet.Runtime.Haptic.Vibrate
{
    public interface IVibrateController
    {
        public void Vibrate(VibrationPresetType preset);
    }
}