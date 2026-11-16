using System;
using System.Collections.Generic;

namespace Com.Krackhet.Runtime.Transactions
{
  /// <summary>
  /// Plug-in behaviour attached to a <see cref="TransactionDefinition"/>.
  /// Traits are plain data; systems query them via TransactionDefinition.TryGetTrait.
  /// </summary>
  [Serializable]
  public abstract class TransactionTrait
  {
    #region Public Methods
    /// <summary>
    /// Whether this trait cannot coexist with <paramref name="other"/> on one transaction.
    /// Default: only one trait of each concrete type.
    /// </summary>
    public virtual bool ConflictsWith(TransactionTrait other)
    {
      return other != null && other.GetType() == GetType();
    }

    /// <summary>Reports problems with this trait's own data.</summary>
    public virtual void Validate(TransactionDefinition owner, List<string> issues)
    {
    }
    #endregion
  }
}
