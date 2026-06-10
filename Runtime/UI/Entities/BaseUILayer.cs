using UnityEngine;
using System;

namespace Com.Krackhet.Runtime.UI
{
    public class BaseUILayer : MonoBehaviour, IUILayer
    {
        #region Serialized Fields
        [SerializeField]
        protected RectTransform layerContent;

        [SerializeField]
        protected int layerIndex;

        protected bool isInitialized;

        protected IUIManager uiManager;

        protected Action onHideCallback;
        #endregion

        #region Public Properties
        public int LayerIndex => layerIndex;
        #endregion

        #region Private Methods
        private void CacheComponents()
        {
            if (layerContent == null)
                layerContent = layerContent.GetChild(0).GetComponent<RectTransform>();
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