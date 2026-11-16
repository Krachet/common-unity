using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Base config for every inventory item. Concrete items add their own behaviour.
    /// Amount is the starting amount; MaxAmount of 0 means unlimited.
    /// </summary>
    [CreateAssetMenu(menuName = "Krommon/Inventory/Item", fileName = "Item")]
    public class InventoryItemDefinition : ScriptableObject
    {
        #region Interfaces & Properties
        public string Id => _id;

        public int Amount => _amount;

        public int MaxAmount => _maxAmount;

        public bool HasMaxAmount => _maxAmount > 0;
        #endregion

        #region Serialized Fields
#if UNITY_EDITOR
        [Tooltip("Unique, stable item id. Defaults to the asset name when empty.")]
#endif
        [SerializeField]
        private string _id;

#if UNITY_EDITOR
        [Tooltip("Starting amount of this item.")]
#endif
        [Min(0)]
        [SerializeField]
        private int _amount;

#if UNITY_EDITOR
        [Tooltip("Maximum amount that can be held. 0 means unlimited.")]
#endif
        [Min(0)]
        [SerializeField]
        private int _maxAmount;
        #endregion

        #region Unity Callbacks
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                _id = name;
            }

            _amount = ClampAmount(_amount);
        }
#endif
        #endregion

        #region Public Methods
        /// <summary>Clamps an amount to [0, MaxAmount], with no upper bound when unlimited.</summary>
        public virtual int ClampAmount(int amount)
        {
            if (amount < 0) return 0;

            return HasMaxAmount && amount > _maxAmount ? _maxAmount : amount;
        }
        #endregion
    }
}
