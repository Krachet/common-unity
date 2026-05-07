using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Com.Krackhet.Runtime.Utilities;
using Object = UnityEngine.Object;

namespace Com.Krackhet.Runtime.UI
{
    public static class GameUI
    {
        private static Canvas canvas;
        private static EventSystem eventSystem;
        private static List<UILayer> activeLayers;
        private static Dictionary<Type, UILayer> layers;
        private static Dictionary<Type, PopUpLayer> popUpLayers;
        private static Dictionary<int, Transform> groups;
        private const int GROUP_ORDER_SPACING = 10;
        public static Camera RenderCamera { get; private set; }
        public static Canvas Canvas => canvas;
        public static int ActiveLayerCount => activeLayers.Count;
        public static float CanvasScaleFactor => canvas.scaleFactor;
        public static void Displayable(bool value) => canvas.enabled = value;
        public static bool Interactable(bool value) => eventSystem.enabled = value;

        public static void Initialise()
        {
            activeLayers = new List<UILayer>();
            layers = new Dictionary<Type, UILayer>();
            popUpLayers = new Dictionary<Type, PopUpLayer>();
            groups = new Dictionary<int, Transform>();
            UICanvas existCanvas = Object.FindAnyObjectByType<UICanvas>();
            if (existCanvas != null) existCanvas.TryGetComponent(out canvas);
            else canvas = UICanvas.InstantiateUICanvas();
            eventSystem = EventSystem.current;
            RenderCamera = canvas.worldCamera;
        }
        public static void HideOnTop()
        {
            UILayer element = GetLastLayer();
            if (element != null && !element.UseManualHide)
            {
                element.Hide();
            }
        }
        public static void HideAll() => HideAll(false);
        public static void HideAll(bool ignoreManualHide)
        {
            if (activeLayers.Count == 0) return;
            int lastIndex = activeLayers.Count - 1;
            for (int index = lastIndex; index >= 0; index--)
                if (!activeLayers[index].UseManualHide || ignoreManualHide)
                    activeLayers[index].Hide();
        }
        public static void Add(UILayer layer)
        {
            if (!activeLayers.Contains(layer))
                activeLayers.Add(layer);
            layer.transform.SetAsLastSibling();
            if (layer.PopUpAttached != null)
            {
                foreach (var popUp in layer.PopUpAttached)
                {
                    PopUpLayer popUpLayer = GetPopUp(popUp);
                    if (popUpLayer != null)
                    {
                        popUpLayer.transform.SetParent(layer.transform);
                        popUpLayer.AttachedUILayer = layer;
                        if (!popUpLayer.IsActive)
                            popUpLayer.Show();
                    }
                }
            }
        }
        public static void Remove(UILayer layer)
        {
            activeLayers.Remove(layer);
            if (layer.PopUpAttached != null)
            {
                foreach (var popUp in layer.PopUpAttached)
                {
                    PopUpLayer popUpLayer = GetPopUp(popUp);
                    if (popUpLayer != null && popUpLayer.IsActive)
                        popUpLayer.Hide();
                }
            }
        }
        public static void Register(UILayer layer)
        {
            Type layerType = layer.GetType();
            if (layers.ContainsKey(layerType)) return;
            layers.Add(layerType, layer);
        }
        public static void Register(PopUpLayer layer)
        {
            Type layerType = layer.GetType();
            if (popUpLayers.ContainsKey(layerType)) return;
            popUpLayers.Add(layerType, layer);
        }
        public static void RegisterPopUp(PopUpLayer layer)
        {
            Type layerType = layer.GetType();
            if (popUpLayers.ContainsKey(layerType)) return;
            popUpLayers.Add(layerType, layer);
        }
        public static void Unregister(UILayer layer)
        {
            if (layers.ContainsKey(layer.GetType()))
                layers.Remove(layer.GetType());
        }
        public static void UnregisterPopUp(PopUpLayer layer)
        {
            if (popUpLayers.ContainsKey(layer.GetType()))
                popUpLayers.Remove(layer.GetType());
        }
        public static Layer Get<Layer>() where Layer : UILayer
        {
            Type layerType = typeof(Layer);
            if (layers.ContainsKey(layerType)) return layers[layerType] as Layer;
            Layer prefab = Resources.Load<Layer>("UI/" + layerType.Name);
            Layer layer = prefab != null ? Object.Instantiate(prefab) : default;
            if (layer != null)
            {
                Transform parent = GetGroup(layer.Order);
                layer.transform.SetParent(parent, false);
                if (layer.ResetPositionOnShow) layer.transform.Reset();
            }
            return layer;
        }
        public static PopUp GetPopUp<PopUp>() where PopUp : PopUpLayer
        {
            Type layerType = typeof(PopUp);
            if (popUpLayers.ContainsKey(layerType)) return popUpLayers[layerType] as PopUp;
            PopUp prefab = Resources.Load<PopUp>("UI/PopUps/" + layerType.Name);
            PopUp layer = prefab != null ? Object.Instantiate(prefab) : default;
            if (layer != null)
            {
                Transform parent = GetGroup(0);
                layer.transform.SetParent(parent, false);
            }
            return layer;
        }
        public static PopUpLayer GetPopUp(Type popUpType)
        {
            if (popUpLayers.ContainsKey(popUpType)) return popUpLayers[popUpType];
            PopUpLayer prefab = Resources.Load<PopUpLayer>("UI/PopUps/" + popUpType.Name);
            PopUpLayer layer = prefab != null ? Object.Instantiate(prefab) : default;
            if (layer != null)
            {
                Transform parent = GetGroup(0);
                layer.transform.SetParent(parent, false);
            }
            return layer;
        }
        public static UILayer GetLastLayer()
        {
            if (activeLayers.Count == 0) return null;
            int lastIndex = activeLayers.Count - 1;
            return activeLayers[lastIndex];
        }
        private static Transform GetGroup(int order)
        {
            if (!groups.ContainsKey(order))
            {
                GameObject group = new(GameHelper.CreateText("Group:[{0}]", order));
                RectTransform rectTransform = group.TryAddComponent<RectTransform>();
                rectTransform.SetParent(canvas.transform, false);
                rectTransform.Reset();
                rectTransform.Stretch(0);
                rectTransform.SetSiblingIndex(order);
                groups.Add(order, rectTransform);
                foreach (var item in groups) item.Value.SetSiblingIndex(item.Key);
                Canvas overrideCanvas = group.AddComponent<Canvas>();
                group.AddComponent<GraphicRaycaster>();
                group.layer = canvas.gameObject.layer;
                overrideCanvas.overrideSorting = true;
                overrideCanvas.sortingLayerName = canvas.sortingLayerName;
                overrideCanvas.sortingOrder = Mathf.Max(1, order * GROUP_ORDER_SPACING);
                overrideCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            }
            return groups[order];
        }
    }
}