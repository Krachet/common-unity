using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Krackhet.Runtime.Utilities
{
    public static class UIHelper
    {
        #region Extension Methods
        public static void Stretch(this RectTransform rectTransform, float offset)
        {
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.one * offset;
            rectTransform.offsetMin = Vector2.one * -offset;
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

        public static Coroutine CrossFadeAlpha(
            this Graphic graphic,
            float delay,
            float fromAlpha,
            float toAlpha,
            float duration,
            AnimationCurve animationCurve,
            Action onFadeEnd)
        {
            return graphic.StartCoroutine(
                CrossFadeRoutine(graphic, delay, fromAlpha, toAlpha, duration, animationCurve, onFadeEnd)
            );
        }
        #endregion

        #region Private Methods
        private static IEnumerator CrossFadeRoutine(
            Graphic graphic,
            float delay,
            float fromAlpha,
            float toAlpha,
            float duration,
            AnimationCurve animationCurve,
            Action onFadeEnd)
        {
            graphic.canvasRenderer.SetAlpha(fromAlpha);
            if (delay > 0) yield return new WaitForSeconds(delay);
            if (duration > 0)
            {
                for (float elapsed = 0; elapsed <= duration; elapsed += Time.deltaTime)
                {
                    float delta = Mathf.Clamp01(elapsed / duration);
                    if (animationCurve != null) delta = animationCurve.Evaluate(delta);
                    graphic.canvasRenderer.SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, delta));
                    yield return null;
                }
            }
            graphic.canvasRenderer.SetAlpha(toAlpha);
            onFadeEnd?.Invoke();
        }
        #endregion
    }
}
