using System;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Runtime state for one catalog item. The definition owns its configuration
    /// and rules; this object owns only the current amount.
    /// </summary>
    public sealed class InventoryItem
    {
        #region Interfaces & Properties
        public ItemDefinition Definition { get; }

        public int Amount { get; private set; }
        #endregion

        #region Constructors
        internal InventoryItem(ItemDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Amount = definition.ClampAmount(definition.ItemStartingAmount);
        }
        #endregion

        #region Internal Methods
        internal void SetAmount(int amount)
        {
            Amount = Definition.ClampAmount(amount);
        }
        #endregion
    }
}
