using System;
using System.Collections.Generic;
using System.Threading;
using Com.Krackhet.Runtime.Pattern.Singleton;
using Com.Krackhet.Runtime.TimeKeeping;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com.Krackhet.Runtime.Save
{
    /// <summary>
    /// Owns the save service and all registered save modules.
    /// Loads via InitializeAsync, saves on pause, focus loss and quit,
    /// and refreshes cycled (daily/weekly) modules on resume.
    /// </summary>
    public abstract class SaveManagerBase<TSelf> : Singleton<TSelf>, ISaveManager where TSelf : SaveManagerBase<TSelf>
    {
        #region Constants
        private const string LogTag = "[SaveManager]";
        #endregion

        #region Events & Delegates
        public event Action Loaded;
        public event Action<CycledSaveModule> ModuleCycleReset;
        #endregion

        #region Interfaces & Properties
        public bool IsLoaded { get; private set; }

        public Weekday DefaultWeeklyResetDay => _defaultWeeklyResetDay;

        protected ISaveService Service => _service;
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Reset day for weekly modules that don't override ResetDay")]
#endif
        [SerializeField]
        private Weekday _defaultWeeklyResetDay = Weekday.Monday;
        #endregion

        #region Private Fields
        private readonly Dictionary<Type, SaveModule> _modules = new Dictionary<Type, SaveModule>();
        private readonly List<SaveModule> _resetModules = new List<SaveModule>();
        private ISaveService _service;
        #endregion

        #region Unity Callbacks
        protected virtual void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                SaveAll();
                return;
            }

            RefreshCycles();
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveAll();
                return;
            }

            RefreshCycles();
        }

        protected override void OnApplicationQuit()
        {
            SaveAll();
            base.OnApplicationQuit();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates the save service and loads every module from RegisterModules.
        /// Safe to call more than once; later calls return immediately.
        /// </summary>
        public async UniTask InitializeAsync(
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default
        )
        {
            if (IsLoaded)
            {
                progress?.Report(1f);
                return;
            }

            _service = CreateService();
            progress?.Report(0.5f);
            await UniTask.Yield(cancellationToken);

            RegisterModules();
            IsLoaded = true;
            progress?.Report(1f);
            Loaded?.Invoke();
        }

        public T Register<T>() where T : SaveModule, new()
        {
            if (_modules.TryGetValue(typeof(T), out SaveModule existing)) return (T)existing;

            if (_service == null)
            {
                Debug.LogError($"{LogTag} {typeof(T).Name} requested before InitializeAsync.", this);
                return null;
            }

            T module = LoadOrCreate<T>(
                out bool wasMigrated,
                out bool hasUnsupportedDataVersion
            );
            _modules.Add(typeof(T), module);

            bool shouldSave = wasMigrated;
            if (!hasUnsupportedDataVersion && RefreshModuleCycle(module, GetCurrentTime()))
            {
                shouldSave = true;
            }

            if (shouldSave)
            {
                SaveModuleNow(module);
            }

            return module;
        }

        public T Get<T>() where T : SaveModule, new()
        {
            return Register<T>();
        }

        public void Save<T>() where T : SaveModule, new()
        {
            if (!_modules.TryGetValue(typeof(T), out SaveModule module)) return;

            PrepareForSave();
            SaveModuleNow(module);
        }

        public void SaveAll()
        {
            if (_service == null) return;

            PrepareForSave();
            foreach (SaveModule module in _modules.Values)
            {
                WriteModule(module);
            }

            _service.Flush();
        }

        public T Delete<T>() where T : SaveModule, new()
        {
            if (_service == null)
            {
                Debug.LogError($"{LogTag} Cannot delete {typeof(T).Name} before InitializeAsync.", this);
                return null;
            }

            T module = new T();
            _service.Delete(module.Key);
            _service.Flush();
            PrepareFresh(module);
            _modules[typeof(T)] = module;
            return module;
        }

        public void DeleteAll()
        {
            if (_service == null) return;

            List<Type> moduleTypes = new List<Type>(_modules.Keys);
            foreach (Type moduleType in moduleTypes)
            {
                ResetModule(moduleType);
            }

            _service.Flush();
        }

        /// <summary>
        /// Resets every daily/weekly module whose cycle rolled over and saves them.
        /// Called automatically on resume; call it manually for mid-session rollovers.
        /// </summary>
        public void RefreshCycles()
        {
            if (_service == null) return;

            DateTime now = GetCurrentTime();
            _resetModules.Clear();
            foreach (SaveModule module in _modules.Values)
            {
                if (RefreshModuleCycle(module, now)) _resetModules.Add(module);
            }

            if (_resetModules.Count == 0) return;

            foreach (SaveModule module in _resetModules)
            {
                WriteModule(module);
            }

            _resetModules.Clear();
            _service.Flush();
        }
        #endregion

        #region Protected Methods
        protected virtual ISaveService CreateService()
        {
            return new PlayerPrefsSaveService();
        }

        /// <summary>Time source for reset cycles. Uses TimeKeeper when present.</summary>
        protected virtual DateTime GetCurrentTime()
        {
            TimeKeeper timeKeeper = TimeKeeper.Instance;
            return timeKeeper != null ? timeKeeper.Now : DateTime.Now;
        }

        /// <summary>
        /// Register every module the game needs via Register&lt;T&gt;().
        /// Called once from InitializeAsync after the service is created.
        /// </summary>
        protected abstract void RegisterModules();

        /// <summary>Synchronizes external runtime state before modules are serialized.</summary>
        protected virtual void PrepareForSave()
        {
        }
        #endregion

        #region Private Methods
        private T LoadOrCreate<T>(
            out bool wasMigrated,
            out bool hasUnsupportedDataVersion
        ) where T : SaveModule, new()
        {
            wasMigrated = false;
            hasUnsupportedDataVersion = false;

            T module = new T();
            if (_service != null && _service.TryLoad(module.Key, out T loaded))
            {
                hasUnsupportedDataVersion = loaded.HasUnsupportedDataVersion;
                if (hasUnsupportedDataVersion)
                {
                    Debug.LogWarning(
                        $"{LogTag} Save module '{loaded.Key}' uses unsupported data version " +
                        $"{loaded.DataVersion}. Current version is not able to load it safely.",
                        this
                    );
                }
                else
                {
                    wasMigrated = loaded.UpgradeDataVersion();
                }

                loaded.OnLoaded();
                return loaded;
            }

            PrepareFresh(module);
            return module;
        }

        private void PrepareFresh(SaveModule module)
        {
            module.OnCreated();
            module.InitializeDataVersion();
            RefreshModuleCycle(module, GetCurrentTime());
        }

        private bool RefreshModuleCycle(SaveModule module, DateTime now)
        {
            if (!(module is CycledSaveModule cycledModule)) return false;

            if (cycledModule is WeeklySaveModule weeklyModule)
            {
                weeklyModule.DefaultResetDay = _defaultWeeklyResetDay;
            }

            if (!cycledModule.RefreshCycle(now)) return false;

            ModuleCycleReset?.Invoke(cycledModule);
            return true;
        }

        private void SaveModuleNow(SaveModule module)
        {
            if (_service == null) return;

            WriteModule(module);
            _service.Flush();
        }

        private void WriteModule(SaveModule module)
        {
            if (module.HasUnsupportedDataVersion)
            {
                return;
            }

            module.OnBeforeSave();
            _service.Save(module.Key, module);
        }

        private void ResetModule(Type moduleType)
        {
            SaveModule module = (SaveModule)Activator.CreateInstance(moduleType);
            _service.Delete(module.Key);
            PrepareFresh(module);
            _modules[moduleType] = module;
        }
        #endregion
    }
}
