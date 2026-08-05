using System;
using Com.Krackhet.Runtime.UI.Utilities;
using UnityEngine;

namespace Com.Krackhet.Runtime.UI
{
    public class UILayerBase : MonoBehaviour, IUILayer
    {
        #region Serialized Fields
        [SerializeField]
        protected RectTransform layerContent;

        protected int layerIndex;

        protected bool isInitialized;

        protected IUIManager uiManager;

        protected Action onHideCallback;
        #endregion

        #region Public Properties
        public int LayerIndex => layerIndex;
        #endregion

        #region Protected Methods
        protected virtual void CacheComponents()
        {
            if (layerContent == null)
                layerContent = layerContent.GetChild(0).GetComponent<RectTransform>();

            if (layerContent.GetComponent<UISafeArea>() == null)
            {
                UISafeArea safeArea = layerContent.gameObject.AddComponent<UISafeArea>();
                safeArea.adjustAnchorMax = true;
                safeArea.adjustAnchorMin = true;
            }
        }
        #endregion

        #region Public Methods
        public virtual void Init(IUIManager manager, int layerIndex)
        {
            if (isInitialized)
                return;

            uiManager = manager;
        }

        public virtual void Show()
        {
            if (uiManager != null)
                uiManager.AddLayer(this);

            layerContent.gameObject.SetActive(true);
        }

        public virtual void Show(Action onHideAction)
        {
            onHideCallback = onHideAction;
            Show();
        }

        public virtual void Hide()
        {
            if (uiManager != null)
                uiManager.RemoveLayer(this);

            onHideCallback?.Invoke();
            layerContent.gameObject.SetActive(false);
        }

        public int GetLayerIndex() => layerIndex;
        public void SetLayerIndex(int index) => layerIndex = index;
        #endregion

        #region Unity Callbacks
        protected virtual void Awake()
        {
            CacheComponents();

            if (uiManager != null)
            {
                uiManager.RegisterLayer(this);
            }
        }

        protected virtual void OnValidate()
        {
            CacheComponents();
        }

        protected virtual void OnDestroy()
        {
            if (uiManager != null)
            {
                uiManager.UnregisterLayer(this);
            }
        }
        #endregion
    }
}