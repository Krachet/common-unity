using System;
using System.Collections;
using System.Collections.Generic;
using Com.Krackhet.Schemas;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Krackhet.Runtime.Utilities
{
    public static class GameExtensions
    {
        private static readonly string[] currencyFormats =
        {
        string.Empty, "K", "M", "B", "T"
    };
        private static readonly System.Random random = new System.Random();
        private static readonly Dictionary<string, int> romanNumbers = new()
    {
        { "M", 1000 }, { "CM", 900 }, { "D", 500 }, { "CD", 400 },
        { "C", 100 }, { "XC", 90 }, { "L", 50 }, { "XL", 40 },
        { "X", 10 }, { "IX", 9 }, { "V", 5 }, { "IV", 4 }, { "I", 1 }
    };
        private static IEnumerator DoTask(float seconds, bool unscaledTime, Action task)
        {
            if (!unscaledTime) yield return GameHelper.GetWaitForSeconds(seconds);
            else yield return GameHelper.GetWaitForSecondsRealtime(seconds);
            task?.Invoke();
        }
        private static IEnumerator DoTask(Animator animator, string animationName, int layerIndex, Action endTask)
        {
            animator.Play(animationName, layerIndex);
            animator.Update(Time.deltaTime);
            yield return new WaitForAnimationFinish(animator, animationName, layerIndex);
            endTask?.Invoke();
        }
        public static Coroutine Schedule(this MonoBehaviour monoBehaviour, float seconds, bool unscaledTime, Action task)
        {
            return monoBehaviour.StartCoroutine(DoTask(seconds, unscaledTime, task));
        }
        public static Coroutine Schedule(this MonoBehaviour monoBehaviour, float seconds, Action task)
        {
            return monoBehaviour.StartCoroutine(DoTask(seconds, false, task));
        }
        public static Coroutine PlayAnimation(this MonoBehaviour monoBehaviour, Animator animator, string animationName, Action endTask)
        {
            return monoBehaviour.StartCoroutine(DoTask(animator, animationName, 0, endTask));
        }
        public static string ToCurrencyFormats(this double number) => number.ToCurrencyFormats(currencyFormats);
        public static string ToCurrencyFormats(this double number, string[] customFormats)
        {
            if (number >= 1000)
            {
                for (int i = 1; i < customFormats.Length; i++)
                {
                    double value = number / Math.Pow(10, 3 * i);
                    if (value >= 1000) continue;
                    value = Math.Round(value, value >= 100 ? 1 : 2);
                    return GameHelper.CreateText("{0}{1}", value, customFormats[i]);
                }
            }
            return Math.Round(number, 0, MidpointRounding.AwayFromZero).ToString();
        }
        public static string ToRoman(this int number)
        {
            string roman = string.Empty;
            foreach (KeyValuePair<string, int> item in romanNumbers)
            {
                if (number <= 0) break;
                while (number >= item.Value)
                {
                    roman += item.Key;
                    number -= item.Value;
                }
            }
            return roman;
        }
        public static Vector2 GetSnapPositionIntoView(this ScrollRect scrollRect, Transform target)
        {
            Canvas.ForceUpdateCanvases();
            Vector2 targetLocalPosition = scrollRect.content.InverseTransformPoint(target.position);
            Vector2 viewportLocalPosition = scrollRect.viewport.localPosition;
            Vector2 contentSize = scrollRect.content.rect.size;
            Vector2 viewportSize = scrollRect.viewport.rect.size;
            float x = 0;
            if (contentSize.x > viewportSize.x)
            {
                x = 0 - (viewportLocalPosition.x + targetLocalPosition.x);
                x = Mathf.Clamp(x, -(contentSize.x - viewportSize.x), 0);
            }
            float y = 0;
            if (contentSize.y > viewportSize.y)
            {
                y = 0 - (viewportLocalPosition.y + targetLocalPosition.y);
                y = Mathf.Clamp(y, 0, contentSize.y - viewportSize.y);
            }
            return new(x, y);
        }
        // public static T CloneObjectInList<T>(this List<T> listObject, T prefab, Transform parent) where T : Component
        // {
        //     foreach (T obj in listObject)
        //     {
        //         if (obj.gameObject.activeSelf) continue;
        //         obj.transform.SetParent(parent);
        //         obj.gameObject.SetActive(true);
        //         return obj;
        //     }
        //     T cloneObject = Object.Instantiate(prefab);
        //     cloneObject.transform.SetParent(parent);
        //     cloneObject.name = prefab.name;
        //     listObject.Add(cloneObject);
        //     return cloneObject;
        // }
        // public static GameObject CloneObjectInList(this List<GameObject> listObject, GameObject prefab, Transform parent)
        // {
        //     foreach (GameObject obj in listObject)
        //     {
        //         if (obj.activeSelf) continue;
        //         obj.transform.SetParent(parent);
        //         obj.SetActive(true);
        //         return obj;
        //     }
        //     GameObject cloneObject = Object.Instantiate(prefab);
        //     cloneObject.transform.SetParent(parent);
        //     cloneObject.name = prefab.name;
        //     listObject.Add(cloneObject);
        //     return cloneObject;
        // }
        public static T TryAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
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
                int randomIndex = random.Next(index + 1);
                T element = list[randomIndex];
                list[randomIndex] = list[index];
                list[index] = element;
            }
        }
        public static void SetActive<T>(this T component, bool value) where T : Component
        {
            component.gameObject.SetActive(value);
        }
        // public static void DeactivateObjectInList<T>(this List<T> listObject) where T : Component
        // {
        //     foreach (T obj in listObject) obj.gameObject.SetActive(false);
        // }
        // public static void DeactivateObjectInList(this List<GameObject> listObject)
        // {
        //     foreach (GameObject obj in listObject) obj.SetActive(false);
        // }
        public static void Fill<T>(this List<T> list, int capacity, T value)
        {
            while (list.Count < capacity) list.Add(value);
        }
        public static void SetParent<T>(this T component, Transform parent) where T : Component
        {
            component.transform.SetParent(parent);
        }
        public static void Reset(this Transform transform, Space space = Space.Self)
        {
            switch (space)
            {
                case Space.World:
                    transform.position = Vector3.zero;
                    transform.rotation = Quaternion.identity;
                    Vector3 lossyScale = transform.lossyScale;
                    transform.localScale = new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z);
                    break;
                case Space.Self:
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    transform.localScale = Vector3.one;
                    break;
            }
        }
        public static void SetCullingMask(this Camera camera, string layerName, int value)
        {
            switch (value)
            {
                case 0: camera.cullingMask &= ~(1 << LayerMask.NameToLayer(layerName)); break;
                case 1: camera.cullingMask |= 1 << LayerMask.NameToLayer(layerName); break;
                default: camera.cullingMask ^= 1 << LayerMask.NameToLayer(layerName); break;
            }
        }
        public static List<Vector3> ToListVector3(this List<Transform> transforms)
        {
            List<Vector3> vector3s = new List<Vector3>();
            foreach (Transform transform in transforms)
                vector3s.Add(transform.position);
            return vector3s;
        }
    }
}