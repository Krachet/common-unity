using System;
using System.Collections.Generic;
using Com.Krackhet.Runtime.Pattern.Singleton;

namespace Com.Krackhet.Runtime.Save
{
    /// <summary>
    /// Owns the save service and all registered save modules.
    /// Loads on Awake, saves on pause, focus loss and quit.
    /// </summary>
    public abstract class SaveManagerBase<TSelf> : Singleton<TSelf> where TSelf : SaveManagerBase<TSelf>
    {
        #region Events & Delegates
        public event Action Loaded;
        #endregion

        #region Interfaces & Properties
        public bool IsLoaded { get; private set; }

        protected ISaveService Service => _service;
        #endregion

        #region Private Fields
        private readonly Dictionary<Type, SaveModule> _modules = new Dictionary<Type, SaveModule>();
        private ISaveService _service;
        #endregion

        #region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            _service = CreateService();
            RegisterModules();
            IsLoaded = true;
            Loaded?.Invoke();
        }

        protected virtual void OnApplicationPause(bool isPaused)
        {
            if (isPaused) SaveAll();
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SaveAll();
        }

        protected override void OnApplicationQuit()
        {
            SaveAll();
            base.OnApplicationQuit();
        }
        #endregion

        #region Public Methods
        public T Register<T>() where T : SaveModule, new()
        {
            if (_modules.TryGetValue(typeof(T), out SaveModule existing)) return (T)existing;

            T module = LoadOrCreate<T>();
            _modules.Add(typeof(T), module);
            return module;
        }

        public T Get<T>() where T : SaveModule, new()
        {
            return Register<T>();
        }

        public void Save<T>() where T : SaveModule, new()
        {
            if (_service == null) return;
            if (!_modules.TryGetValue(typeof(T), out SaveModule module)) return;

            WriteModule(module);
            _service.Flush();
        }

        public void SaveAll()
        {
            if (_service == null) return;

            foreach (SaveModule module in _modules.Values)
            {
                WriteModule(module);
            }

            _service.Flush();
        }

        public T Delete<T>() where T : SaveModule, new()
        {
            T module = CreateFresh<T>();
            _service?.Delete(module.Key);
            _service?.Flush();
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
        #endregion

        #region Protected Methods
        protected virtual ISaveService CreateService()
        {
            return new PlayerPrefsSaveService();
        }

        /// <summary>Register every module the game needs via Register&lt;T&gt;().</summary>
        protected abstract void RegisterModules();
        #endregion

        #region Private Methods
        private T LoadOrCreate<T>() where T : SaveModule, new()
        {
            string key = new T().Key;
            if (_service != null && _service.TryLoad(key, out T loaded))
            {
                loaded.OnLoaded();
                return loaded;
            }

            return CreateFresh<T>();
        }

        private static T CreateFresh<T>() where T : SaveModule, new()
        {
            T module = new T();
            module.OnCreated();
            return module;
        }

        private void WriteModule(SaveModule module)
        {
            module.OnBeforeSave();
            _service.Save(module.Key, module);
        }

        private void ResetModule(Type moduleType)
        {
            SaveModule module = (SaveModule)Activator.CreateInstance(moduleType);
            _service.Delete(module.Key);
            module.OnCreated();
            _modules[moduleType] = module;
        }
        #endregion
    }
}
