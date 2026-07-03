using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.UI
{
    public class UIPanelResizable : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private RectTransform rectTransform;
        [SerializeField]
        private RectTransform targetRectTransform;
        [SerializeField]
        private ControlFlags controlFlags = ControlFlags.None;
        [SerializeField]
        private Vector2 factor = Vector2.one;
        #endregion
        #region Private Fields
#if UNITY_EDITOR
        private DrivenRectTransformTracker _drtt = new DrivenRectTransformTracker();
#endif
        #endregion
        #region Unity Methods
        private void Update()
        {
            ModifySize();
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
        }
        private void OnDrawGizmos()
        {
            DrivenTransformProperties drivenProperties = DrivenTransformProperties.None;
            if ((controlFlags & ControlFlags.Width) != 0)
            {
                drivenProperties |= DrivenTransformProperties.SizeDeltaX;
            }
            if ((controlFlags & ControlFlags.Height) != 0)
            {
                drivenProperties |= DrivenTransformProperties.SizeDeltaY;
            }
            _drtt.Clear();
            _drtt.Add(this, rectTransform, drivenProperties);
            ModifySize();
        }
#endif
        #endregion
        #region Private Methods
        private void ModifySize()
        {
            if (rectTransform == null || targetRectTransform == null) return;
            if ((controlFlags & ControlFlags.Width) != 0)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetRectTransform.rect.width * factor.x);
            }
            if ((controlFlags & ControlFlags.Height) != 0)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetRectTransform.rect.height * factor.y);
            }
        }
        #endregion
        #region Nested Classes
        [Flags]
        public enum ControlFlags
        {
            None = 0,
            Width = 1 << 0,
            Height = 1 << 1,
            Both = Width | Height
        }
        #endregion
    }
}