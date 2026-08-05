using System;
using System.Collections.Generic;
using Com.Krackhet.Runtime.Managers;
using Com.Krackhet.Runtime.Pattern.Singleton;
using Com.Krackhet.Runtime.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com.Krackhet.Runtime.UI
{
    public enum UIManagerStatus
    {
        NotInitialized,
        Initializing,
        Ready
    }

    public abstract class UIManagerBase<T> : Singleton<T>, IUIManager where T : UIManagerBase<T>
    {
        #region Constants
        private const int GROUP_ORDER_SPACING = 10;
        #endregion

        #region Protected Fields
        protected UIManagerStatus _status { get; set; }

        protected List<IUILayer> _activeLayers { get; set; }

        protected Dictionary<Type, IUILayer> _uiLayers { get; set; }

        protected Dictionary<int, Transform> _uiLayerGroups { get; set; }
        #endregion

        #region Serialized Fields
        [SerializeField]
        protected Canvas _uiCanvas;

        [SerializeField]
        protected Camera _renderCamera;

        [SerializeField]
        protected EventSystem _eventSystem;

        [SerializeField]
        protected BaseUIManagerConfiguration _configuration;
        #endregion

        #region Public Properties
        public Camera RenderCamera => _renderCamera;

        public Canvas UICanvas => _uiCanvas;

        public UIManagerStatus Status => _status;
        #endregion

        #region Unity Methods
        protected override void Awake()
        {
            base.Awake();
            GameInternalManager.RegisterManager(this);
            CacheComponents();
            _status = UIManagerStatus.NotInitialized;
        }

        void OnValidate()
        {
            CacheComponents();
        }
        #endregion

        #region Public Methods 
        public virtual void Initialize()
        {
            _status = UIManagerStatus.Initializing;
            _activeLayers = new List<IUILayer>();
            _uiLayers = new Dictionary<Type, IUILayer>();
            _uiLayerGroups = new Dictionary<int, Transform>();
            _configuration.Initialize();
            _status = UIManagerStatus.Ready;
        }

        public void RegisterLayer(IUILayer layer)
        {
            if (layer == null)
            {
                Debug.LogError("Cannot register a null UI Layer.");
                return;
            }

            Type layerType = layer.GetType();
            if (_uiLayers.TryGetValue(layerType, out IUILayer existingLayer))
            {
                Debug.LogWarning($"UI Layer of type {layerType} is already registered. Replacing existing layer.");
                _uiLayers[layerType] = layer;
            }
            else
            {
                _uiLayers[layerType] = layer;
            }
        }

        public void UnregisterLayer(IUILayer layer)
        {
            if (layer == null)
            {
                Debug.LogError("Cannot unregister a null UI Layer.");
                return;
            }

            Type layerType = layer.GetType();
            if (_uiLayers.ContainsKey(layerType))
            {
                _uiLayers.Remove(layerType);
            }
            else
            {
                Debug.LogWarning($"UI Layer of type {layerType} is not registered and cannot be unregistered.");
            }
        }

        public void AddLayer(IUILayer layer)
        {
            if (layer == null)
            {
                Debug.LogError("Cannot add a null UI Layer.");
                return;
            }

            if (!_activeLayers.Contains(layer))
            {
                _activeLayers.Add(layer);
            }
        }

        public void RemoveLayer(IUILayer layer)
        {
            if (layer == null)
            {
                Debug.LogError("Cannot remove a null UI Layer.");
                return;
            }

            if (_activeLayers.Contains(layer as UILayerBase))
            {
                _activeLayers.Remove(layer as UILayerBase);
            }
        }   

        public Layer GetUILayer<Layer>() where Layer : UILayerBase
        {
            Type layerType = typeof(Layer);
            if (_uiLayers.TryGetValue(layerType, out IUILayer layer))
                return layer as Layer;

            Layer layerPrefab = _configuration.GetLayerPrefab<Layer>();
            int layerIndex = layerPrefab != null ? layerPrefab.LayerIndex : -1;
            Layer newLayer = SpawnUILayer(layerPrefab, layerIndex);

            _uiLayers[layerType] = newLayer;
            if (newLayer != null)
            {
                newLayer.transform.SetParent(GetGroup(layerIndex), false);
            }

            return newLayer;
        }
        #endregion

        #region Private Methods
        private void CacheComponents()
        {
            if (_uiCanvas == null)
            {
                _uiCanvas = FindFirstObjectByType<Canvas>();
                if (_uiCanvas == null)
                    _uiCanvas = new GameObject("UICanvas").AddComponent<Canvas>();
            }

            if (_renderCamera == null)
            {
                _renderCamera = Camera.main;
                if (_renderCamera == null)
                    _renderCamera = new GameObject("UICamera").AddComponent<Camera>();
            }

            if (_eventSystem == null)
            {
                _eventSystem = FindFirstObjectByType<EventSystem>();
                if (_eventSystem == null)
                    _eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            }
        }

        private Layer SpawnUILayer<Layer>(Layer uiLayer, int layerIndex) where Layer : UILayerBase
        {
            if (uiLayer == null)
            {
                Debug.LogError($"Invalid configuration for UI Layer of type {typeof(Layer)}.");
                return null;
            }

            Layer layerInstance = Instantiate(uiLayer, _uiCanvas.transform);
            layerInstance.Init(this, layerIndex);
            _uiLayers[typeof(Layer)] = layerInstance;
            return layerInstance;
        }

        private Transform GetGroup(int order)
        {
            if (!_uiLayerGroups.ContainsKey(order))
            {
                GameObject group = new(StringHelper.CreateText("Group:[{0}]", order));
                RectTransform rectTransform = group.TryAddComponent<RectTransform>();
                rectTransform.SetParent(_uiCanvas.transform, false);
                rectTransform.Reset();
                rectTransform.Stretch(0);
                rectTransform.SetSiblingIndex(order);
                _uiLayerGroups.Add(order, rectTransform);
                foreach (var item in _uiLayerGroups) item.Value.SetSiblingIndex(item.Key);
                Canvas overrideCanvas = group.AddComponent<Canvas>();
                group.AddComponent<GraphicRaycaster>();
                group.layer = _uiCanvas.gameObject.layer;
                overrideCanvas.overrideSorting = true;
                overrideCanvas.sortingLayerName = _uiCanvas.sortingLayerName;
                overrideCanvas.sortingOrder = Mathf.Max(1, order * GROUP_ORDER_SPACING);
                overrideCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            }
            return _uiLayerGroups[order];
        }

        #endregion
    }
}