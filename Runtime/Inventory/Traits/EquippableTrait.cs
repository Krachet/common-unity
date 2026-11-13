using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
  /// <summary>
  /// Allows the item to be equipped into a slot. Independent of any counter:
  /// equipping never consumes or changes the item's amount.
  /// </summary>
  [Serializable]
  public class EquippableTrait : InventoryItemTrait
  {
    #region Serialized Fields
#if UNITY_EDITOR
    [Tooltip("Slot this item equips into. Falls back to the item's Category when empty.")]
#endif
    [SerializeField]
    private string _slot;
    #endregion

    #region Public Methods
    public string GetSlot(InventoryItemDefinition owner)
    {
      if (!string.IsNullOrEmpty(_slot)) return _slot;
      return owner != null ? owner.Category : string.Empty;
    }
    #endregion
  }
}
