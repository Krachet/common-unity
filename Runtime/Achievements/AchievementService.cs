using System;
using System.Collections.Generic;

namespace Com.Krackhet.Runtime.Achievements
{
    public interface IAchievementReporter
    {
        /// <summary>
        /// Reports a gameplay event. Every registered task whose token type is exactly <typeparamref name="TToken"/>
        /// receives the token and decides whether/how it changes its progress. Triggers raised while a dispatch is
        /// running (e.g. from notification handlers) are queued and processed in order afterwards.
        /// </summary>
        void Trigger<TToken>(TToken token) where TToken : struct, IAchievementToken;

        /// <summary>Reports a payload-free event; equivalent to <c>Trigger(default(TToken))</c>.</summary>
        void Trigger<TToken>() where TToken : struct, IAchievementToken;
    }

    public sealed class AchievementState
    {
        public string Id { get; internal set; }
        public bool Unlocked { get; internal set; }
        public DateTime? UnlockedAtUtc { get; internal set; }
    }

    public sealed class AchievementProgressNotification
    {
        public string TaskId { get; internal set; }
        public long Value { get; internal set; }
    }

    public sealed class AchievementUnlockedNotification
    {
        public AchievementDefinition Definition { get; internal set; }
        public AchievementState State { get; internal set; }
    }

    public sealed class AchievementService : IAchievementReporter
    {
        private readonly IAchievementStore _store;
        private readonly Dictionary<string, AchievementTask> _tasks = new Dictionary<string, AchievementTask>();
        private readonly Dictionary<string, AchievementTaskProgress> _progress = new Dictionary<string, AchievementTaskProgress>();
        private readonly Dictionary<string, AchievementDefinition> _definitions = new Dictionary<string, AchievementDefinition>();
        private readonly Dictionary<string, AchievementState> _states = new Dictionary<string, AchievementState>();
        private readonly Dictionary<string, List<AchievementDefinition>> _affected = new Dictionary<string, List<AchievementDefinition>>();
        private readonly Dictionary<string, AchievementUnlockSaveEntry> _unknownAchievements = new Dictionary<string, AchievementUnlockSaveEntry>();
        private readonly List<string> _configurationErrors = new List<string>();
        // Exact token type -> registered tasks (distinct objects, catalog order). Immutable after Build.
        private readonly Dictionary<Type, List<AchievementTask>> _routes = new Dictionary<Type, List<AchievementTask>>();
        private readonly Queue<Action> _pending = new Queue<Action>();
        private readonly List<AchievementTask> _changedTasks = new List<AchievementTask>();
        private readonly List<AchievementDefinition> _affectedDefinitions = new List<AchievementDefinition>();
        private readonly HashSet<AchievementDefinition> _affectedSet = new HashSet<AchievementDefinition>();
        private bool _dispatching;

        /// <summary>Upper bound of queued (reentrant) dispatches drained per top-level trigger, guarding against feedback loops.</summary>
        public const int MaxQueuedDispatches = 1024;

        public event Action<AchievementProgressNotification> ProgressChanged;
        public event Action<AchievementUnlockedNotification> Unlocked;
        public event Action<Exception> Diagnostic;
        public IReadOnlyDictionary<string, AchievementState> States => _states;
        public IReadOnlyList<string> ConfigurationErrors => _configurationErrors;
        public string LastError { get; private set; }

