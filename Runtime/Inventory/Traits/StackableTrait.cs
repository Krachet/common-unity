using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
  [Serializable]
  public class StackableTrait : CounterTrait
  {
    #region Interfaces & Properties
    public int MaxAmount => _maxAmount;

    public override int DefaultAmount => _defaultAmount;

    public bool IsUnlimited => _maxAmount <= 0;
    #endregion

    #region Serialized Fields
#if UNITY_EDITOR
    [Tooltip("Maximum amount that can be held. 0 = unlimited.")]
#endif
    [Min(0)]
    [SerializeField]
    private int _maxAmount;

#if UNITY_EDITOR
    [Tooltip("Amount reported when the save has no entry. Not written to the save.")]
#endif
    [Min(0)]
    [SerializeField]
    private int _defaultAmount;
    #endregion

    #region Public Methods
    public override int Clamp(int amount)
    {
      if (amount < 0) return 0;
      return IsUnlimited ? amount : Mathf.Min(amount, _maxAmount);
    }

    public override void Validate(InventoryItemDefinition owner, List<string> issues)
    {
      if (IsUnlimited || _defaultAmount <= _maxAmount) return;

      issues.Add(
          $"StackableTrait: DefaultAmount ({_defaultAmount}) is above MaxAmount ({_maxAmount})."
      );
    }
    #endregion
  }
}
