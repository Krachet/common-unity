using UnityEngine;

namespace Com.Krackhet.Runtime.Pattern.Singleton
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null) instance = FindAnyObjectByType<T>();
                return instance;
            }
        }
        private void ClearInstance()
        {
            if (instance == this) instance = null;
        }
        protected virtual void Awake()
        {
            if (instance == null) instance = this as T;
            else if (instance != this) Destroy(gameObject);
        }
        protected virtual void OnDestroy() => ClearInstance();
        protected virtual void OnApplicationQuit() => ClearInstance();
    }
}