using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
  [Serializable]
  public struct InventoryItemAmount
  {
    #region Interfaces & Properties
    public InventoryItemDefinition Item => _item;

    public int Amount => _amount;

    public bool IsValid => _item != null && _amount > 0;
    #endregion

    #region Serialized Fields
    [SerializeField]
    private InventoryItemDefinition _item;

    [Min(0)]
    [SerializeField]
    private int _amount;
    #endregion

    #region Public Methods
    public InventoryItemAmount(InventoryItemDefinition item, int amount)
    {
      _item = item;
      _amount = Mathf.Max(0, amount);
    }

    public override string ToString()
    {
      string id = _item != null ? _item.Id : "<none>";
      return $"{id} x{_amount}";
    }
    #endregion
  }
}
