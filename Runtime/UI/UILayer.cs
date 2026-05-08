using System;
using System.Collections.Generic;
using Com.Krackhet.Runtime.UI.Components;

#if DOTWEEN
using DG.Tweening;
#endif

using UnityEngine;

namespace Com.Krackhet.Runtime.UI
{
    public abstract class UILayer : MonoBehaviour
    {
        #region Events & Delegates
        protected Action _hideCallback;
        #endregion

        #region Interfaces & Properties
        public virtual int Order => 0;

        public virtual bool ResetPositionOnShow => false;

        public virtual bool UseManualHide => true;

        public virtual bool DestroyOnHide => false;

        public virtual List<Type> PopUpAttached => null;

        public bool IsActive => _context.activeSelf;
        #endregion

        #region Serialized Fields
        [Header("Default Settings")]
        [SerializeField]
        protected GameObject _context;

        [SerializeField]
        protected List<UIAnimatedObjects> _animatedObjects;
        #endregion

        #region Protected Fields
        protected bool _hasShownOnce;
        protected UILayer _previousLayer;
        #endregion

        #region Unity Callbacks
        protected virtual void Awake()
        {
            GameUI.Register(this);
            _context?.SetActive(false);
        }
        #endregion

        #region Public Methods
        public virtual void Show(Action hideCallback)
        {
            _hideCallback = hideCallback;
            Show();
        }

        public virtual void Init()
        {
        }

        public virtual void Show()
        {
            if (!_hasShownOnce)
            {
                _hasShownOnce = true;
                Init();
            }

            GameUI.Add(this);
            HideAnimatedObjectsAtStart();
            _context?.SetActive(true);
        }

        public virtual void Show(UILayer backTrackLayer)
        {
            _previousLayer = backTrackLayer;
            Show();
        }

        public virtual void Hide()
        {
            GameUI.Remove(this);

            _hideCallback?.Invoke();
            _previousLayer?.Show();

            _hideCallback = null;
            _previousLayer = null;

            if (DestroyOnHide)
            {
                GameUI.Unregister(this);
                Destroy(gameObject);
            }
            else
            {
                _context?.SetActive(false);
            }
        }

        public bool IsType<Layer>() where Layer : UILayer
        {
            Type layerType = GetType();
            Type compareType = typeof(Layer);
            return layerType == compareType || layerType.IsSubclassOf(compareType);
        }
        #endregion

        #region Protected Methods
#if DOTWEEN
        protected virtual Tween AnimateObjectsIn()
        {
            Sequence spawnSequence = DOTween.Sequence();

            foreach (UIAnimatedObjects animatedObject in _animatedObjects)
            {
                if (animatedObject.animObject == null)
                {
                    continue;
                }

                animatedObject.animObject.SetActive(true);

                if (animatedObject.joinPreviousObject)
                {
                    spawnSequence.Join(AnimateObjectIn(animatedObject));
                }
                else
                {
                    spawnSequence.Append(AnimateObjectIn(animatedObject));
                }
            }

            return spawnSequence;
        }

        protected virtual Tween AnimateObjectsOut()
        {
            Sequence hideSequence = DOTween.Sequence();

            foreach (UIAnimatedObjects animatedObject in _animatedObjects)
            {
                if (animatedObject.animObject == null)
                {
                    continue;
                }

                if (animatedObject.joinPreviousObject)
                {
                    hideSequence.Join(AnimateObjectOut(animatedObject));
                }
                else
                {
                    hideSequence.Append(AnimateObjectOut(animatedObject));
                }
            }

            return hideSequence;
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
            if (_animatedObjects == null)
            {
                return;
            }

            foreach (UIAnimatedObjects animatedObject in _animatedObjects)
            {
                if (animatedObject == null)
                {
                    continue;
                }

                animatedObject.animObject.SetActive(false);
            }
        }

        protected float GetAnimatedObjectsTimer(int objectIndex)
        {
            float timer = 0f;

            for (int i = 0; i < objectIndex; i++)
            {
                if (_animatedObjects[i].joinPreviousObject)
                {
                    continue;
                }

                timer += _animatedObjects[i].animationDuration;
            }

            return timer;
        }
        #endregion
    }
}
