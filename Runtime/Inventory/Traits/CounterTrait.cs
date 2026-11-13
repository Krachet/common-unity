using System;

namespace Com.Krackhet.Runtime.Inventory
{
  /// <summary>
  /// Decides what the saved amount of an item means (stack count, durability, ...).
  /// An item can hold at most one counter; without one the item is unique (0/1).
  /// </summary>
  [Serializable]
  public abstract class CounterTrait : InventoryItemTrait
  {
    #region Interfaces & Properties
    /// <summary>Amount reported when the save has no entry. Not written to the save.</summary>
    public abstract int DefaultAmount { get; }
    #endregion

    #region Public Methods
    public abstract int Clamp(int amount);

    public override bool ConflictsWith(InventoryItemTrait other)
    {
      return other is CounterTrait || base.ConflictsWith(other);
    }
    #endregion
  }
}
