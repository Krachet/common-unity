using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Com.Krackhet.Runtime.Pattern.Singleton;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Com.Krackhet.Runtime.Persistence
{
    /// <summary>
    /// Loads, migrates, resets and saves one <typeparamref name="TData"/> as a single json blob.
    /// </summary>
    /// <remarks>
    /// Changing the save structure:
    /// <list type="bullet">
    /// <item>No migration needed: adding a field or section, changing a default value, changing a cycle policy.</item>
    /// <item>Migration needed: renaming or moving a field, changing a field's type, splitting or merging sections,
    /// changing what saved keys mean. Add an <see cref="IUserDataMigration"/> and bump <see cref="CurrentVersion"/>.</item>
    /// <item>Never edit a migration that has shipped. Always add a new one.</item>
    /// </list>
    /// </remarks>
    public abstract class UserDataManagerBase<TManager, TData> : Singleton<TManager>, IUserDataManager, IUserDataOwner
        where TManager : Component
        where TData : UserDataRoot, new()
    {
        private static readonly FieldInfo[] SectionFields = FindSectionFields();

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            TypeNameHandling = TypeNameHandling.None,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Converters = { new StringEnumConverter() }
        };

        private static readonly JsonSerializer Serializer = JsonSerializer.Create(SerializerSettings);

        private IUserDataStorage _storage;
        private bool _dirty;
        private bool _saveBlocked;

        public event Action Loaded;

        public TData Data { get; private set; }
        public bool IsLoaded { get; private set; }

        public string Key => StorageKey;
        public IUserDataStorage Storage => _storage ??= CreateStorage();

        protected abstract string StorageKey { get; }
        protected abstract int CurrentVersion { get; }
        protected virtual IEnumerable<IUserDataMigration> Migrations => Array.Empty<IUserDataMigration>();

        protected virtual IUserDataStorage CreateStorage() => new PlayerPrefsUserDataStorage();

        /// <summary>Synchronous under the hood (PlayerPrefs is main-thread only); returns a Task to fit loading tasks.</summary>
        public Task LoadAsync()
        {
            Load();
            return Task.CompletedTask;
        }

        public void Load()
        {
            _saveBlocked = false;
            string key = StorageKey;
            string raw = Storage.Exists(key) ? Storage.Read(key) : null;

            if (string.IsNullOrEmpty(raw))
            {
                Complete(CreateDefault(), save: true);
                return;
            }

            JObject json;
            try
            {
                using var reader = new JsonTextReader(new StringReader(raw)) { DateParseHandling = DateParseHandling.None };
                json = JObject.Load(reader);
            }
            catch (JsonException e)
            {
                HandleCorrupt(raw, e);
                return;
            }

            int version = json.Value<int?>("version") ?? 1;
            bool migrated = false;

            if (version > CurrentVersion)
            {
                Debug.LogError($"[UserData] Save '{key}' is version {version} but this build only knows {CurrentVersion}. " +
                               "Loading best-effort and blocking saves so the newer data is not overwritten.");
                _saveBlocked = true;
            }
            else if (version < CurrentVersion)
            {
                if (!TryMigrate(json, version, raw))
                {
                    _saveBlocked = true;
                    Complete(CreateDefault(), save: false);
                    return;
                }

                migrated = true;
            }

            TData data;
            try
            {
                data = json.ToObject<TData>(Serializer) ?? new TData();
            }
            catch (Exception e)
            {
                HandleCorrupt(raw, e);
                return;
            }

            bool changed = EnsureSections(data, resetAll: false) | migrated;
            if (!_saveBlocked)
                data.version = CurrentVersion;

            Complete(data, save: changed);
        }

        public void Save()
        {
            if (!IsLoaded || Data == null)
            {
                Debug.LogWarning($"[UserData] Save called on '{StorageKey}' before it was loaded.");
                return;
            }

            if (_saveBlocked)
            {
                Debug.LogWarning($"[UserData] Save blocked for '{StorageKey}' (see the load error).");
                return;
            }

            Data.lastSavedUtc = UserDataClock.UtcNow;
            Storage.Write(StorageKey, JsonConvert.SerializeObject(Data, SerializerSettings));
            _dirty = false;
        }

        public void MarkDirty() => _dirty = true;

        public void SaveIfDirty()
        {
            if (_dirty)
                Save();
        }

        /// <summary>Applies every cycle and expire reset rule, saving if anything was reset.</summary>
        public void RefreshExpirations()
        {
            if (!IsLoaded)
                return;

            if (RefreshSections(Data))
                Save();
        }

        /// <summary>Deletes the save and starts over with defaults. Raises <see cref="Loaded"/> again.</summary>
        public void DeleteAll()
        {
            Storage.Delete(StorageKey);
            _saveBlocked = false;
            Complete(CreateDefault(), save: true);
        }

        public string ToJson() => Data == null ? null : JsonConvert.SerializeObject(Data, Formatting.Indented, SerializerSettings);

        protected virtual void OnApplicationPause(bool paused)
        {
            if (paused)
                SaveIfDirty();
            else
                RefreshExpirations();
        }

        protected virtual void OnApplicationFocus(bool focused)
        {
            if (!focused)
                SaveIfDirty();
        }

        protected override void OnApplicationQuit()
        {
            SaveIfDirty();
            base.OnApplicationQuit();
        }

        private void Complete(TData data, bool save)
        {
            Data = data;
            IsLoaded = true;
            _dirty = false;

            foreach (FieldInfo field in SectionFields)
                (field.GetValue(data) as UserData)?.Bind(this);

            if (RefreshSections(data))
                save = true;

            if (save)
                Save();

            Loaded?.Invoke();
        }

        private TData CreateDefault()
        {
            var data = new TData { version = CurrentVersion };
            EnsureSections(data, resetAll: true);
            return data;
        }

        private void HandleCorrupt(string raw, Exception e)
        {
            Debug.LogError($"[UserData] Save '{StorageKey}' could not be read, starting with defaults. " +
                           $"The raw data was copied to '{StorageKey}.corrupt'.\n{e}");
            Storage.Write(StorageKey + ".corrupt", raw);
            Complete(CreateDefault(), save: true);
        }

        private bool TryMigrate(JObject json, int fromVersion, string raw)
        {
            string backupKey = $"{StorageKey}.backup_v{fromVersion}";
            if (!Storage.Exists(backupKey))
                Storage.Write(backupKey, raw);

            var steps = new Dictionary<int, IUserDataMigration>();
            foreach (IUserDataMigration migration in Migrations)
            {
                if (!steps.TryAdd(migration.FromVersion, migration))
                {
                    Debug.LogError($"[UserData] Two migrations start from version {migration.FromVersion}. Save left untouched.");
                    return false;
                }
            }

            for (int v = fromVersion; v < CurrentVersion; v++)
            {
                if (!steps.TryGetValue(v, out IUserDataMigration step))
                {
                    Debug.LogError($"[UserData] No migration from version {v} to {v + 1}. Save left untouched.");
                    return false;
                }

                try
                {
                    step.Migrate(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UserData] Migration {v} -> {v + 1} failed. Save left untouched.\n{e}");
                    return false;
                }
            }

            json["version"] = CurrentVersion;
            return true;
        }

        private static bool EnsureSections(TData data, bool resetAll)
        {
            bool changed = false;

            foreach (FieldInfo field in SectionFields)
            {
                var section = field.GetValue(data) as UserData;

                if (section == null)
                {
                    if (field.FieldType.IsAbstract)
                    {
                        Debug.LogError($"[UserData] Section '{field.Name}' has abstract type {field.FieldType.Name}; it cannot be created.");
                        continue;
                    }

                    section = (UserData)Activator.CreateInstance(field.FieldType, nonPublic: true);
                    field.SetValue(data, section);
                    section.ResetToDefaults();
                    changed = true;
                }
                else if (resetAll)
                {
                    section.ResetToDefaults();
                }
            }

            return changed;
        }

        private static bool RefreshSections(TData data)
        {
            DateTime utcNow = UserDataClock.UtcNow;
            bool changed = false;

            foreach (FieldInfo field in SectionFields)
            {
                if (field.GetValue(data) is UserData section && section.RefreshExpiration(utcNow))
                    changed = true;
            }

            return changed;
        }

        private static FieldInfo[] FindSectionFields()
        {
            return typeof(TData)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => typeof(UserData).IsAssignableFrom(f.FieldType))
                .Where(f => f.IsPublic || f.IsDefined(typeof(JsonPropertyAttribute)))
                .ToArray();
        }
    }
}
