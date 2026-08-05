using UnityEngine;

namespace Com.Krackhet.Runtime.UI.Utilities
{
    public class UISafeArea : MonoBehaviour
    {
        public bool adjustAnchorMin;
        public bool adjustAnchorMax;

        private void Awake()
        {
            SetDeviceSafeArea();
        }

        private void SetDeviceSafeArea()
        {
            Canvas.ForceUpdateCanvases();
            Rect safeAreaRect = Screen.safeArea;
            if (TryGetComponent(out RectTransform rectTransform))
            {
                if (adjustAnchorMax)
                {
                    Vector2 anchorMax = safeAreaRect.position + safeAreaRect.size;
                    anchorMax.x /= Screen.width;
                    anchorMax.y /= Screen.height;
                    rectTransform.anchorMax = anchorMax;
                }
                if (adjustAnchorMin)
                {
                    Vector2 anchorMin = safeAreaRect.position;
                    anchorMin.x /= Screen.width;
                    anchorMin.y /= Screen.height;
                    rectTransform.anchorMin = anchorMin;
                }
            }
        }
    }
}