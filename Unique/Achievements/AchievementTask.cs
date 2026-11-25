using System;
using UnityEngine;

namespace Com.Krackhet.Unique.Achievements
{
    /// <summary>
    /// Marker for achievement event tokens. Tokens are value types describing something that happened in gameplay;
    /// payload-free events are simply empty <c>readonly struct</c>s reported via <see cref="IAchievementReporter.Trigger{TToken}()"/>.
    /// </summary>
    public interface IAchievementToken { }

    /// <summary>
    /// Identity/configuration base for achievement tasks. Tasks are stateless assets: runtime progress lives in
    /// <see cref="AchievementService"/> keyed by <see cref="Id"/>. Derive from <see cref="AchievementTask{TToken}"/>
    /// to declare which token type the task listens to.
    /// </summary>
    public abstract class AchievementTask : ScriptableObject
    {
        [SerializeField] private string _id;
        public string Id => _id;

        /// <summary>Exact token type this task is routed by. Only implemented by <see cref="AchievementTask{TToken}"/>.</summary>
        internal abstract Type TokenType { get; }

        /// <summary>Optional configuration check run while the service builds its catalog. Return false with an error to reject the task.</summary>
        protected virtual bool Validate(out string error)
        {
            error = null;
            return true;
        }

        internal bool ValidateConfiguration(out string error) => Validate(out error);

        public static T Create<T>(string id) where T : AchievementTask
        {
            var task = CreateInstance<T>();
            task._id = id;
            return task;
        }

#if UNITY_EDITOR
        protected virtual void Reset() => EnsureEditorId();
        protected virtual void OnValidate() => EnsureEditorId();
        private void EnsureEditorId()
        {
            if (string.IsNullOrWhiteSpace(_id))
                _id = UnityEditor.GUID.Generate().ToString();
        }
#endif
    }

    /// <summary>
    /// Task that reacts to tokens of exactly <typeparamref name="TToken"/>. Every registered task of that token type
    /// receives each reported token and decides itself whether it applies.
    /// </summary>
    public abstract class AchievementTask<TToken> : AchievementTask where TToken : struct, IAchievementToken
    {
        internal sealed override Type TokenType => typeof(TToken);

        /// <summary>
        /// Pure progress hook. Return false to decline the token; otherwise return true with the new progress in
        /// <paramref name="next"/>. <paramref name="current"/> is a copy; stored progress is only changed by the service.
        /// Negative results are rejected by the service.
        /// </summary>
        protected abstract bool TryProgress(in TToken token, long current, out long next);

        internal bool Evaluate(in TToken token, long current, out long next) => TryProgress(in token, current, out next);
    }
}
