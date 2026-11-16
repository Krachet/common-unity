using System;
using System.Collections.Generic;

namespace Com.Krackhet.Runtime.Transactions
{
  /// <summary>
  /// Expiry trait: a duration expressed as whole days + hours (e.g. 1 day 12 hours).
  /// Pure data for now; nothing currently checks this against elapsed/real time.
  /// </summary>
  [Serializable]
  public class ExpireTrait : TransactionTrait
  {
    #region Interfaces & Properties
    public int Day => _day;

    public int Hour => _hour;
    #endregion

    #region Serialized Fields
    [UnityEngine.SerializeField]
    private int _day;

    [UnityEngine.SerializeField]
    private int _hour;
    #endregion

    #region Public Methods
    public override void Validate(TransactionDefinition owner, List<string> issues)
    {
      if (_day < 0 || _hour < 0)
      {
        issues.Add("ExpireTrait: day/hour cannot be negative.");
      }
      else if (_day == 0 && _hour == 0)
      {
        issues.Add("ExpireTrait: duration is zero (both day and hour are 0).");
      }
    }
    #endregion
  }
}
