using System;

public static class GameEvents
{
    #region TIME
    public static event Action OnNewDay;
    public static event Action OnNewWeek;

    public static void RaiseNewDay()
    {
        OnNewDay?.Invoke();
    }

    public static void RaiseNewWeek()
    {
        OnNewWeek?.Invoke();
    }
    #endregion

    #region SETTINGS
    public static event Action<bool> OnMusicToggle;
    public static event Action<bool> OnSoundToggle;
    public static event Action<bool> OnVibrationToggle;
    public static event Action<int> OnControlOptionChange;

    public static event Action<int> OnCurrencyChanged; 

    public static void RaiseMusicToggle(bool enabled)
    {
        OnMusicToggle?.Invoke(enabled);
    }

    public static void RaiseSoundToggle(bool enabled)
    {
        OnSoundToggle?.Invoke(enabled);
    }

    public static void RaiseVibrationToggle(bool enabled)
    {
        OnVibrationToggle?.Invoke(enabled);
    }

    public static void RaiseChangeControl(int type)
    {
        OnControlOptionChange?.Invoke(type);
    }

    public static void RaiseCurrencyChanged(int amount)
    {
        OnCurrencyChanged?.Invoke(amount);
    }
    #endregion

    #region INTERACTIONS
    #endregion

    #region Monetization
    public static event Action<bool> OnToggleBanner;

    public static void RaiseToggleBanner(bool enabled)
    {
        OnToggleBanner?.Invoke(enabled);
    }
    #endregion
}
