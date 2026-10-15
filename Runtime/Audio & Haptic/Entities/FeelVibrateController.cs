using Lofelt.NiceVibrations;

namespace Com.Krackhet.Runtime.Haptic.Vibrate
{
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
    public class FeelVibrateController : IVibrateController
    {
        public void Vibrate(VibrationPresetType preset)
        {
            HapticPatterns.PlayPreset(FeelPresetConverter(preset));
        }

        private static HapticPatterns.PresetType FeelPresetConverter(VibrationPresetType presetType)
        {
            switch (presetType)
            {
                case VibrationPresetType.ExtraLight:
                    return HapticPatterns.PresetType.SoftImpact;
                case VibrationPresetType.Light:
                    return HapticPatterns.PresetType.LightImpact;
                case VibrationPresetType.Normal:
                    return HapticPatterns.PresetType.MediumImpact;
                case VibrationPresetType.Intense:
                    return HapticPatterns.PresetType.HeavyImpact;
                case VibrationPresetType.Button:
                    return HapticPatterns.PresetType.Selection;
                case VibrationPresetType.Win:
                    return HapticPatterns.PresetType.Success;
                case VibrationPresetType.Lose:
                    return HapticPatterns.PresetType.Failure;
                default:
                    return HapticPatterns.PresetType.None;
            }
        }
#endif
    }
}