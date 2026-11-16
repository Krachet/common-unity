using System;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Com.Krackhet.Runtime.Transactions
{
  /// <summary>
  /// A purchasable/claimable transaction: pays through a <see cref="TransactionPayment"/>,
  /// then grants its optional <see cref="TransactionReward"/> on success.
  /// Behaviour beyond payment/reward (discount, expiry, ...) is plugged in via traits.
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

    /// <summary>Valid traits only: nulls and later conflicting traits are skipped.</summary>
    public IReadOnlyList<TransactionTrait> Traits
    {
      get
      {
        EnsureResolved();
        return _resolvedTraits;
      }
    }
    #endregion

    #region Serialized Fields
    [SerializeReference]
    private TransactionPayment _payment;

    [SerializeReference]
    private TransactionReward _reward; // optional: some transactions grant nothing

#if UNITY_EDITOR && ODIN_INSPECTOR
    [InfoBox("$IssuesMessage", InfoMessageType.Error, "HasIssues")]
#endif
#if UNITY_EDITOR
    [Tooltip("Plug-in behaviours, e.g. Discount, Expire.")]
#endif
    [SerializeReference]
    private List<TransactionTrait> _traits = new List<TransactionTrait>();
    #endregion

    #region Private Fields
    private List<TransactionTrait> _resolvedTraits;
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
      _resolvedTraits = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
      _resolvedTraits = null;
#if !ODIN_INSPECTOR
      LogIssues();
#endif
    }
#endif
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

    public bool TryGetTrait<T>(out T trait) where T : TransactionTrait
    {
      EnsureResolved();
      foreach (TransactionTrait candidate in _resolvedTraits)
      {
        if (candidate is T match)
        {
          trait = match;
          return true;
        }
      }

      trait = null;
      return false;
    }

    public T GetTrait<T>() where T : TransactionTrait
    {
      return TryGetTrait(out T trait) ? trait : null;
    }

    public bool HasTrait<T>() where T : TransactionTrait
    {
      return TryGetTrait<T>(out _);
    }

    /// <summary>Configuration problems: null traits, conflicts, trait data.</summary>
    public List<string> CollectIssues()
    {
      var issues = new List<string>();
      for (int i = 0; i < _traits.Count; i++)
      {
        TransactionTrait trait = _traits[i];
        if (trait == null)
        {
          issues.Add($"Trait #{i} is empty.");
          continue;
        }

        trait.Validate(this, issues);
        CollectConflicts(i, issues);
      }

      return issues;
    }
    #endregion

    #region Private Methods
    private static bool Conflicts(TransactionTrait first, TransactionTrait second)
    {
      return first.ConflictsWith(second) || second.ConflictsWith(first);
    }

    private void CollectConflicts(int index, List<string> issues)
    {
      TransactionTrait trait = _traits[index];
      for (int j = index + 1; j < _traits.Count; j++)
      {
        TransactionTrait other = _traits[j];
        if (other == null || !Conflicts(trait, other)) continue;

        issues.Add($"{trait.GetType().Name} conflicts with {other.GetType().Name} (#{j} ignored).");
      }
    }

    private void EnsureResolved()
    {
      if (_resolvedTraits != null) return;

      _resolvedTraits = new List<TransactionTrait>(_traits.Count);
      foreach (TransactionTrait trait in _traits)
      {
        if (trait == null || IsBlockedByResolved(trait)) continue;

        _resolvedTraits.Add(trait);
      }
    }

    private bool IsBlockedByResolved(TransactionTrait trait)
    {
      foreach (TransactionTrait kept in _resolvedTraits)
      {
        if (Conflicts(kept, trait)) return true;
      }

      return false;
    }
    #endregion

    #region Editor Code
#if UNITY_EDITOR
#if ODIN_INSPECTOR
    private bool HasIssues => CollectIssues().Count > 0;

    private string IssuesMessage => string.Join("\n", CollectIssues());
#else
    private void LogIssues()
    {
      foreach (string issue in CollectIssues())
      {
        Debug.LogWarning($"[TransactionDefinition] '{name}': {issue}", this);
      }
    }
#endif

    [ContextMenu("Add Trait/Discount")]
    private void AddDiscountTrait()
    {
      AddTrait(new DiscountTrait());
    }

    [ContextMenu("Add Trait/Expire")]
    private void AddExpireTrait()
    {
      AddTrait(new ExpireTrait());
    }

    private void AddTrait(TransactionTrait trait)
    {
      UnityEditor.Undo.RecordObject(this, "Add Transaction Trait");
      _traits.Add(trait);
      _resolvedTraits = null;
      UnityEditor.EditorUtility.SetDirty(this);
#if !ODIN_INSPECTOR
      LogIssues();
#endif
    }
#endif
    #endregion
  }
}
