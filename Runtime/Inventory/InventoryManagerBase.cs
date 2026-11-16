using System;
using System.Threading;
using Com.Krackhet.Runtime.Pattern.Singleton;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Scene entry point for the inventory. Builds the InventoryService from the
    /// assigned catalog and the game's save module, syncs them, and persists changes.
    /// The service is built by InitializeAsync (after the save manager has loaded)
    /// or lazily on first access.
    /// </summary>
    public abstract class InventoryManagerBase<TSelf> : Singleton<TSelf>
        where TSelf : InventoryManagerBase<TSelf>
    {
        #region Constants
        private const string LogTag = "[InventoryManager]";
        #endregion

        #region Public Fields & Interfaces
        private Inventory _inventory;

        private InventorySaveModule _saveModule;
        #endregion

    }
}
