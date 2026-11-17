using UnityEngine;
using UnityEngine.Serialization;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Base config for every inventory item. Concrete items add their own behaviour.
    /// ItemStartingAmount seeds a new inventory; ItemMaxAmount of 0 means unlimited.
    /// </summary>
    public abstract class ItemDefinition : ScriptableObject
    {
        #region Interfaces & Properties
        public string ItemId => _itemId;

        public int ItemStartingAmount => _itemStartingAmount;

        public int ItemMaxAmount => _itemMaxAmount;

        public bool HasMaxAmount => _itemMaxAmount > 0;

        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Unique, stable item id. Defaults to the asset name when empty.")]
#endif
        [SerializeField]
        private string _itemId;

#if UNITY_EDITOR
        [Tooltip("Starting amount of this item.")]
#endif
        [Min(0)]
        [FormerlySerializedAs("_amount")]
        [SerializeField]
        private int _itemStartingAmount;

#if UNITY_EDITOR
        [Tooltip("Maximum amount that can be held. 0 means unlimited.")]
#endif
        [Min(0)]
        [SerializeField]
        private int _itemMaxAmount;
        #endregion

        #region Unity Callbacks
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_itemId))
            {
                _itemId = name;
            }

            _itemStartingAmount = ClampAmount(_itemStartingAmount);
        }
#endif
        #endregion

        #region Public Methods
        /// <summary>Clamps an amount to [0, MaxAmount], with no upper bound when unlimited.</summary>
        public virtual int ClampAmount(int amount)
        {
            if (amount < 0) return 0;

            return HasMaxAmount && amount > _itemMaxAmount ? _itemMaxAmount : amount;
        }

        public virtual bool TryGetAmountAfterAdd(
            int currentAmount,
            int amount,
            out int resultAmount
        )
        {
            resultAmount = ClampAmount(currentAmount);
            if (amount <= 0) return false;

            int maxIncrease = int.MaxValue - resultAmount;
            int increasedAmount = amount > maxIncrease
                ? int.MaxValue
                : resultAmount + amount;

            resultAmount = ClampAmount(increasedAmount);
            return true;
        }

        public virtual bool TryGetAmountAfterRemove(
            int currentAmount,
            int amount,
            out int resultAmount
        )
        {
            resultAmount = ClampAmount(currentAmount);
            if (amount <= 0 || resultAmount < amount) return false;

            resultAmount -= amount;
            return true;
        }
        #endregion
    }
}
