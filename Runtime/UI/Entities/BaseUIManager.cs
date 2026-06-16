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
    public abstract class BaseUIManager<T> : Singleton<T>, IUIManager where T : BaseUIManager<T>
    {
        #region Constants
        private const int GROUP_ORDER_SPACING = 10;
        #endregion

        #region Protected Fields
        protected List<IUILayer> activeLayers { get; set; }

        protected Dictionary<Type, IUILayer> uiLayers { get; set; }

        protected Dictionary<int, Transform> uiLayerGroups { get; set; }
        #endregion

        #region Serialized Fields
        [SerializeField]
        protected Canvas uiCanvas;

        [SerializeField]
        protected Camera renderCamera;

        [SerializeField]
        protected EventSystem eventSystem;

        [SerializeField]
        protected BaseUIManagerConfiguration configuration;
        #endregion

        #region Public Properties
        public Camera RenderCamera => renderCamera;

        public Canvas UICanvas => uiCanvas;
        #endregion

        #region Unity Methods
        protected override void Awake()
        {
            base.Awake();
            GameInternalManager.RegisterUIManager(this);
            CacheComponents();
        }

        void OnValidate()
        {
            CacheComponents();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                ToggleCanvas();
            }  
        }
        #endregion

        #region Public Methods 
        public virtual void Initialize()
        {
            activeLayers = new List<IUILayer>();
            uiLayers = new Dictionary<Type, IUILayer>();
            uiLayerGroups = new Dictionary<int, Transform>();
            configuration.Initialize();
        }

        public void RegisterLayer(IUILayer layer)
        {
            if (layer == null)
            {
                Debug.LogError("Cannot register a null UI Layer.");
                return;
            }

            Type layerType = layer.GetType();
            if (uiLayers.TryGetValue(layerType, out IUILayer existingLayer))
            {
                Debug.LogWarning($"UI Layer of type {layerType} is already registered. Replacing existing layer.");
                uiLayers[layerType] = layer;
            }
            else
            {
                uiLayers[layerType] = layer;
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
            if (uiLayers.ContainsKey(layerType))
            {
                uiLayers.Remove(layerType);
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

            if (!activeLayers.Contains(layer))
            {
                activeLayers.Add(layer);
            }
        }

        public void RemoveLayer(IUILayer layer)
        {
            if (layer == null)
            {
                Debug.LogError("Cannot remove a null UI Layer.");
                return;
            }

            if (activeLayers.Contains(layer as BaseUILayer))
            {
                activeLayers.Remove(layer as BaseUILayer);
            }
        }   

        public Layer GetUILayer<Layer>() where Layer : BaseUILayer
        {
            Type layerType = typeof(Layer);
            if (uiLayers.TryGetValue(layerType, out IUILayer layer))
                return layer as Layer;

            Layer layerPrefab = configuration.GetLayerPrefab<Layer>();
            int layerIndex = layerPrefab != null ? layerPrefab.LayerIndex : -1;
            Layer newLayer = SpawnUILayer(layerPrefab, layerIndex);

            uiLayers[layerType] = newLayer;
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
            if (uiCanvas == null)
            {
                uiCanvas = FindFirstObjectByType<Canvas>();
                if (uiCanvas == null)
                    uiCanvas = new GameObject("UICanvas").AddComponent<Canvas>();
            }

            if (renderCamera == null)
            {
                renderCamera = Camera.main;
                if (renderCamera == null)
                    renderCamera = new GameObject("UICamera").AddComponent<Camera>();
            }

            if (eventSystem == null)
            {
                eventSystem = FindFirstObjectByType<EventSystem>();
                if (eventSystem == null)
                    eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            }
        }

        private Layer SpawnUILayer<Layer>(Layer uiLayer, int layerIndex) where Layer : BaseUILayer
        {
            if (uiLayer == null)
            {
                Debug.LogError($"Invalid configuration for UI Layer of type {typeof(Layer)}.");
                return null;
            }

            Layer layerInstance = Instantiate(uiLayer, uiCanvas.transform);
            layerInstance.Init(this, layerIndex);
            uiLayers[typeof(Layer)] = layerInstance;
            return layerInstance;
        }

        private Transform GetGroup(int order)
        {
            if (!uiLayerGroups.ContainsKey(order))
            {
                GameObject group = new(StringHelper.CreateText("Group:[{0}]", order));
                RectTransform rectTransform = group.TryAddComponent<RectTransform>();
                rectTransform.SetParent(uiCanvas.transform, false);
                rectTransform.Reset();
                rectTransform.Stretch(0);
                rectTransform.SetSiblingIndex(order);
                uiLayerGroups.Add(order, rectTransform);
                foreach (var item in uiLayerGroups) item.Value.SetSiblingIndex(item.Key);
                Canvas overrideCanvas = group.AddComponent<Canvas>();
                group.AddComponent<GraphicRaycaster>();
                group.layer = uiCanvas.gameObject.layer;
                overrideCanvas.overrideSorting = true;
                overrideCanvas.sortingLayerName = uiCanvas.sortingLayerName;
                overrideCanvas.sortingOrder = Mathf.Max(1, order * GROUP_ORDER_SPACING);
                overrideCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            }
            return uiLayerGroups[order];
        }

        private void ToggleCanvas()
        {
            uiCanvas.SetActive(!uiCanvas.gameObject.activeSelf);
        }
        #endregion
    }
}