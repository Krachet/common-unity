using UnityEngine;

namespace Com.Krackhet.Runtime.Achievements
{
    public abstract class AchievementTask : ScriptableObject
    {
        [SerializeField] private string _id;
        public string Id => _id;
        internal abstract long Combine(long current, long reported);

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
}
