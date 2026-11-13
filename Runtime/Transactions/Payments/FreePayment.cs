using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.Transactions
{
  [Serializable]
  public class FreePayment : TransactionPayment
  {
    #region Serialized Fields
#if UNITY_EDITOR
    [Tooltip("Price text shown for this free transaction. May be empty.")]
#endif
    [SerializeField]
    private string _priceText;
    #endregion

    #region Public Methods
    public override void Pay(Action onComplete, Action onFail)
    {
      onComplete?.Invoke();
    }

    public override string GetPriceText() => _priceText;
    #endregion
  }
}
