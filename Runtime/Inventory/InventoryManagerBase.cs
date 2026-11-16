using System;
using System.Collections.Generic;
using Com.Krackhet.Runtime.Pattern.Singleton;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Scene entry point for the inventory. Builds the Inventory from the
    /// assigned catalog and exposes it to game code.
    /// </summary>
    public abstract class InventoryManagerBase<TSelf> : Singleton<TSelf>
        where TSelf : InventoryManagerBase<TSelf>
    {
        #region Events & Delegates
        /// <summary>Raised with the item and its new amount.</summary>
        public event Action<InventoryItemDefinition, int> Changed
        {
            add => Inventory.Changed += value;
            remove => Inventory.Changed -= value;
        }
        #endregion

        #region Interfaces & Properties
        public InventoryItemCatalog Catalog => _catalog;

        public Inventory Inventory => _inventory ??= new Inventory(_catalog);
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Every item in the game. Each starts at its starting amount.")]
#endif
        [SerializeField]
        private InventoryItemCatalog _catalog;
        #endregion

        #region Private Fields
        private Inventory _inventory;
        #endregion

        #region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();
            _ = Inventory;
        }
        #endregion

        #region Public Methods
        public int GetItemCount(InventoryItemDefinition item) => Inventory.GetItemCount(item);

        public bool Has(InventoryItemDefinition item, int amount = 1) => Inventory.Has(item, amount);

        public T GetItem<T>() where T : InventoryItemDefinition => Inventory.GetItem<T>();

        public List<T> GetItems<T>() where T : InventoryItemDefinition => Inventory.GetItems<T>();

        public void Add(InventoryItemDefinition item, int amount) => Inventory.Add(item, amount);

        public bool Remove(InventoryItemDefinition item, int amount) => Inventory.Remove(item, amount);
        #endregion
    }
}
