using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Owns the runtime item created for each catalog definition. Each game
    /// derives its own inventory and may add game-specific shortcuts.
    /// </summary>
    public abstract class InventoryBase
    {
        #region Events & Delegates
        /// <summary>Raised after a runtime item's amount changes.</summary>
        public event Action<InventoryItem> Changed;
        #endregion

        #region Interfaces & Properties
        public bool IsInitialized { get; private set; }

        public IReadOnlyList<InventoryItem> Items => _items;
        #endregion

        #region Private Fields
        private readonly List<InventoryItem> _items = new();
        private readonly Dictionary<string, InventoryItem> _itemsById = new();
        private readonly Dictionary<ItemDefinition, InventoryItem> _itemsByDefinition = new();
        #endregion

        #region Public Methods
        /// <summary>Creates one runtime item for every valid catalog definition.</summary>
        public void Initialize(ItemCatalog catalog)
        {
            _items.Clear();
            _itemsById.Clear();
            _itemsByDefinition.Clear();

            if (catalog != null && catalog.Items != null)
            {
                foreach (ItemDefinition definition in catalog.Items)
                {
                    AddDefinition(definition);
                }
            }

            IsInitialized = true;
        }

        public InventoryItem GetItemByDefinition(ItemDefinition definition)
        {
            if (definition == null) return null;

            return _itemsByDefinition.TryGetValue(definition, out InventoryItem item)
                ? item
                : null;
        }

        public InventoryItem GetItemById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            return _itemsById.TryGetValue(id, out InventoryItem item) ? item : null;
        }

        public InventoryItem GetItemByType<TDefinition>()
            where TDefinition : ItemDefinition
        {
            foreach (InventoryItem item in _items)
            {
                if (item.Definition is TDefinition) return item;
            }

            return null;
        }

        public List<InventoryItem> GetItemsByType<TDefinition>()
            where TDefinition : ItemDefinition
        {
            var result = new List<InventoryItem>();
            foreach (InventoryItem item in _items)
            {
                if (item.Definition is TDefinition)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public InventoryItem GetOwnedItemByType<TDefinition>()
            where TDefinition : ItemDefinition
        {
            foreach (InventoryItem item in _items)
            {
                if (item.Amount > 0 && item.Definition is TDefinition) return item;
            }

            return null;
        }

        public List<InventoryItem> GetOwnedItemsByType<TDefinition>()
            where TDefinition : ItemDefinition
        {
            var result = new List<InventoryItem>();
            foreach (InventoryItem item in _items)
            {
                if (item.Amount > 0 && item.Definition is TDefinition)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public int GetItemCount()
        {
            return _items.Count;
        }

        public int GetItemCount<TDefinition>() where TDefinition : ItemDefinition
        {
            return CountItems<TDefinition>(false);
        }

        public int GetOwnedItemCount()
        {
            int count = 0;
            foreach (InventoryItem item in _items)
            {
                if (item.Amount > 0)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetOwnedItemCount<TDefinition>() where TDefinition : ItemDefinition
        {
            return CountItems<TDefinition>(true);
        }

        public bool Has(InventoryItem item, int amount = 1)
        {
            return Owns(item) && amount > 0 && item.Amount >= amount;
        }

        public bool Add(InventoryItem item, int amount)
        {
            if (!Owns(item)) return false;

            bool accepted = item.Definition.TryGetAmountAfterAdd(
                item.Amount,
                amount,
                out int resultAmount
            );
            return accepted && ApplyAmount(item, resultAmount);
        }

        /// <summary>Removes the amount only if there is enough.</summary>
        public bool Remove(InventoryItem item, int amount)
        {
            if (!Owns(item)) return false;

            bool accepted = item.Definition.TryGetAmountAfterRemove(
                item.Amount,
                amount,
                out int resultAmount
            );
            return accepted && ApplyAmount(item, resultAmount);
        }
        #endregion

        #region Private Methods
        private void AddDefinition(ItemDefinition definition)
        {
            if (!IsValidDefinition(definition)) return;

            var item = new InventoryItem(definition);
            _items.Add(item);
            _itemsByDefinition.Add(definition, item);
            _itemsById.Add(definition.ItemId, item);
        }

        private bool IsValidDefinition(ItemDefinition definition)
        {
            if (definition == null)
            {
                Debug.LogError("[Inventory] Catalog contains a null item definition.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(definition.ItemId))
            {
                Debug.LogError(
                    $"[Inventory] Item definition '{definition.name}' has no item id.",
                    definition
                );
                return false;
            }

            if (_itemsByDefinition.ContainsKey(definition))
            {
                Debug.LogError(
                    $"[Inventory] Item definition '{definition.name}' is duplicated.",
                    definition
                );
                return false;
            }

            if (!_itemsById.ContainsKey(definition.ItemId)) return true;

            Debug.LogError(
                $"[Inventory] Item id '{definition.ItemId}' is duplicated.",
                definition
            );
            return false;
        }

        private bool Owns(InventoryItem item)
        {
            if (item == null || item.Definition == null) return false;

            return _itemsByDefinition.TryGetValue(
                item.Definition,
                out InventoryItem registeredItem
            ) && ReferenceEquals(item, registeredItem);
        }

        private bool ApplyAmount(InventoryItem item, int amount)
        {
            int clampedAmount = item.Definition.ClampAmount(amount);
            if (clampedAmount == item.Amount) return false;

            item.SetAmount(clampedAmount);
            Changed?.Invoke(item);
            return true;
        }

        private int CountItems<TDefinition>(bool ownedOnly)
            where TDefinition : ItemDefinition
        {
            int count = 0;
            foreach (InventoryItem item in _items)
            {
                if (item.Definition is not TDefinition)
                {
                    continue;
                }

                if (ownedOnly && item.Amount <= 0)
                {
                    continue;
                }

                count++;
            }

            return count;
        }
        #endregion
    }
}
