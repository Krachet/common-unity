using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Com.Krackhet.Runtime.Inventory
{
  /// <summary>
  /// Identity of an item. Behaviour (stacking, equipping, ...) is plugged in via traits.
  /// </summary>
  [CreateAssetMenu(
      fileName = "ItemDefinition",
      menuName = "Krackhet/Inventory/Item Definition"
  )]
  public class InventoryItemDefinition : ScriptableObject
  {
    #region Interfaces & Properties
    public string Id => _id;

    public string DisplayName =>
        string.IsNullOrEmpty(_displayName) ? name : _displayName;

    public Sprite Icon => _icon;

    public string Category => _category;

    /// <summary>Valid traits only: nulls and later conflicting traits are skipped.</summary>
    public IReadOnlyList<InventoryItemTrait> Traits
    {
      get
      {
        EnsureResolved();
        return _resolvedTraits;
      }
    }

    /// <summary>The item's counter, or null when the item is unique (amount 0/1).</summary>
    public CounterTrait Counter
    {
      get
      {
        EnsureResolved();
        return _counter;
      }
    }

    public bool IsUnique => Counter == null;
    #endregion

    #region Serialized Fields
#if UNITY_EDITOR
    [Tooltip("Stable, unique identifier used as the save key. Never change it after release.")]
#endif
    [SerializeField]
    private string _id;

#if UNITY_EDITOR
    [Tooltip("Name shown to players. Falls back to the asset name when empty.")]
#endif
    [SerializeField]
    private string _displayName;

    [SerializeField]
    private Sprite _icon;

#if UNITY_EDITOR
    [Tooltip("Free-form grouping, e.g. currency, skin, booster.")]
#endif
    [SerializeField]
    private string _category;

#if UNITY_EDITOR && ODIN_INSPECTOR
    [InfoBox("$IssuesMessage", InfoMessageType.Error, "HasIssues")]
#endif
#if UNITY_EDITOR
    [Tooltip("Plug-in behaviours. At most one counter (e.g. Stackable) per item.")]
#endif
    [SerializeReference]
    private List<InventoryItemTrait> _traits = new List<InventoryItemTrait>();
    #endregion

    #region Private Fields
    private List<InventoryItemTrait> _resolvedTraits;
    private CounterTrait _counter;
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
    public bool TryGetTrait<T>(out T trait) where T : InventoryItemTrait
    {
      EnsureResolved();
      foreach (InventoryItemTrait candidate in _resolvedTraits)
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

    public T GetTrait<T>() where T : InventoryItemTrait
    {
      return TryGetTrait(out T trait) ? trait : null;
    }

    public bool HasTrait<T>() where T : InventoryItemTrait
    {
      return TryGetTrait<T>(out _);
    }

    /// <summary>Configuration problems: empty id, null traits, conflicts, trait data.</summary>
    public List<string> CollectIssues()
    {
      var issues = new List<string>();
      if (string.IsNullOrWhiteSpace(_id)) issues.Add("Id is empty.");

      for (int i = 0; i < _traits.Count; i++)
      {
        InventoryItemTrait trait = _traits[i];
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
    private static bool Conflicts(InventoryItemTrait first, InventoryItemTrait second)
    {
      return first.ConflictsWith(second) || second.ConflictsWith(first);
    }

    private void CollectConflicts(int index, List<string> issues)
    {
      InventoryItemTrait trait = _traits[index];
      for (int j = index + 1; j < _traits.Count; j++)
      {
        InventoryItemTrait other = _traits[j];
        if (other == null || !Conflicts(trait, other)) continue;

        issues.Add($"{trait.GetType().Name} conflicts with {other.GetType().Name} (#{j} ignored).");
      }
    }

    private void EnsureResolved()
    {
      if (_resolvedTraits != null) return;

      _resolvedTraits = new List<InventoryItemTrait>(_traits.Count);
      _counter = null;
      foreach (InventoryItemTrait trait in _traits)
      {
        if (trait == null || IsBlockedByResolved(trait)) continue;

        _resolvedTraits.Add(trait);
        if (_counter == null && trait is CounterTrait counter)
        {
          _counter = counter;
        }
      }
    }

    private bool IsBlockedByResolved(InventoryItemTrait trait)
    {
      foreach (InventoryItemTrait kept in _resolvedTraits)
      {
        if (Conflicts(kept, trait)) return true;
      }

      return false;
    }
    #endregion

    #region Editor Code
#if UNITY_EDITOR
    [ContextMenu("Generate Id")]
    private void GenerateId()
    {
      UnityEditor.Undo.RecordObject(this, "Generate Item Id");
      _id = name.Trim().Replace(' ', '_').ToLowerInvariant();
      UnityEditor.EditorUtility.SetDirty(this);
    }

#if ODIN_INSPECTOR
    private bool HasIssues => CollectIssues().Count > 0;

    private string IssuesMessage => string.Join("\n", CollectIssues());
#else
    private void LogIssues()
    {
      foreach (string issue in CollectIssues())
      {
        Debug.LogWarning($"[InventoryItemDefinition] '{name}': {issue}", this);
      }
    }

    [ContextMenu("Add Trait/Stackable")]
    private void AddStackableTrait()
    {
      AddTrait(new StackableTrait());
    }

    [ContextMenu("Add Trait/Equippable")]
    private void AddEquippableTrait()
    {
      AddTrait(new EquippableTrait());
    }

    private void AddTrait(InventoryItemTrait trait)
    {
      UnityEditor.Undo.RecordObject(this, "Add Item Trait");
      _traits.Add(trait);
      _resolvedTraits = null;
      UnityEditor.EditorUtility.SetDirty(this);
      LogIssues();
    }
#endif
#endif
    #endregion
  }
}
