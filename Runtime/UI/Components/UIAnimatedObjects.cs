using System;
using Com.Krackhet.Schemas;
using UnityEngine;
#if DOTWEEN
using DG.Tweening;
#endif

namespace Com.Krackhet.Runtime.UI.Components
{
    [Serializable]
    public class UIAnimatedObjects
    {
        public UIObjectAnimationType.AnimationIn animationInType;
        public UIObjectAnimationType.AnimationOut animationOutType;
        public GameObject animObject;
        public bool joinPreviousObject;
        public float animationDuration;
        public float animationDelay;

#if DOTWEEN
    public Tween AnimateIn(UIObjectAnimationType.AnimationIn animationType)
    {
        switch (animationType)
        {
            case UIObjectAnimationType.AnimationIn.FadeIn:
                return FadeIn();
            case UIObjectAnimationType.AnimationIn.ScaleIn:
                return ScaleIn();
            case UIObjectAnimationType.AnimationIn.FloatIn:
                return FloatIn();
        }
        return null;
    }

    public Tween AnimateIn()
    {
        return AnimateIn(animationInType);
    }

    private Tween FadeIn()
    {
        Sequence fadeInTween = DOTween.Sequence();
        CanvasGroup canvasGroup = animObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = animObject.AddComponent<CanvasGroup>();
        }
        fadeInTween.Append(canvasGroup.DOFade(1f, animationDuration).From(0f));
        return fadeInTween;
    }

    private Tween ScaleIn()
    {
        Sequence scaleInTween = DOTween.Sequence();
        scaleInTween.Append(animObject.transform
            .DOScale(Vector3.one, animationDuration)
            .SetEase(Ease.OutBack)
            .From(Vector3.zero));
        return scaleInTween;
    }

    private Tween FloatIn()
    {
        Sequence floatInTween = DOTween.Sequence();
        Vector3 startPos = animObject.transform.localPosition;
        animObject.transform.localPosition = new Vector3(startPos.x, startPos.y - 100f, startPos.z);
        floatInTween.Append(animObject.transform
            .DOLocalMoveY(startPos.y, animationDuration)
            .SetEase(Ease.OutBack));
        return floatInTween;
    }

    public Tween AnimateOut(UIObjectAnimationType.AnimationOut animationType)
    {
        switch (animationType)
        {
            case UIObjectAnimationType.AnimationOut.FadeOut:
                return FadeOut();
            case UIObjectAnimationType.AnimationOut.ScaleOut:
                return ScaleOut();
            case UIObjectAnimationType.AnimationOut.FloatOut:
                return FloatOut();
        }
        return null;
    }

    public Tween AnimateOut()
    {
        return AnimateOut(animationOutType);
    }

    private Tween FadeOut()
    {
        Sequence fadeOutTween = DOTween.Sequence();
        CanvasGroup canvasGroup = animObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = animObject.AddComponent<CanvasGroup>();
        }
        fadeOutTween.Append(canvasGroup.DOFade(0f, animationDuration).From(canvasGroup.alpha));
        return fadeOutTween;
    }

    private Tween ScaleOut()
    {
        Sequence scaleOutTween = DOTween.Sequence();
        scaleOutTween.Append(animObject.transform
            .DOScale(Vector3.zero, animationDuration)
            .SetEase(Ease.InBack)
            .From(animObject.transform.localScale));
        return scaleOutTween;
    }

    private Tween FloatOut()
    {
        Sequence floatOutTween = DOTween.Sequence();
        Vector3 startPos = animObject.transform.localPosition;
        floatOutTween.Append(animObject.transform
            .DOLocalMoveY(startPos.y - 100f, animationDuration)
            .SetEase(Ease.InBack));
        return floatOutTween;
    }
#endif
    }
}