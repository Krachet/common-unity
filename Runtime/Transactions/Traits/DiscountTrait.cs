using System;
using System.Collections.Generic;

namespace Com.Krackhet.Runtime.Transactions
{
  /// <summary>
  /// Discount trait: a whole percent off, expressed as 1-100 (not a 0-1 fraction).
  /// Pure data for now; nothing currently reads this to change a displayed/charged price.
  /// </summary>
  [Serializable]
  public class DiscountTrait : TransactionTrait
  {
    #region Interfaces & Properties
    public int Percent => _percent;
    #endregion

    #region Serialized Fields
    [UnityEngine.Range(1, 100)]
    [UnityEngine.SerializeField]
    private int _percent = 10;
    #endregion

    #region Public Methods
    public override void Validate(TransactionDefinition owner, List<string> issues)
    {
      if (_percent < 1 || _percent > 100)
      {
        issues.Add($"DiscountTrait: percent {_percent} must be between 1 and 100.");
      }
    }
    #endregion
  }
}
