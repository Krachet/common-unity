using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.UI
{
    [CreateAssetMenu(fileName = "UIManagerConfiguration", menuName = "Krackhet/UI/UIManagerConfiguration")]
    public class BaseUIManagerConfiguration : ScriptableObject
    {
        [HideInInspector] public List<BaseUILayer> layerConfigurations = new List<BaseUILayer>();

        public List<BaseUILayerGroupConfiguration> layerGroupConfigurations = new List<BaseUILayerGroupConfiguration>();

        public void Initialize()
        {
            layerConfigurations = new List<BaseUILayer>();

            foreach (var groupConfig in layerGroupConfigurations)
            {
                foreach (var layer in groupConfig.layerPrefab)
                {
                    layer.SetLayerIndex(groupConfig.groupOrder);
                    layerConfigurations.Add(layer);
                }
            }
        }

        public TLayer GetLayerPrefab<TLayer>() where TLayer : BaseUILayer
        {
            for (int i = 0; i < layerConfigurations.Count; i++)
            {
                BaseUILayer layerConfig = layerConfigurations[i];
                if (layerConfig is TLayer)
                {
                    return layerConfig as TLayer;
                }
            }

            Debug.LogError($"No UI Layer of type {typeof(TLayer)} found in configuration.");
            return null;
        }
    }

    [System.Serializable]
    public class BaseUILayerGroupConfiguration
    {
        public int groupOrder;
        public List<BaseUILayer> layerPrefab;
    }
}