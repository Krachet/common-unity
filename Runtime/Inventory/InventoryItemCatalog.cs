using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Every item that exists in the game. Holds definitions only, never amounts.
    /// </summary>
    [CreateAssetMenu(menuName = "Krommon/Inventory/Item Catalog", fileName = "ItemCatalog")]
    public class InventoryItemCatalog : ScriptableObject
    {
        #region Interfaces & Properties
        public IReadOnlyList<InventoryItemDefinition> Items => _items;
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("All item definitions in the game.")]
#endif
        [SerializeField]
        private List<InventoryItemDefinition> _items = new();
        #endregion

        #region Public Methods
        /// <summary>Returns the first item of type T, or null.</summary>
        public T GetItem<T>() where T : InventoryItemDefinition
        {
            foreach (InventoryItemDefinition item in _items)
            {
                if (item is T match) return match;
            }

            return null;
        }

        /// <summary>Returns every item of type T.</summary>
        public List<T> GetItems<T>() where T : InventoryItemDefinition
        {
            var result = new List<T>();
            foreach (InventoryItemDefinition item in _items)
            {
                if (item is T match)
                {
                    result.Add(match);
                }
            }

            return result;
        }
        #endregion
    }
}
