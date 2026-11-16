using System;

namespace Com.Krackhet.Runtime.Transactions
{
  /// <summary>
  /// Plug-in reward granted by a transaction definition.
  /// </summary>
  [Serializable]
  public abstract class TransactionReward
  {
    #region Public Methods
    public abstract void Grant();
    #endregion
  }
}
