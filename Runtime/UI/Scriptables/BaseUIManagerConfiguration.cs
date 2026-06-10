using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.Krackhet.Runtime.UI
{
    [CreateAssetMenu(fileName = "UIManagerConfiguration", menuName = "Krackhet/UI/UIManagerConfiguration")]
    public class BaseUIManagerConfiguration : ScriptableObject
    {
        public List<BaseUILayer> layerConfigurations = new List<BaseUILayer>();

        public List<BaseUILayerGroupConfiguration> layerGroupConfigurations = new List<BaseUILayerGroupConfiguration>();

        public void Initialize()
        {
            layerConfigurations = layerGroupConfigurations != null ? 
                layerGroupConfigurations.SelectMany(group => group.layerPrefab).ToList() : 
                new List<BaseUILayer>();
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