using System;
using System.Collections.Generic;

namespace Com.Krackhet.Runtime.Inventory
{
  /// <summary>
  /// Plug-in behaviour attached to an <see cref="InventoryItemDefinition"/>.
  /// Traits are plain data; systems query them via InventoryItemDefinition.TryGetTrait.
  /// </summary>
  [Serializable]
  public abstract class InventoryItemTrait
  {
    #region Public Methods
    /// <summary>
    /// Whether this trait cannot coexist with <paramref name="other"/> on one item.
    /// Default: only one trait of each concrete type.
    /// </summary>
    public virtual bool ConflictsWith(InventoryItemTrait other)
    {
      return other != null && other.GetType() == GetType();
    }

    /// <summary>Reports problems with this trait's own data.</summary>
    public virtual void Validate(InventoryItemDefinition owner, List<string> issues)
    {
    }
    #endregion
  }
}
