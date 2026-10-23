using Com.Krackhet.Runtime.Pattern.Singleton;
using UnityEngine;
using TimeKeeperComponent = Com.Krackhet.Runtime.TimeKeeper.TimeKeeper;
using WeekdayType = Com.Krackhet.Runtime.TimeKeeper.Weekday;
#if CYSHARP_UNITY
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace Com.Krackhet.Runtime.UserData
{
    /// <summary>
    /// Scene entry point of the save system. Subclass it per game and override <see cref="RegisterModules"/>
    /// to add or replace modules, then expose game-specific helpers on the subclass.
    /// </summary>
    public abstract class UserDataManagerBase<T> : Singleton<T> where T : UserDataManagerBase<T>
    {
        #region Serialize Fields
#if UNITY_EDITOR
        [Tooltip("Day of the week that triggers a weekly reset")]
#endif
        [SerializeField]
        private WeekdayType _weeklyResetDay = WeekdayType.Monday;
        #endregion

        #region Interfaces & Properties
        public SaveService Service { get; private set; }

        public bool isNewUser => Service != null && Service.IsNewUser;

        protected virtual string StorageKey => "UserSave";
        #endregion

        #region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            Service = new SaveService(new TimeKeeperResetClock(_weeklyResetDay), StorageKey);
            RegisterBuiltInModules(Service);
            RegisterModules(Service);
            Service.Reset += OnScopeReset;

            TimeKeeperComponent timeKeeper = TimeKeeperComponent.Instance;
            if (timeKeeper != null)
                timeKeeper.DayChanged += OnDayChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Service != null)
                Service.Reset -= OnScopeReset;

            TimeKeeperComponent timeKeeper = TimeKeeperComponent.Instance;
            if (timeKeeper != null)
                timeKeeper.DayChanged -= OnDayChanged;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
            else Service?.ApplyResets();
        }
        #endregion

        #region Public Methods
        public void Initialize()
        {
            Service.Load();
            Service.ApplyResets();
        }

#if CYSHARP_UNITY
        public async UniTask InitializeAsync()
#else
        public async Task InitializeAsync()
#endif
        {
            Service.Load();

            TimeKeeperComponent timeKeeper = TimeKeeperComponent.Instance;
            if (timeKeeper != null)
                await timeKeeper.InitializeAsync(Service.GetLastResetMarker(ResetScope.Daily));

            Service.ApplyResets();
        }

        public TModule Get<TModule>() where TModule : SaveModule
        {
            return Service.Get<TModule>();
        }

        public void Save()
        {
            Service?.Save();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Registers the modules most games need. Override without calling base to pick a different set.
        /// </summary>
        protected virtual void RegisterBuiltInModules(SaveService service)
        {
            service.Register<IdentityModule>();
            service.Register<SettingsModule>();
            service.Register<EconomyModule>();
            service.Register<UnlocksModule>();
            service.Register<CountersModule>();
            service.Register<DailyCountersModule>();
            service.Register<WeeklyCountersModule>();
        }

        /// <summary>
        /// Register the game's own modules here, e.g. <c>service.Register&lt;LevelSaveData&gt;()</c>, or a
        /// replacement for a built-in one: <c>service.Register&lt;SettingsModule, MySettings&gt;()</c>.
        /// Runs after the built-ins, so a replacement wins.
        /// </summary>
        protected abstract void RegisterModules(SaveService service);

        /// <summary>Called after a scope was reset, before the global new day/week events.</summary>
        protected virtual void OnModulesReset(ResetScope scope)
        {
        }
        #endregion

        #region Private Methods
        private void OnDayChanged()
        {
            Service.ApplyResets();
        }

        private void OnScopeReset(ResetScope scope)
        {
            OnModulesReset(scope);

            switch (scope)
            {
                case ResetScope.Daily:
                    GameEvents.RaiseNewDay();
                    break;
                case ResetScope.Weekly:
                    GameEvents.RaiseNewWeek();
                    break;
            }
        }
        #endregion
    }
}
