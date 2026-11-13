using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
  /// <summary>
  /// Runtime rules over an InventorySaveModule: amounts, spending and equipping.
  /// Changes only touch the module in memory; the save manager persists it.
  /// </summary>
  public class InventoryService
  {
    #region Constants
    private const string LogTag = "[InventoryService]";
    #endregion

    #region Events & Delegates
    /// <summary>Item, old amount, new amount.</summary>
    public event Action<InventoryItemDefinition, int, int> ItemChanged;

    /// <summary>Slot, old item, new item. Either item may be null.</summary>
    public event Action<string, InventoryItemDefinition, InventoryItemDefinition> EquippedChanged;
    #endregion

    #region Interfaces & Properties
    public InventoryItemCatalog Catalog => _catalog;

    public InventorySaveModule Data => _data;
    #endregion

    #region Private Fields
    private readonly InventoryItemCatalog _catalog;
    private readonly InventorySaveModule _data;
    private readonly Dictionary<InventoryItemDefinition, int> _costBuffer = new Dictionary<InventoryItemDefinition, int>();
    #endregion

    #region Public Methods
    public InventoryService(InventoryItemCatalog catalog, InventorySaveModule data)
    {
      _catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
      _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public int GetAmount(InventoryItemDefinition item)
    {
      if (!IsValidItem(item)) return 0;

      return _data.MutableAmounts.TryGetValue(item.Id, out int amount)
          ? amount
          : GetDefaultAmount(item);
    }

    public int GetAmount(string id) => GetAmount(Resolve(id));

    public bool Has(InventoryItemDefinition item, int amount = 1) => GetAmount(item) >= amount;

    public bool Owns(InventoryItemDefinition item) => GetAmount(item) > 0;

    public bool Owns(string id) => Owns(Resolve(id));

    /// <summary>True when adding would actually increase the amount.</summary>
    public bool CanAdd(InventoryItemDefinition item, int amount = 1)
    {
      if (!IsValidItem(item) || amount <= 0) return false;

      int current = GetAmount(item);
      return ClampAmount(item, SafeSum(current, amount)) > current;
    }

    /// <summary>Adds clamped by the item's counter. Returns the amount actually added.</summary>
    public int Add(InventoryItemDefinition item, int amount = 1)
    {
      if (!IsValidItem(item) || !IsValidAmount(amount) || amount == 0) return 0;

      int current = GetAmount(item);
      int next = ClampAmount(item, SafeSum(current, amount));
      WriteAmount(item, current, next);
      return next - current;
    }

    public int Add(string id, int amount = 1) => Add(Resolve(id), amount);

    public bool TrySpend(InventoryItemDefinition item, int amount = 1)
    {
      if (!IsValidItem(item) || !IsValidAmount(amount)) return false;

      int current = GetAmount(item);
      if (current < amount) return false;

      WriteAmount(item, current, current - amount);
      return true;
    }

    public bool TrySpend(string id, int amount = 1) => TrySpend(Resolve(id), amount);

    /// <summary>Spends every cost or nothing. Duplicate items are summed.</summary>
    public bool TrySpend(IReadOnlyList<InventoryItemAmount> costs)
    {
      if (!CanAfford(costs)) return false;

      // Iterate the source list so ItemChanged handlers can safely re-enter CanAfford.
      foreach (InventoryItemAmount cost in costs)
      {
        if (!cost.IsValid) continue;

        int current = GetAmount(cost.Item);
        WriteAmount(cost.Item, current, current - cost.Amount);
      }

      return true;
    }

    public bool CanAfford(IReadOnlyList<InventoryItemAmount> costs)
    {
      if (costs == null || !CollectCosts(costs)) return false;

      bool canAfford = true;
      foreach (KeyValuePair<InventoryItemDefinition, int> cost in _costBuffer)
      {
        if (GetAmount(cost.Key) < cost.Value)
        {
          canAfford = false;
          break;
        }
      }

      _costBuffer.Clear();
      return canAfford;
    }

    /// <summary>Sets the amount directly (clamped). Intended for debugging and migrations.</summary>
    public void Set(InventoryItemDefinition item, int amount)
    {
      if (!IsValidItem(item) || !IsValidAmount(amount)) return;

      WriteAmount(item, GetAmount(item), ClampAmount(item, amount));
    }

    /// <summary>Equips an owned item with an EquippableTrait into its slot.</summary>
    public bool Equip(InventoryItemDefinition item)
    {
      if (!IsValidItem(item)) return false;

      if (!item.TryGetTrait(out EquippableTrait equippable))
      {
        Debug.LogWarning($"{LogTag} '{item.Id}' has no EquippableTrait.", item);
        return false;
      }

      if (!Owns(item)) return false;

      string slot = equippable.GetSlot(item);
      InventoryItemDefinition previous = GetEquipped(slot);
      if (previous == item) return true;

      _data.MutableEquipped[slot] = item.Id;
      EquippedChanged?.Invoke(slot, previous, item);
      return true;
    }

    public bool Equip(string id) => Equip(Resolve(id));

    public bool Unequip(string slot)
    {
      if (string.IsNullOrEmpty(slot)) return false;

      if (!_data.MutableEquipped.TryGetValue(slot, out string id)) return false;

      _data.MutableEquipped.Remove(slot);
      _catalog.TryGet(id, out InventoryItemDefinition previous);
      EquippedChanged?.Invoke(slot, previous, null);
      return true;
    }

    /// <summary>Equipped item in the slot, or null (also when the saved id is unknown).</summary>
    public InventoryItemDefinition GetEquipped(string slot)
    {
      if (string.IsNullOrEmpty(slot)) return null;

      if (!_data.MutableEquipped.TryGetValue(slot, out string id)) return null;

      return _catalog.TryGet(id, out InventoryItemDefinition item) ? item : null;
    }

    public bool IsEquipped(InventoryItemDefinition item)
    {
      if (item == null || !item.TryGetTrait(out EquippableTrait equippable)) return false;

      return GetEquipped(equippable.GetSlot(item)) == item;
    }
    #endregion

    #region Private Methods
    private static int GetDefaultAmount(InventoryItemDefinition item)
    {
      return item.Counter != null ? ClampAmount(item, item.Counter.DefaultAmount) : 0;
    }

    private static int ClampAmount(InventoryItemDefinition item, int amount)
    {
      return item.Counter != null ? item.Counter.Clamp(amount) : Mathf.Clamp(amount, 0, 1);
    }

    private static int SafeSum(int a, int b)
    {
      return (int)Math.Min((long)a + b, int.MaxValue);
    }

    private static bool IsValidAmount(int amount)
    {
      if (amount >= 0) return true;

      Debug.LogWarning($"{LogTag} Negative amount {amount} ignored.");
      return false;
    }

    private static bool IsValidItem(InventoryItemDefinition item)
    {
      if (item == null) return false;

      if (!string.IsNullOrEmpty(item.Id)) return true;

      Debug.LogWarning($"{LogTag} Item '{item.name}' has no id.", item);
      return false;
    }

    private InventoryItemDefinition Resolve(string id)
    {
      if (_catalog.TryGet(id, out InventoryItemDefinition item)) return item;

      Debug.LogWarning($"{LogTag} Unknown item id '{id}'.", _catalog);
      return null;
    }

    private bool CollectCosts(IReadOnlyList<InventoryItemAmount> costs)
    {
      _costBuffer.Clear();
      foreach (InventoryItemAmount cost in costs)
      {
        if (!cost.IsValid) continue;

        if (!IsValidItem(cost.Item)) return false;

        _costBuffer.TryGetValue(cost.Item, out int total);
        _costBuffer[cost.Item] = SafeSum(total, cost.Amount);
      }

      return true;
    }

    private void WriteAmount(InventoryItemDefinition item, int previous, int next)
    {
      if (previous == next) return;

      if (next == GetDefaultAmount(item))
      {
        _data.MutableAmounts.Remove(item.Id);
      }
      else
      {
        _data.MutableAmounts[item.Id] = next;
      }

      ItemChanged?.Invoke(item, previous, next);
    }
    #endregion
  }
}
