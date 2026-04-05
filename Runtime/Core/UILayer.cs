using System;
using System.Collections.Generic;
#if DOTWEEN
using DG.Tweening;
#endif
using UnityEngine;

public abstract class UILayer : MonoBehaviour
{
    #region Protected Field 
    protected Action onHideEvent;
    protected bool onFirstShow;
    protected UILayer previousLayer;
    #endregion

    #region Public Properties
    public virtual int Order => 0;
    public virtual bool UseManualHide => true;
    public virtual bool DestroyOnHide => false;
    public virtual bool ResetPositionOnShow => false;
    public virtual List<Type> PopUpAttached => null;
    public bool IsActive => context.activeSelf;
    #endregion

    #region Serialized Field
    [Header("Default Settings")]
    [SerializeField] protected GameObject context;
    [SerializeField] protected List<UIAnimatedObjects> animatedObjects;
    #endregion    

    #region Public Methods
    public virtual void Show(Action hideCallback)
    {
        onHideEvent = hideCallback;
        Show();
    }

    public virtual void Init()
    {
    }

    public virtual void Show()
    {
        if (!onFirstShow)
        {
            onFirstShow = true;
            Init();
        }
        GameUI.Add(this);
        HideAnimatedObjectsAtStart();
        context?.SetActive(true);
    }

    public virtual void Show(UILayer backTrackLayer)
    {
        previousLayer = backTrackLayer;
        Show();
    }

    public virtual void Hide()
    {
        GameUI.Remove(this);

        onHideEvent?.Invoke();
        previousLayer?.Show();

        onHideEvent = null;
        previousLayer = null;

        if (DestroyOnHide)
        {
            GameUI.Unregister(this);
            Destroy(gameObject);
        }
        else context?.SetActive(false);
    }

    public bool IsType<Layer>() where Layer : UILayer
    {
        Type layerType = GetType();
        Type compareType = typeof(Layer);
        return layerType == compareType || layerType.IsSubclassOf(compareType);
    }
    #endregion

    protected virtual void Awake()
    {
        GameUI.Register(this);
        context?.SetActive(false);
    }

    #region Protected Methods
#if DOTWEEN
    protected virtual Tween AnimateObjectsIn()
    {
        Sequence spawnSeq = DOTween.Sequence();
        foreach (UIAnimatedObjects animatedObject in animatedObjects)
        {
            if (animatedObject.animObject == null) continue;
            animatedObject.animObject.SetActive(true);
            if (animatedObject.joinPreviousObject)
                spawnSeq.Join(AnimateObjectIn(animatedObject));
            else
                spawnSeq.Append(AnimateObjectIn(animatedObject));
        }
        return spawnSeq;
    }

    protected virtual Tween AnimateObjectsOut()
    {
        Sequence hideSeq = DOTween.Sequence();
        foreach (UIAnimatedObjects animatedObject in animatedObjects)
        {
            if (animatedObject.animObject == null) continue;
            if (animatedObject.joinPreviousObject)
                hideSeq.Join(AnimateObjectOut(animatedObject));
            else
                hideSeq.Append(AnimateObjectOut(animatedObject));
        }
        return hideSeq;
    }

    protected virtual Tween AnimateObjectIn(UIAnimatedObjects animatedObject)
    {
        return animatedObject.AnimateIn();
    }

    protected virtual Tween AnimateObjectOut(UIAnimatedObjects animatedObject)
    {
        return animatedObject.AnimateOut();
    }
#endif

    protected virtual void HideAnimatedObjectsAtStart()
    {
        if (animatedObjects == null) return;
        foreach (UIAnimatedObjects animatedObject in animatedObjects)
        {
            if (animatedObject == null) continue;
            animatedObject.animObject.SetActive(false);
        }
    }

    protected float GetAnimatedObjectsTimer(int objectIndex)
    {
        float timer = 0;
        for (int i = 0; i < objectIndex; i++)
        {
            if (animatedObjects[i].joinPreviousObject) continue;
            timer += animatedObjects[i].animationDuration;
        }
        return timer;
    }
    #endregion
}
