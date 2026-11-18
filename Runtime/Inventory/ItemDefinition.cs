using System.Collections.Generic;
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

        public IReadOnlyList<ITrait> Traits => _traits;
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

#if UNITY_EDITOR
        [Tooltip("Optional plug-in data describing what this item can do.")]
#endif
        [SerializeReference]
        private List<ITrait> _traits = new();
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
            ValidateTraits();
        }
#endif
        #endregion

        #region Public Methods
        /// <summary>Returns the first trait of the requested type, if any.</summary>
        public bool TryGetTrait<TTrait>(out TTrait trait) where TTrait : ITrait
        {
            trait = default;
            if (_traits == null) return false;

            foreach (ITrait candidate in _traits)
            {
                if (candidate is not TTrait match)
                {
                    continue;
                }

                trait = match;
                return true;
            }

            return false;
        }

        public bool HasTrait<TTrait>() where TTrait : ITrait
        {
            return TryGetTrait(out TTrait _);
        }

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

        #region Editor Code
#if UNITY_EDITOR
        private void ValidateTraits()
        {
            if (_traits == null)
            {
                _traits = new List<ITrait>();
                return;
            }

            _traits.RemoveAll(trait => trait == null);
            WarnOnDuplicatedTraits();
        }

        private void WarnOnDuplicatedTraits()
        {
            var seenTypes = new HashSet<System.Type>();
            foreach (ITrait trait in _traits)
            {
                if (seenTypes.Add(trait.GetType())) continue;

                Debug.LogWarning(
                    $"[Inventory] Item '{name}' has more than one "
                    + $"'{trait.GetType().Name}' trait.",
                    this
                );
            }
        }
#endif
        #endregion
    }
}
