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
        /// <summary>Rewards were granted through <see cref="AchievementService.TryClaim"/>. Only ever true while unlocked.</summary>
        public bool Claimed { get; internal set; }
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
        private readonly HashSet<string> _claiming = new HashSet<string>();
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
                        state.Claimed = entry.Unlocked && entry.Claimed;
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
#if UNITY_EDITOR
                UnityEngine.Debug.Log(
                    $"[Achievements] {typeof(TToken).Name} -> task '{task.Id}' ({task.name}): {old} -> {next}", task);
#endif
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

        /// <summary>
        /// True when <paramref name="id"/> is a valid unlocked achievement with valid rewards that has not been claimed.
        /// Does not check whether a reward context can currently grant them; <see cref="TryClaim"/> does.
        /// </summary>
        public bool CanClaim(string id) => ValidateClaim(id, out _, out _, out _);

        /// <summary>
        /// Claims an unlocked, unclaimed achievement: grants all of its rewards and sets a one-time claimed state, after
        /// which repeat claims are rejected (also after reload once the state is saved). All rewards are preflighted
        /// against <paramref name="context"/> before any is granted.
        /// <para>Returns false when the claim was not applied: the achievement stays claimable and
        /// <paramref name="error"/> explains why. A grant failing after a passed preflight is not rolled back.</para>
        /// <para>Returns true when the claim was applied. <paramref name="error"/> is null only if the granted rewards
        /// and then the claimed state were both saved; otherwise it describes the persistence failure (also raised via
        /// <see cref="Diagnostic"/>) while the claim stays applied in memory. The two saves are separate writes, so this
        /// is not crash-atomic: a crash between them keeps the rewards but loses the claimed state, making the
        /// achievement claimable again after restart.</para>
        /// </summary>
        public bool TryClaim(string id, IAchievementRewardContext context, out string error)
        {
            if (!ValidateClaim(id, out var definition, out var state, out error)) return FailClaim(id, error);
            if (context == null) return FailClaim(id, error = "No reward context.");
            if (!_claiming.Add(id)) return FailClaim(id, error = "A claim is already in progress.");
            try
            {
                foreach (var reward in definition.Rewards)
                {
                    bool can;
                    try { can = reward.CanGrantTo(context, out error); }
                    catch (Exception exception) { can = false; error = $"Reward '{reward.name}' threw during preflight: {exception.Message}"; }
                    if (!can) return FailClaim(id, error ?? $"Reward '{reward.name}' cannot be granted.");
                }
                foreach (var reward in definition.Rewards)
                {
                    bool granted;
                    try { granted = reward.GrantTo(context, out error); }
                    catch (Exception exception) { granted = false; error = $"Reward '{reward.name}' threw while granting: {exception.Message}"; }
                    // Unexpected after a passed preflight; earlier rewards may already be granted and are not rolled back.
                    if (!granted) return FailClaim(id, error ?? $"Reward '{reward.name}' failed to grant.");
                }
                // Rewards are now applied in memory, so the claim is applied too: repeat claims must be rejected even if
                // persistence below fails. Persistence failures are reported through error/Diagnostic, not rolled back.
                state.Claimed = true;
                string saveError;
                bool rewardsSaved;
                try { rewardsSaved = context.SaveGrantedRewards(out saveError); }
                catch (Exception exception) { rewardsSaved = false; saveError = exception.Message; }
                error = null;
                if (!rewardsSaved)
                    error = $"Rewards granted but saving them failed: {saveError ?? "unknown error"}.";
                if (!SaveCurrentPayload())
                    error = (error == null ? string.Empty : error + " ") + $"Claimed state could not be saved: {LastError}.";
                if (error != null)
                    RecordDiagnostic(new InvalidOperationException($"Achievement '{id}' claim applied with persistence failure: {error}"));
                return true;
            }
            finally { _claiming.Remove(id); }
        }

        private bool ValidateClaim(string id, out AchievementDefinition definition, out AchievementState state, out string error)
        {
            state = null;
            if (string.IsNullOrEmpty(id) || !_definitions.TryGetValue(id, out definition))
            { definition = null; error = $"Unknown or invalid achievement '{id}'."; return false; }
            state = _states[id];
            if (!state.Unlocked) { error = "Achievement is not unlocked."; return false; }
            if (state.Claimed) { error = "Achievement was already claimed."; return false; }
            return ValidateRewards(definition, out error);
        }

        private bool ValidateRewards(AchievementDefinition definition, out string error)
        {
            var rewards = definition.Rewards;
            if (rewards == null || rewards.Count == 0) { error = $"Definition '{definition.Id}' has no rewards."; return false; }
            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                if (reward == null) { error = $"Definition '{definition.Id}' has a null reward."; return false; }
                for (var j = 0; j < i; j++)
                {
                    if (ReferenceEquals(rewards[j], reward)) { error = $"Definition '{definition.Id}' lists reward '{reward.name}' more than once."; return false; }
                    if (rewards[j] != null && rewards[j].GetType() == reward.GetType() && !reward.AllowMultiplePerAchievement)
                    {
                        error = $"Definition '{definition.Id}' has more than one {reward.GetType().Name} ('{rewards[j].name}', '{reward.name}'); combine their entries into one reward.";
                        return false;
                    }
                }
                bool valid;
                try { valid = reward.ValidateConfiguration(out error); }
                catch (Exception exception) { valid = false; error = exception.Message; }
                if (!valid) { error = $"Reward '{reward.name}' ({reward.GetType().Name}) in '{definition.Id}' is misconfigured: {error ?? "validation failed"}."; return false; }
            }
            error = null;
            return true;
        }

        private bool FailClaim(string id, string error)
        {
            RecordDiagnostic(new InvalidOperationException($"Achievement '{id}' claim failed: {error}"));
            return false;
        }

        private bool SaveCurrentPayload()
        {
            try
            {
                var achievements = new List<AchievementUnlockSaveEntry>(_unknownAchievements.Values);
                foreach (var pair in _states) achievements.Add(new AchievementUnlockSaveEntry
                    { Id = pair.Key, Unlocked = pair.Value.Unlocked, UnlockedAtUtc = pair.Value.UnlockedAtUtc?.ToString("O"), Claimed = pair.Value.Claimed });
                var tasks = new List<AchievementTaskProgress>(_progress.Values);
                _store.Save(new AchievementSaveData { Tasks = tasks.ToArray(), Achievements = achievements.ToArray() });
                LastError = null;
                return true;
            }
            catch (Exception exception) { RecordDiagnostic(exception); return false; }
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
                // Invalid rewards only block claiming; progress and unlocking still work.
                if (definition.Rewards != null && definition.Rewards.Count > 0 && !ValidateRewards(definition, out var rewardError))
                    Error(rewardError);
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