        public AchievementService(AchievementCatalog catalog, IAchievementStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store)); Build(catalog);
        }

        public void Initialize()
        {
            try
            {
                var data = _store.Load() ?? new AchievementSaveData();
                if (data.Tasks != null) foreach (var entry in data.Tasks)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.Id)) continue;
                    if (_tasks.ContainsKey(entry.Id)) entry.Value = Math.Max(0, entry.Value);
                    _progress[entry.Id] = entry;
                }
                if (data.Achievements != null) foreach (var entry in data.Achievements)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.Id)) continue;
                    if (_states.TryGetValue(entry.Id, out var state))
                    {
                        state.Unlocked = entry.Unlocked;
                        if (DateTime.TryParse(entry.UnlockedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out var time))
                            state.UnlockedAtUtc = time.ToUniversalTime();
                    }
                    else _unknownAchievements[entry.Id] = entry;
                }
                foreach (var definition in _definitions.Values) Evaluate(definition);
            }
            catch (Exception exception) { RecordDiagnostic(exception); }
        }

        public long GetValue(AchievementTask task) =>
            task != null && ValidIdentity(task) && _progress.TryGetValue(task.Id, out var progress) ? progress.Value : 0;
        public bool TryGetState(string id, out AchievementState state) => _states.TryGetValue(id ?? string.Empty, out state);
        public void Trigger<TToken>() where TToken : struct, IAchievementToken => Trigger(default(TToken));

        public void Trigger<TToken>(TToken token) where TToken : struct, IAchievementToken
        {
            if (_dispatching) { _pending.Enqueue(() => Dispatch(in token)); return; }
            _dispatching = true;
            try
            {
                SafeDispatch(() => Dispatch(in token));
                var drained = 0;
                while (_pending.Count > 0)
                {
                    if (++drained > MaxQueuedDispatches)
                    {
                        RecordDiagnostic(new InvalidOperationException(
                            $"Achievement trigger queue exceeded {MaxQueuedDispatches} nested dispatches; dropped {_pending.Count} pending trigger(s)."));
                        _pending.Clear();
                        break;
                    }
                    SafeDispatch(_pending.Dequeue());
                }
            }
            finally { _dispatching = false; }
        }

        private void SafeDispatch(Action dispatch)
        {
            try { dispatch(); } catch (Exception exception) { RecordDiagnostic(exception); }
        }

        private void Dispatch<TToken>(in TToken token) where TToken : struct, IAchievementToken
        {
            if (!_routes.TryGetValue(typeof(TToken), out var tasks)) return;
            _changedTasks.Clear(); _affectedDefinitions.Clear(); _affectedSet.Clear();

            // 1) Let every matching task compute its next progress against the current value.
            foreach (var task in tasks)
            {
                var progress = _progress[task.Id];
                var old = progress.Value;
                bool accepted; long next;
                try { accepted = ((AchievementTask<TToken>)task).Evaluate(in token, old, out next); }
                catch (Exception exception)
                {
                    RecordDiagnostic(new InvalidOperationException(
                        $"Achievement task '{task.Id}' ({task.GetType().Name}) threw while handling {typeof(TToken).Name}: {exception.Message}", exception));
                    continue;
                }
                if (!accepted || next == old) continue;
                if (next < 0)
                {
                    RecordDiagnostic(new InvalidOperationException(
                        $"Achievement task '{task.Id}' ({task.GetType().Name}) produced negative progress {next} for {typeof(TToken).Name}; kept {old}."));
                    continue;
                }
                progress.Value = next;
                _changedTasks.Add(task);
            }
            if (_changedTasks.Count == 0) return;

            // 2) Notify and re-evaluate each affected definition once, then persist once.
            var changed = _changedTasks.ToArray();
            foreach (var task in changed)
            {
                Notify(ProgressChanged, new AchievementProgressNotification { TaskId = task.Id, Value = _progress[task.Id].Value });
                if (_affected.TryGetValue(task.Id, out var definitions))
                    foreach (var definition in definitions)
                        if (_affectedSet.Add(definition)) _affectedDefinitions.Add(definition);
            }
            var affected = _affectedDefinitions.ToArray();
            foreach (var definition in affected) Evaluate(definition);
            SaveCurrentPayload();
        }

        public void Flush() => SaveCurrentPayload();

        private void SaveCurrentPayload()
        {
            try
            {
                var achievements = new List<AchievementUnlockSaveEntry>(_unknownAchievements.Values);
                foreach (var pair in _states) achievements.Add(new AchievementUnlockSaveEntry
                    { Id = pair.Key, Unlocked = pair.Value.Unlocked, UnlockedAtUtc = pair.Value.UnlockedAtUtc?.ToString("O") });
                var tasks = new List<AchievementTaskProgress>(_progress.Values);
                _store.Save(new AchievementSaveData { Tasks = tasks.ToArray(), Achievements = achievements.ToArray() });
                LastError = null;
            }
            catch (Exception exception) { RecordDiagnostic(exception); }
        }

        private void Build(AchievementCatalog catalog)
        {
            if (catalog == null) return;
            var definitionIds = new HashSet<string>();
            var taskObjects = new Dictionary<string, AchievementTask>();
            var taskValidity = new Dictionary<AchievementTask, bool>();
            foreach (var definition in catalog.Definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                { Error("Blank/null achievement definition ID."); continue; }
                if (!definitionIds.Add(definition.Id))
                { Error($"Duplicate definition ID '{definition.Id}'."); continue; }
                if (definition.Requirements == null || definition.Requirements.Count == 0)
                { Error($"Definition '{definition.Id}' has no requirements."); continue; }
                var local = new HashSet<string>(); var valid = true;
                var pendingTasks = new Dictionary<string, AchievementTask>();
                foreach (var requirement in definition.Requirements)
                {
                    var task = requirement?.Task;
                    if (task == null) { Error($"Definition '{definition.Id}' has a null task."); valid = false; continue; }
                    if (string.IsNullOrWhiteSpace(task.Id)) { Error("Blank task ID."); valid = false; continue; }
                    if (taskObjects.TryGetValue(task.Id, out var existing) && !ReferenceEquals(existing, task))
                    {
                        Error(existing.GetType() != task.GetType()
                            ? $"Task ID '{task.Id}' is used by inconsistent task types."
                            : $"Task ID '{task.Id}' is shared by different task assets ('{existing.name}', '{task.name}') in '{definition.Id}'.");
                        valid = false; continue;
                    }
                    if (!ValidateTask(task, taskValidity)) { valid = false; continue; }
                    if (requirement.Target <= 0) { Error($"Invalid target in '{definition.Id}'."); valid = false; }
                    if (!local.Add(task.Id)) { Error($"Duplicate task '{task.Id}' in '{definition.Id}'."); valid = false; }
                    else pendingTasks[task.Id] = task;
                }
                if (!valid) continue;
                foreach (var pair in pendingTasks)
                    if (!taskObjects.ContainsKey(pair.Key)) taskObjects.Add(pair.Key, pair.Value);
                _definitions[definition.Id] = definition;
                _states[definition.Id] = new AchievementState { Id = definition.Id };
                foreach (var requirement in definition.Requirements)
                {
                    var task = requirement.Task; taskObjects[task.Id] = task;
                    if (!_tasks.ContainsKey(task.Id))
                    {
                        _tasks[task.Id] = task;
                        if (!_routes.TryGetValue(task.TokenType, out var route)) _routes[task.TokenType] = route = new List<AchievementTask>();
                        route.Add(task);
                    }
                    if (!_progress.ContainsKey(task.Id))
                        _progress[task.Id] = new AchievementTaskProgress { Id = task.Id };
                    if (!_affected.TryGetValue(task.Id, out var list)) _affected[task.Id] = list = new List<AchievementDefinition>();
                    list.Add(definition);
                }
            }
        }

        // Runs the task's own configuration hook once per task object; a shared task reports its error only once.
        private bool ValidateTask(AchievementTask task, Dictionary<AchievementTask, bool> cache)
        {
            if (cache.TryGetValue(task, out var known)) return known;
            bool valid;
            string error;
            var tokenType = task.TokenType;
            if (tokenType == null || !tokenType.IsValueType || !typeof(IAchievementToken).IsAssignableFrom(tokenType))
            {
                Error($"Task '{task.Id}' ({task.GetType().Name}) has no valid token type '{tokenType?.Name ?? "null"}'.");
                cache[task] = false;
                return false;
            }
            try { valid = task.ValidateConfiguration(out error); }
            catch (Exception exception) { valid = false; error = exception.Message; RecordDiagnostic(exception); }
            if (!valid) Error($"Task '{task.Id}' ({task.GetType().Name}) is misconfigured: {error ?? "validation failed"}.");
            cache[task] = valid;
            return valid;
        }

        // Only the exact registered task object may read progress; same-ID clones are treated as unknown.
        private bool ValidIdentity(AchievementTask task) =>
            !string.IsNullOrWhiteSpace(task.Id) && _tasks.TryGetValue(task.Id, out var known) && ReferenceEquals(known, task);
        private void Evaluate(AchievementDefinition definition)
        {
            var state = _states[definition.Id]; if (state.Unlocked) return;
            var complete = definition.Policy == AchievementRequirementPolicy.All;
            foreach (var requirement in definition.Requirements)
            {
                var met = _progress[requirement.Task.Id].Value >= requirement.Target;
                if (definition.Policy == AchievementRequirementPolicy.Any && met) { complete = true; break; }
                if (definition.Policy == AchievementRequirementPolicy.All && !met) { complete = false; break; }
            }
            if (!complete) return;
            state.Unlocked = true;
            state.UnlockedAtUtc = DateTime.UtcNow;
            Notify(Unlocked, new AchievementUnlockedNotification { Definition = definition, State = state });
        }
        private void Error(string message) { _configurationErrors.Add(message); }
        private void Notify<T>(Action<T> handlers, T value)
        {
            if (handlers == null) return;
            foreach (Action<T> handler in handlers.GetInvocationList())
                try { handler(value); } catch (Exception exception) { RecordDiagnostic(exception); }
        }
        private void RecordDiagnostic(Exception exception)
        {
            LastError = exception.Message;
            if (Diagnostic == null) return;
            foreach (Action<Exception> handler in Diagnostic.GetInvocationList()) try { handler(exception); } catch { }
        }
    }
}
