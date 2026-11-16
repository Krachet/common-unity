using System;
using System.Collections.Generic;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Holds how many of each item the player has. Starts with every catalog
    /// item at its starting amount.
    /// </summary>
    public class Inventory
    {
        #region Events & Delegates
        /// <summary>Raised with the item and its new amount.</summary>
        public event Action<InventoryItemDefinition, int> Changed;
        #endregion

        #region Private Fields
        private readonly Dictionary<InventoryItemDefinition, int> _amounts = new();
        #endregion

        #region Constructors
        public Inventory(InventoryItemCatalog catalog)
        {
            if (catalog == null) return;

            foreach (InventoryItemDefinition item in catalog.Items)
            {
                if (item == null) continue;

                _amounts[item] = item.ClampAmount(item.Amount);
            }
        }
        #endregion

        #region Public Methods
        public int GetItemCount(InventoryItemDefinition item)
        {
            if (item == null) return 0;

            return _amounts.TryGetValue(item, out int amount) ? amount : 0;
        }

        public bool Has(InventoryItemDefinition item, int amount = 1)
        {
            return item != null && GetItemCount(item) >= amount;
        }

        /// <summary>Returns the first owned item of type T (count above 0), or null.</summary>
        public T GetItem<T>() where T : InventoryItemDefinition
        {
            foreach (KeyValuePair<InventoryItemDefinition, int> pair in _amounts)
            {
                if (pair.Value > 0 && pair.Key is T match) return match;
            }

            return null;
        }

        /// <summary>Returns every owned item of type T (count above 0).</summary>
        public List<T> GetItems<T>() where T : InventoryItemDefinition
        {
            var result = new List<T>();
            foreach (KeyValuePair<InventoryItemDefinition, int> pair in _amounts)
            {
                if (pair.Value > 0 && pair.Key is T match)
                {
                    result.Add(match);
                }
            }

            return result;
        }

        public void Add(InventoryItemDefinition item, int amount)
        {
            if (item == null || amount <= 0) return;

            Set(item, GetItemCount(item) + amount);
        }

        /// <summary>Removes the amount only if there is enough. Returns false otherwise.</summary>
        public bool Remove(InventoryItemDefinition item, int amount)
        {
            if (amount <= 0 || !Has(item, amount)) return false;

            Set(item, GetItemCount(item) - amount);
            return true;
        }
        #endregion

        #region Private Methods
        private void Set(InventoryItemDefinition item, int amount)
        {
            amount = item.ClampAmount(amount);
            _amounts[item] = amount;
            Changed?.Invoke(item, amount);
        }
        #endregion
    }
}
