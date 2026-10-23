using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WA.Runtime.Utilities;

namespace Com.Krackhet.Runtime.UserData
{
    /// <summary>
    /// Game-agnostic container of <see cref="SaveModule"/>s: persistence, per-scope resets and lookup.
    /// Modules are replaced by fresh instances on reset, so call <see cref="Get{T}"/> instead of caching them.
    /// </summary>
    public class SaveService
    {
        #region Nested Types
        private class Envelope
        {
            public int schema = 1;
            public Dictionary<string, JToken> modules = new Dictionary<string, JToken>();
            public Dictionary<string, int> versions = new Dictionary<string, int>();
            public Dictionary<string, string> resetMarkers = new Dictionary<string, string>();
        }

        private class Entry
        {
            public string key;
            public ResetScope scope;
            public Type implType;
            public Func<SaveModule> factory;
            public SaveModule module;
        }
        #endregion

        #region Events & Delegates
        /// <summary>Raised after every module of the scope was reset to defaults.</summary>
        public event Action<ResetScope> Reset;
        #endregion

        #region Interfaces & Properties
        public bool IsNewUser { get; private set; }

        public bool IsLoaded => _loaded;
        #endregion

        #region Private Fields
        private readonly string _storageKey;
        private readonly IResetClock _clock;
        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();
        private readonly Dictionary<Type, string> _keyByType = new Dictionary<Type, string>();
        private Envelope _stored = new Envelope();
        private bool _loaded;
        #endregion

        public SaveService(IResetClock clock, string storageKey = "UserSave")
        {
            _clock = clock;
            _storageKey = storageKey;
        }

        #region Registration
        /// <summary>Registers <typeparamref name="T"/> under the key and scope of its <see cref="SaveModuleAttribute"/>.</summary>
        public T Register<T>() where T : SaveModule, new()
        {
            return (T)RegisterInternal(typeof(T), typeof(T), () => new T());
        }

        /// <summary>
        /// Stores <typeparamref name="TDerived"/> under the key of <typeparamref name="TBase"/>, replacing any module
        /// registered for that key. Use it to add fields to a built-in module.
        /// </summary>
        public TDerived Register<TBase, TDerived>() where TBase : SaveModule where TDerived : TBase, new()
        {
            return (TDerived)RegisterInternal(typeof(TBase), typeof(TDerived), () => new TDerived());
        }

        public T Get<T>() where T : SaveModule
        {
            if (!_keyByType.TryGetValue(typeof(T), out string key))
                throw new InvalidOperationException(
                    $"{typeof(T).Name} is not registered. Call Register<{typeof(T).Name}>() in RegisterModules.");

            return (T)_entries[key].module;
        }

        public bool Has<T>() where T : SaveModule
        {
            return _keyByType.ContainsKey(typeof(T));
        }

        private SaveModule RegisterInternal(Type baseType, Type implType, Func<SaveModule> factory)
        {
            SaveModuleAttribute attribute = SaveModuleAttribute.Read(baseType);
            if (attribute == null)
                throw new InvalidOperationException($"{baseType.Name} needs a [SaveModule] attribute.");

            if (_entries.TryGetValue(attribute.Key, out Entry previous))
            {
                foreach (Type type in _keyByType.Where(pair => pair.Value == attribute.Key).Select(pair => pair.Key).ToList())
                    _keyByType.Remove(type);
                _entries.Remove(previous.key);
            }

            Entry entry = new Entry
            {
                key = attribute.Key,
                scope = attribute.Scope,
                implType = implType,
                factory = factory,
                module = Create(factory)
            };

            _entries[entry.key] = entry;
            _keyByType[baseType] = entry.key;
            _keyByType[implType] = entry.key;

            if (_loaded) Hydrate(entry);
            return entry.module;
        }

        private SaveModule Create(Func<SaveModule> factory)
        {
            SaveModule module = factory();
            module.Owner = this;
            return module;
        }
        #endregion

        #region Persistence
        /// <summary>Loads every registered module from storage. Safe to call once after registration.</summary>
        public void Load()
        {
            Envelope envelope = SaverLoader.LoadData<Envelope>(_storageKey);
            IsNewUser = envelope == null;
            _stored = envelope ?? new Envelope();
            _stored.modules ??= new Dictionary<string, JToken>();
            _stored.versions ??= new Dictionary<string, int>();
            _stored.resetMarkers ??= new Dictionary<string, string>();
            _loaded = true;

            foreach (Entry entry in _entries.Values)
                Hydrate(entry);
        }

        /// <summary>Writes all non-session modules. Modules that are not registered this run are preserved untouched.</summary>
        public void Save()
        {
            if (!_loaded) return;

            foreach (Entry entry in _entries.Values)
            {
                if (entry.scope == ResetScope.Session) continue;

                _stored.modules[entry.key] = JToken.FromObject(entry.module);
                _stored.versions[entry.key] = entry.module.Version;
            }

            SaverLoader.SaveData(_storageKey, _stored);
        }

        private void Hydrate(Entry entry)
        {
            if (entry.scope == ResetScope.Session) return;
            if (!_stored.modules.TryGetValue(entry.key, out JToken token)) return;

            try
            {
                if (token is JObject raw
                    && _stored.versions.TryGetValue(entry.key, out int storedVersion)
                    && storedVersion < entry.module.Version)
                {
                    entry.module.Migrate(storedVersion, raw);
                }

                SaveModule loaded = (SaveModule)token.ToObject(entry.implType);
                loaded.Owner = this;
                entry.module = loaded;
                loaded.OnLoaded();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load save module '{entry.key}', using defaults.\n{exception}");
            }
        }
        #endregion

        #region Resets
        public string GetLastResetMarker(ResetScope scope)
        {
            return _stored.resetMarkers.TryGetValue(scope.ToString(), out string marker) ? marker : string.Empty;
        }

        /// <summary>
        /// Resets every scope whose period changed since the last call (also across app launches).
        /// The first call for a scope only records the period.
        /// </summary>
        /// <returns>True if any period marker changed.</returns>
        public bool ApplyResets()
        {
            if (!_loaded) return false;

            bool changed = false;
            List<ResetScope> scopes = _entries.Values
                .Select(entry => entry.scope)
                .Where(scope => scope != ResetScope.Never && scope != ResetScope.Session)
                .Distinct()
                .ToList();

            foreach (ResetScope scope in scopes)
            {
                string current = _clock.GetPeriodKey(scope);
                if (string.IsNullOrEmpty(current)) continue;

                string last = GetLastResetMarker(scope);
                if (current == last) continue;

                _stored.resetMarkers[scope.ToString()] = current;
                changed = true;

                if (string.IsNullOrEmpty(last)) continue;

                ResetModules(scope);
                Reset?.Invoke(scope);
            }

            if (changed) Save();
            return changed;
        }

        private void ResetModules(ResetScope scope)
        {
            foreach (Entry entry in _entries.Values.Where(entry => entry.scope == scope))
                entry.module = Create(entry.factory);
        }
        #endregion
    }
}
