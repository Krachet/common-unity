using System;
using System.Collections.Generic;
using System.Threading;
using Com.Krackhet.Runtime.Pattern.Singleton;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
    public interface IInventoryManager
    {
        InventoryBase Inventory { get; }
        ItemCatalog Catalog { get; }

        void SaveInventory();
        void LoadInventory();
    }

    /// <summary>
    /// Scene entry point for the inventory. Creates the game's inventory in
    /// InitializeAsync, seeds it from the catalog and exposes it to game code.
    /// </summary>
    public abstract class InventoryManagerBase<TSelf, TInventory> : Singleton<TSelf>, IInventoryManager
        where TSelf : InventoryManagerBase<TSelf, TInventory>
        where TInventory : InventoryBase
    {
        #region Constants
        private const string LogTag = "[Inventory]";
        #endregion

        #region Events & Delegates
        /// <summary>Raised after a runtime item's amount changes.</summary>
        public event Action<InventoryItem> Changed;
        #endregion

        #region Interfaces & Properties
        public ItemCatalog Catalog => _catalog;

        public TInventory Inventory { get; private set; }

        public bool IsInitialized => Inventory != null && Inventory.IsInitialized;

        InventoryBase IInventoryManager.Inventory => Inventory;
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Every item in the game. Each starts at its starting amount.")]
#endif
        [SerializeField]
        private ItemCatalog _catalog;
        #endregion

        #region Unity Callbacks
        protected override void OnDestroy()
        {
            if (Inventory != null)
            {
                Inventory.Changed -= HandleInventoryChanged;
            }

            base.OnDestroy();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates and seeds the inventory. Safe to call more than once; later calls return immediately.
        /// </summary>
        public UniTask InitializeAsync(
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsInitialized)
            {
                Inventory = CreateInventory();
                Inventory.Initialize(_catalog);
                Inventory.Changed += HandleInventoryChanged;
            }

            progress?.Report(1f);
            return UniTask.CompletedTask;
        }

        public InventoryItem GetItemByDefinition(ItemDefinition definition)
        {
            return EnsureInitialized() ? Inventory.GetItemByDefinition(definition) : null;
        }

        public InventoryItem GetItemById(string id)
        {
            return EnsureInitialized() ? Inventory.GetItemById(id) : null;
        }

        public InventoryItem GetItemByType<TDefinition>()
            where TDefinition : ItemDefinition
        {
            return EnsureInitialized() ? Inventory.GetItemByType<TDefinition>() : null;
        }

        public List<InventoryItem> GetItemsByType<TDefinition>()
            where TDefinition : ItemDefinition
        {
            return EnsureInitialized()
                ? Inventory.GetItemsByType<TDefinition>()
                : new List<InventoryItem>();
        }

        public InventoryItem GetOwnedItemByType<TDefinition>()
            where TDefinition : ItemDefinition
        {
            return EnsureInitialized() ? Inventory.GetOwnedItemByType<TDefinition>() : null;
        }

        public List<InventoryItem> GetOwnedItemsByType<TDefinition>()
            where TDefinition : ItemDefinition
        {
            return EnsureInitialized()
                ? Inventory.GetOwnedItemsByType<TDefinition>()
                : new List<InventoryItem>();
        }

        public int GetItemCount()
        {
            return EnsureInitialized() ? Inventory.GetItemCount() : 0;
        }

        public int GetItemCount<TDefinition>() where TDefinition : ItemDefinition
        {
            return EnsureInitialized() ? Inventory.GetItemCount<TDefinition>() : 0;
        }

        public int GetOwnedItemCount()
        {
            return EnsureInitialized() ? Inventory.GetOwnedItemCount() : 0;
        }

        public int GetOwnedItemCount<TDefinition>()
            where TDefinition : ItemDefinition
        {
            return EnsureInitialized() ? Inventory.GetOwnedItemCount<TDefinition>() : 0;
        }

        public bool Has(InventoryItem item, int amount = 1)
        {
            return EnsureInitialized() && Inventory.Has(item, amount);
        }

        public bool Add(InventoryItem item, int amount)
        {
            return EnsureInitialized() && Inventory.Add(item, amount);
        }

        public bool Remove(InventoryItem item, int amount)
        {
            return EnsureInitialized() && Inventory.Remove(item, amount);
        }

        public void SaveInventory()
        {
            throw new NotImplementedException();
        }

        public void LoadInventory()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Protected Methods
        /// <summary>Creates the game-specific inventory instance.</summary>
        protected abstract TInventory CreateInventory();
        #endregion

        #region Private Methods
        private bool EnsureInitialized()
        {
            if (IsInitialized) return true;

            Debug.LogError($"{LogTag} Used before InitializeAsync completed.", this);
            return false;
        }

        private void HandleInventoryChanged(InventoryItem item)
        {
            Changed?.Invoke(item);
        }
        #endregion
    }
}
