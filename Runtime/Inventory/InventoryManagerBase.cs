using System;
using Com.Krackhet.Runtime.Pattern.Singleton;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
  /// <summary>
  /// Scene entry point for the inventory. Builds the InventoryService lazily from
  /// the assigned catalog and the save module supplied by the game.
  /// </summary>
  public abstract class InventoryManagerBase<TSelf> : Singleton<TSelf>
      where TSelf : InventoryManagerBase<TSelf>
  {
    #region Constants
    private const string LogTag = "[InventoryManager]";
    #endregion

    #region Events & Delegates
    /// <summary>Item, old amount, new amount.</summary>
    public event Action<InventoryItemDefinition, int, int> ItemChanged;

    /// <summary>Slot, old item, new item. Either item may be null.</summary>
    public event Action<string, InventoryItemDefinition, InventoryItemDefinition> EquippedChanged;
    #endregion

    #region Interfaces & Properties
    public InventoryItemCatalog Catalog => _catalog;

    public bool IsReady => _inventoryService != null;

    /// <summary>Created on first access, so it does not depend on Awake order.</summary>
    public InventoryService InventoryService
    {
      get
      {
        EnsureInventoryService();
        return _inventoryService;
      }
    }
    #endregion

    #region Serialized Fields
#if UNITY_EDITOR
    [Tooltip("Every item the inventory can resolve by id.")]
#endif
    [SerializeField]
    private InventoryItemCatalog _catalog;
    #endregion

    #region Private Fields
    private InventoryService _inventoryService;
    #endregion

    #region Unity Callbacks
    protected override void Awake()
    {
      base.Awake();
      if (Instance != this) return;

      if (_catalog == null)
      {
        Debug.LogWarning($"{LogTag} No InventoryItemCatalog assigned on '{name}'.", this);
      }
    }

    protected override void OnDestroy()
    {
      ReleaseInventoryService();
      base.OnDestroy();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Drops the current inventory so the next access rebuilds it.
    /// Call after the save module is replaced (SaveManager Delete / DeleteAll).
    /// </summary>
    public void Rebuild()
    {
      ReleaseInventoryService();
    }
    #endregion

    #region Protected Methods
    /// <summary>Return the module from the game's save manager, e.g. SaveManager.Instance.Get&lt;InventorySaveModule&gt;().</summary>
    protected abstract InventorySaveModule ResolveSaveModule();

    /// <summary>Called once right after the inventory is built (migrations, starter grants, ...).</summary>
    protected virtual void OnInventoryServiceCreated(InventoryService inventoryService)
    {
    }
    #endregion

    #region Private Methods
    private void EnsureInventoryService()
    {
      if (_inventoryService != null) return;

      if (_catalog == null)
      {
        Debug.LogError($"{LogTag} Cannot build inventory without an InventoryItemCatalog.", this);
        return;
      }

      InventorySaveModule saveModule = ResolveSaveModule();
      if (saveModule == null)
      {
        Debug.LogError($"{LogTag} ResolveSaveModule returned null.", this);
        return;
      }

      _inventoryService = new InventoryService(_catalog, saveModule);
      _inventoryService.ItemChanged += HandleItemChanged;
      _inventoryService.EquippedChanged += HandleEquippedChanged;
      OnInventoryServiceCreated(_inventoryService);
    }

    private void ReleaseInventoryService()
    {
      if (_inventoryService == null) return;

      _inventoryService.ItemChanged -= HandleItemChanged;
      _inventoryService.EquippedChanged -= HandleEquippedChanged;
      _inventoryService = null;
    }

    private void HandleItemChanged(InventoryItemDefinition item, int previous, int next)
    {
      ItemChanged?.Invoke(item, previous, next);
    }

    private void HandleEquippedChanged(string slot, InventoryItemDefinition previous, InventoryItemDefinition next)
    {
      EquippedChanged?.Invoke(slot, previous, next);
    }
    #endregion
  }
}
