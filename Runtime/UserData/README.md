# UserData

A game-agnostic save system. Data is split into **modules** (plain classes), each with its own save key,
reset scope and version. The game decides which modules exist by registering them explicitly.

Namespace: `Com.Krackhet.Runtime.UserData`. Storage: one PlayerPrefs entry (`"UserSave"`) written as JSON
through `SaverLoader` (Newtonsoft).

## Pieces

| Type | Role |
|---|---|
| `SaveModule` | Base class of every saved block. |
| `[SaveModule("key", ResetScope)]` | Gives a module its save key and reset scope. |
| `SaveService` | Registry, load/save, resets. No game references. |
| `UserDataManagerBase<T>` | MonoBehaviour singleton each game subclasses. |
| `IResetClock` / `TimeKeeperResetClock` | Decides when a day/week changed (uses `TimeKeeper`, so mock time works). |
| `ResetScope` | `Never`, `Daily`, `Weekly`, `Session` (memory only, never saved). |

Built-in modules (registered by default):

| Module | Key | Scope | Use |
|---|---|---|---|
| `IdentityModule` | `identity` | Never | `userId`, saved app version, `CheckNewVersion()` |
| `SettingsModule` | `settings` | Never | Toggles by name (`Sound`, `Music`, `Haptic`), `noAds` |
| `EconomyModule` | `wallet` | Never | Currencies by id (`Get`, `Add`, `TrySpend`), default `"coins"` |
| `UnlocksModule` | `unlocks` | Never | Unlocked ids and selected id per category |
| `CountersModule` | `counters` | Never | Named int counters |
| `DailyCountersModule` | `counters_daily` | Daily | Counters reset every day |
| `WeeklyCountersModule` | `counters_weekly` | Weekly | Counters reset every week |

## Setup in a game

1. Subclass the manager and register your modules:

```csharp
public class UserDataManager : UserDataManagerBase<UserDataManager>
{
    protected override void RegisterModules(SaveService service)
    {
        service.Register<LevelSaveData>();
        service.Register<SettingsModule, SnakeSettings>();   // replaces a built-in
    }
}
```

2. Put the component on a GameObject in the boot scene (next to `TimeKeeper`). Set the weekly reset day in the inspector.
3. Initialize once at startup, before anything reads data:

```csharp
await UserDataManager.Instance.InitializeAsync();   // or Initialize() without TimeKeeper init
```

`InitializeAsync` loads the save, initializes `TimeKeeper`, then applies daily/weekly resets.

## Using data

```csharp
var levels = UserDataManager.Instance.Get<LevelSaveData>();
levels.Complete(5);

UserDataManager.Instance.Get<EconomyModule>().Add(50);
UserDataManager.Instance.Get<DailyCountersModule>().TryIncrement("ads_for_coins", maxPerDay);
```

- `Get<T>()` throws if `T` was not registered.
- Modules with a reset scope are **replaced by a new instance** on reset. Call `Get<T>()` each time; don't keep the reference.

## Writing a new module

```csharp
[Serializable]
[SaveModule("levels")]                       // unique key; scope defaults to Never
public class LevelSaveData : SaveModule
{
    public List<LevelEntry> levels = new List<LevelEntry>();

    public void Complete(int id)
    {
        GetLevel(id).state = 2;
        RequestSave();                       // persist after changing data
    }
}
```

Rules:
- Register it in `RegisterModules`. The attribute alone does nothing.
- Keys must be unique. Changing a key orphans the old data.
- Only public fields/properties are saved (not `[SerializeField] private`).
- Needs a public parameterless constructor.
- Call `RequestSave()` inside the module, or `UserDataManager.Instance.Save()` from outside, after changes.
  The manager also saves automatically when the app is paused.
- Reset data: `[SaveModule("daily_quests", ResetScope.Daily)]`.

## Extending a built-in module

Derive from it and register the replacement under the base key. Do not put `[SaveModule]` on the derived class.

```csharp
public class SnakeSettings : SettingsModule
{
    public int controlSide;
}

service.Register<SettingsModule, SnakeSettings>();
```

Code that asks for `Get<SettingsModule>()` still works and gets the derived instance.
Register replacements in `RegisterModules`; it runs after the built-ins so the replacement wins.

To use a different set of built-ins, override `RegisterBuiltInModules` without calling base.

## Resets and events

- Daily/weekly resets run on `Initialize*`, on `TimeKeeper.DayChanged`, and when the app resumes.
- The first run for a scope only records the current period; nothing is reset.
- After a reset the manager calls `OnModulesReset(scope)` (override it), then raises
  `GameEvents.RaiseNewDay()` or `GameEvents.RaiseNewWeek()`.
- To add a scope (monthly, season): add a `ResetScope` value and handle it in `IResetClock.GetPeriodKey`.

## Versions and migration

Each module has `Version` (default 1). When the stored version is lower than the current one,
`Migrate(fromVersion, raw)` runs on the old JSON before it becomes your class.

```csharp
public override int Version => 3;

public override void Migrate(int fromVersion, JObject raw)
{
    if (fromVersion < 2) { /* e.g. raw["wallet"] = ...; raw.Remove("coins"); */ }
    if (fromVersion < 3) { raw["hintCount"] = raw["hints"] ?? 0; raw.Remove("hints"); }
}
```

- Adding or removing fields needs no migration (missing fields use defaults, unknown ones are ignored).
- Chain steps with `if (fromVersion < N)` and never edit a released step.
- `Migrate` works only inside one module. Moving data between modules or changing a key is not supported.
- If a migration throws, the module loads with defaults and an error is logged.

## Notes

- Modules that are not registered in a run are kept untouched in the save file.
- `SettingsModule` and `EconomyModule` raise the existing `GameEvents` toggle/currency events.
- The reset clock reads the device date, so changing the phone clock can trigger or skip resets.
- Everything lives in one PlayerPrefs string; fine for normal saves, move to file storage if it grows very large.
