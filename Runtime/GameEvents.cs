using System;
using WA.Runtime.Levels;

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

    #region LEVELS
    public static event Action<int> OnLevelStart;
    public static event Action<int, LevelResult> OnLevelEnd;
    public static event Action<bool> OnLevelPause;
    public static event Action<int> OnLevelResume;
    public static event Action<int> OnLevelUnlocked;

    public static void RaiseLevelStart(int levelId)
    {
        OnLevelStart?.Invoke(levelId);
    }

    public static void RaiseLevelEnd(int levelId, LevelResult result)
    {
        OnLevelEnd?.Invoke(levelId, result);
    }

    public static void RaiseLevelPause(bool isPaused)
    {
        OnLevelPause?.Invoke(isPaused);
    }

    public static void RaiseLevelUnlocked(int levelId)
    {
        OnLevelUnlocked?.Invoke(levelId);
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
