using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Com.Krackhet.Runtime.Utilities
{
    public static class EventHelper
    {
        #region Private Fields
        private static readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

        private static PointerEventData _pointerEventData;
        #endregion

        #region Public Methods
        public static bool IsPointerOverGameObject(Vector2 screenPosition)
        {
            return IsPointerOverGameObject(screenPosition, string.Empty);
        }

        public static bool IsPointerOverGameObject(Vector2 screenPosition, string ignoreLayer)
        {
            _raycastResults.Clear();
            if (EventSystem.current == null) return true;
            _pointerEventData ??= new PointerEventData(EventSystem.current);
            _pointerEventData.position = screenPosition;
            EventSystem.current.RaycastAll(_pointerEventData, _raycastResults);
            if (!string.IsNullOrWhiteSpace(ignoreLayer) && _raycastResults.Count > 0)
            {
                int layer = LayerMask.NameToLayer(ignoreLayer);
                _raycastResults.RemoveAll(item => item.gameObject.layer == layer);
            }
            return _raycastResults.Count > 0;
        }
        #endregion

        #region Extension Methods
        public static void AddListener(
            this EventTrigger eventTrigger,
            EventTriggerType eventTriggerType,
            UnityAction<BaseEventData> unityAction)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = eventTriggerType;
            entry.callback.AddListener(unityAction);
            eventTrigger.triggers.Add(entry);
        }
        #endregion
    }
}
