using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

public static class UIExtensions
{
    public static void Stretch(this RectTransform rectTransform, float offset)
    {
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.one * offset;
        rectTransform.offsetMin = Vector2.one * -offset;
    }
    public static void AddListener(this EventTrigger eventTrigger, EventTriggerType eventTriggerType, UnityAction<BaseEventData> unityAction)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventTriggerType;
        entry.callback.AddListener(unityAction);
        eventTrigger.triggers.Add(entry);
    }
    public static Coroutine CrossFadeAlpha(this Graphic graphic, float delay, float fromAlpha, float toAlpha, float duration, AnimationCurve animationCurve, Action onFadeEnd)
    {
        return graphic.StartCoroutine(CrossFadeRoutine(graphic, delay, fromAlpha, toAlpha, duration, animationCurve, onFadeEnd));
    }
    private static IEnumerator CrossFadeRoutine(Graphic graphic, float delay, float fromAlpha, float toAlpha, float duration, AnimationCurve animationCurve, Action onFadeEnd)
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
}