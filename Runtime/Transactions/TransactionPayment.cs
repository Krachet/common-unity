using System;

namespace Com.Krackhet.Runtime.Transactions
{
  /// <summary>
  /// Plug-in payment method used by a transaction definition.
  /// Implementations must invoke exactly one completion callback for every payment attempt.
  /// </summary>
  [Serializable]
  public abstract class TransactionPayment
  {
    #region Public Methods
    public abstract void Pay(Action onComplete, Action onFail);

    /// <summary>Ready-to-display price text (may include game-specific rich text/icon tags). Empty when there is nothing to show.</summary>
    public abstract string GetPriceText();
    #endregion
  }
}
