using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com.Krackhet.Runtime.Utilities
{
    public static class CollectionHelper
    {
        #region Private Fields
        private static readonly Dictionary<float, WaitForSeconds> _waitForSeconds =
            new Dictionary<float, WaitForSeconds>();

        private static readonly Dictionary<float, WaitForSecondsRealtime> _waitForSecondsRealtime =
            new Dictionary<float, WaitForSecondsRealtime>();

        private static readonly System.Random _random = new System.Random();
        #endregion

        #region Public Methods
        public static WaitForSeconds GetWaitForSeconds(float seconds)
        {
            if (!_waitForSeconds.ContainsKey(seconds))
                _waitForSeconds.Add(seconds, new(seconds));
            return _waitForSeconds[seconds];
        }

        public static WaitForSecondsRealtime GetWaitForSecondsRealtime(float seconds)
        {
            if (!_waitForSecondsRealtime.ContainsKey(seconds))
                _waitForSecondsRealtime.Add(seconds, new(seconds));
            return _waitForSecondsRealtime[seconds];
        }
        #endregion

        #region Extension Methods
        public static T CloneObjectInList<T>(this List<T> listObject, T prefab, Transform parent)
            where T : Component
        {
            foreach (T obj in listObject)
            {
                if (obj.gameObject.activeSelf) continue;
                obj.transform.SetParent(parent);
                obj.gameObject.SetActive(true);
                return obj;
            }
            T cloneObject = Object.Instantiate(prefab);
            cloneObject.transform.SetParent(parent);
            cloneObject.name = prefab.name;
            listObject.Add(cloneObject);
            return cloneObject;
        }

        public static GameObject CloneObjectInList(
            this List<GameObject> listObject,
            GameObject prefab,
            Transform parent)
        {
            foreach (GameObject obj in listObject)
            {
                if (obj.activeSelf) continue;
                obj.transform.SetParent(parent);
                obj.SetActive(true);
                return obj;
            }
            GameObject cloneObject = Object.Instantiate(prefab);
            cloneObject.transform.SetParent(parent);
            cloneObject.name = prefab.name;
            listObject.Add(cloneObject);
            return cloneObject;
        }

        public static T FindReverse<T>(this List<T> list, Predicate<T> match)
        {
            for (int index = list.Count - 1; index >= 0; index--)
                if (match.Invoke(list[index])) return list[index];
            return default;
        }

        public static void Shuffle<T>(this List<T> list)
        {
            for (int index = list.Count - 1; index > 0; index--)
            {
                int randomIndex = _random.Next(index + 1);
                T element = list[randomIndex];
                list[randomIndex] = list[index];
                list[index] = element;
            }
        }

        public static void DeactivateObjectInList<T>(this List<T> listObject) where T : Component
        {
            foreach (T obj in listObject) obj.gameObject.SetActive(false);
        }

        public static void DeactivateObjectInList(this List<GameObject> listObject)
        {
            foreach (GameObject obj in listObject) obj.SetActive(false);
        }

        public static void Fill<T>(this List<T> list, int capacity, T value)
        {
            while (list.Count < capacity) list.Add(value);
        }

        public static List<Vector3> ToListVector3(this List<Transform> transforms)
        {
            List<Vector3> vector3s = new List<Vector3>();
            foreach (Transform transform in transforms)
                vector3s.Add(transform.position);
            return vector3s;
        }
        #endregion
    }
}
