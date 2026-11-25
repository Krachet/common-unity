using System;
using Com.Krackhet.Runtime.Inventory;
using UnityEngine;

namespace Com.Krackhet.Unique.DailyReward
{
    /// <summary>
    /// One daily reward gift: the inventory item granted and its amount.
    /// Games derive a concrete prize to add their own presentation data and
    /// store it in <see cref="DailyRewardCycle"/> via [SerializeReference].
    /// </summary>
    [Serializable]
    public abstract class DailyRewardPrize
    {
        #region Interfaces & Properties
        public ItemDefinition Item => _item;

        public int Amount => _amount;

        public string Label => string.IsNullOrEmpty(_label) ? $"{_amount}" : _label;

        public virtual bool IsValid => _item != null && _amount > 0;
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Inventory item granted when this day is claimed.")]
#endif
        [SerializeField]
        protected ItemDefinition _item;

#if UNITY_EDITOR
        [Tooltip("Amount of the item granted.")]
#endif
        [Min(1)]
        [SerializeField]
        protected int _amount = 1;

#if UNITY_EDITOR
        [Tooltip("Optional label. Defaults to the amount when empty.")]
#endif
        [SerializeField]
        protected string _label;
        #endregion
    }
}
