using System;
using System.Collections.Generic;
using Com.Krackhet.Runtime.Inventory;
using UnityEngine;

namespace Com.Krackhet.Unique.Achievements
{
    /// <summary>One item of an <see cref="InventoryItemsAchievementReward"/>: a definition and the positive amount to add.</summary>
    [Serializable]
    public sealed class InventoryItemRewardEntry
    {
        [SerializeField] private ItemDefinition _item;
        [Min(1)] [SerializeField] private int _amount = 1;
        public ItemDefinition Item => _item;
        public int Amount => _amount;

        /// <summary>Used by Unity/inspector serialization so new list elements get the field defaults.</summary>
        public InventoryItemRewardEntry() { }

        public InventoryItemRewardEntry(ItemDefinition item, int amount)
        {
            _item = item;
            _amount = amount;
        }
    }

    /// <summary>
    /// Adds a list of inventory items on claim. Every entry is checked against the inventory before any amount is
    /// added, so predictable failures (missing item, already at max amount, ...) never produce a partial grant.
    /// Each item may appear only once, and an achievement may carry only one reward of this type so the preflight
    /// covers every inventory change of the claim. An item whose amount would not change (e.g. an owned max-1 skin)
    /// blocks the claim.
    /// </summary>
    [CreateAssetMenu(menuName = "Krackhet/Achievements/Rewards/Inventory Items")]
    public sealed class InventoryItemsAchievementReward : AchievementReward
    {
        [SerializeField] private List<InventoryItemRewardEntry> _items = new List<InventoryItemRewardEntry>();
        public IReadOnlyList<InventoryItemRewardEntry> Items => _items;

        public static InventoryItemsAchievementReward Create(params InventoryItemRewardEntry[] items)
        {
            var reward = CreateInstance<InventoryItemsAchievementReward>();
            reward._items = items == null ? new List<InventoryItemRewardEntry>() : new List<InventoryItemRewardEntry>(items);
            return reward;
        }

        protected internal override bool AllowMultiplePerAchievement => false;

        protected override bool Validate(out string error)
        {
            if (_items == null || _items.Count == 0) { error = $"Reward '{name}' has no items"; return false; }
            var seen = new HashSet<ItemDefinition>();
            for (var i = 0; i < _items.Count; i++)
            {
                var entry = _items[i];
                if (entry == null || entry.Item == null) { error = $"Reward '{name}' item #{i} has no item definition"; return false; }
                if (entry.Amount <= 0) { error = $"Reward '{name}' item '{entry.Item.name}' has non-positive amount {entry.Amount}"; return false; }
                if (!seen.Add(entry.Item)) { error = $"Reward '{name}' lists item '{entry.Item.name}' more than once"; return false; }
            }
            error = null;
            return true;
        }

        protected override bool CanGrant(IAchievementRewardContext context, out string error)
        {
            var inventory = context?.Inventory;
            if (inventory == null || !inventory.IsInitialized) { error = $"Reward '{name}': inventory is unavailable."; return false; }
            foreach (var entry in _items)
            {
                var item = inventory.GetItemByDefinition(entry.Item);
                if (item == null) { error = $"Reward '{name}': item '{entry.Item.name}' is not in the inventory."; return false; }
                // Mirrors InventoryBase.Add, which rejects adds that are refused or leave the amount unchanged.
                if (!entry.Item.TryGetAmountAfterAdd(item.Amount, entry.Amount, out var result) || result == item.Amount)
                {
                    error = $"Reward '{name}': cannot add {entry.Amount} x '{entry.Item.name}' (current {item.Amount}, max {entry.Item.ItemMaxAmount}).";
                    return false;
                }
            }
            error = null;
            return true;
        }

        protected override bool Grant(IAchievementRewardContext context, out string error)
        {
            var inventory = context.Inventory;
            for (var i = 0; i < _items.Count; i++)
            {
                var entry = _items[i];
                if (inventory.Add(inventory.GetItemByDefinition(entry.Item), entry.Amount)) continue;
                error = $"Reward '{name}': adding {entry.Amount} x '{entry.Item.name}' failed after preflight; {i} earlier item(s) were already added.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
