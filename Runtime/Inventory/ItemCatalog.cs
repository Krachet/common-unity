using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Every item that exists in the game. Holds definitions only, never amounts.
    /// </summary>
    [CreateAssetMenu(menuName = "Krackhet/Inventory/Item Catalog", fileName = "ItemCatalog")]
    public class ItemCatalog : ScriptableObject
    {
        #region Interfaces & Properties
        public IReadOnlyList<ItemDefinition> Items => _items;
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("All item definitions in the game.")]
#endif
        [SerializeField]
        private List<ItemDefinition> _items = new();
        #endregion

        #region Public Methods
        /// <summary>Returns the first item of type T, or null.</summary>
        public T GetItem<T>() where T : ItemDefinition
        {
            foreach (ItemDefinition item in _items)
            {
                if (item is T match) return match;
            }

            return null;
        }

        /// <summary>Returns every item of type T.</summary>
        public List<T> GetItems<T>() where T : ItemDefinition
        {
            var result = new List<T>();
            foreach (ItemDefinition item in _items)
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
