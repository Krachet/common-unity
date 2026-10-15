using Com.Krackhet.Runtime.Haptic.Vibrate;
using Com.Krackhet.Runtime.Pattern.Singleton;

namespace Com.Krackhet.Runtime.Haptic
{
    public abstract class HapticManagerBase<T> : Singleton<T>, IHapticManager where T : HapticManagerBase<T>
    {
        protected IVibrateController _vibrateController;

        protected bool _isVibrateEnabled;

        public bool IsVibrateEnabled
        {
            get => _isVibrateEnabled;
            set => _isVibrateEnabled = value;
        }

        protected override void Awake()
        {
            base.Awake();
#if MOREMOUNTAINS_NICEVIBRATIONS_INSTALLED
            _vibrateController = new FeelVibrateController();
#endif
        }

        public virtual void PlayVibrate(VibrationPresetType presetType)
        {
            if (_vibrateController != null)
            {
                _vibrateController.Vibrate(presetType);
            }
        }
    }
}