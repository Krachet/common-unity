using System;

namespace Com.Krackhet.Runtime.Transactions
{
  [Serializable]
  public class FreePayment : TransactionPayment
  {
    #region Public Methods
    public override void Pay(Action onComplete, Action onFail)
    {
      onComplete?.Invoke();
    }
    #endregion
  }
}
