using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.Transactions
{
  /// <summary>
  /// A purchasable/claimable transaction: pays through a <see cref="TransactionPayment"/>,
  /// then grants its optional <see cref="TransactionReward"/> on success.
  /// </summary>
  [CreateAssetMenu(
      fileName = "TransactionDefinition",
      menuName = "Krackhet/Transactions/Transaction Definition"
  )]
  public class TransactionDefinition : ScriptableObject
  {
    #region Interfaces & Properties
    public TransactionPayment Payment => _payment;

    public TransactionReward Reward => _reward;
    #endregion

    #region Serialized Fields
    [SerializeReference]
    private TransactionPayment _payment;

    [SerializeReference]
    private TransactionReward _reward; // optional: some transactions grant nothing
    #endregion

    #region Public Methods
    public string GetPriceText()
    {
      return _payment != null ? _payment.GetPriceText() : string.Empty;
    }

    public void Execute(Action onComplete, Action onFail)
    {
      if (_payment == null)
      {
        onFail?.Invoke();
        return;
      }

      _payment.Pay(
          () =>
          {
            _reward?.Grant();
            onComplete?.Invoke();
          },
          onFail
      );
    }
    #endregion
  }
}
