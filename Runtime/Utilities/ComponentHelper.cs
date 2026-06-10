using UnityEngine;

namespace Com.Krackhet.Runtime.Utilities
{
    public static class ComponentHelper
    {
        #region Extension Methods
        public static T TryAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
                component = gameObject.AddComponent<T>();

            return component;
        }

        public static void SetActive<T>(this T component, bool value) where T : Component
        {
            component.gameObject.SetActive(value);
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
                    transform.localScale = new Vector3(
                        1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z
                    );
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
        #endregion
    }
}
