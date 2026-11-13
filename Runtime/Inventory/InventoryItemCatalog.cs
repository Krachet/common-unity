using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
  [CreateAssetMenu(
      fileName = "ItemCatalog",
      menuName = "Krackhet/Inventory/Item Catalog"
  )]
  public class InventoryItemCatalog : ScriptableObject
  {
    #region Interfaces & Properties
    public IReadOnlyList<InventoryItemDefinition> All => _items;
    #endregion

    #region Serialized Fields
    [SerializeField]
    private List<InventoryItemDefinition> _items = new List<InventoryItemDefinition>();
    #endregion

    #region Private Fields
    private Dictionary<string, InventoryItemDefinition> _lookup;
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
      _lookup = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
      _lookup = null;
      WarnDuplicateIds();
    }
#endif
    #endregion

    #region Public Methods
    public bool TryGet(string id, out InventoryItemDefinition item)
    {
      item = null;
      if (string.IsNullOrEmpty(id)) return false;

      EnsureLookup();
      return _lookup.TryGetValue(id, out item);
    }

    public InventoryItemDefinition Get(string id)
    {
      if (TryGet(id, out InventoryItemDefinition item)) return item;

      Debug.LogWarning($"[InventoryItemCatalog] Item '{id}' not found in '{name}'.", this);
      return null;
    }

    public bool Contains(string id)
    {
      return TryGet(id, out _);
    }

    public List<InventoryItemDefinition> GetByCategory(string category)
    {
      var result = new List<InventoryItemDefinition>();
      foreach (InventoryItemDefinition item in _items)
      {
        if (item != null && item.Category == category)
        {
          result.Add(item);
        }
      }

      return result;
    }

    public List<InventoryItemDefinition> GetWithTrait<T>() where T : InventoryItemTrait
    {
      var result = new List<InventoryItemDefinition>();
      foreach (InventoryItemDefinition item in _items)
      {
        if (item != null && item.HasTrait<T>())
        {
          result.Add(item);
        }
      }

      return result;
    }
    #endregion

    #region Private Methods
    private void EnsureLookup()
    {
      if (_lookup != null) return;

      _lookup = new Dictionary<string, InventoryItemDefinition>(_items.Count);
      foreach (InventoryItemDefinition item in _items)
      {
        if (item == null || string.IsNullOrEmpty(item.Id)) continue;

        if (!_lookup.TryAdd(item.Id, item))
        {
          Debug.LogWarning($"[InventoryItemCatalog] Duplicate id '{item.Id}' ignored ('{item.name}').", this);
        }
      }
    }

#if UNITY_EDITOR
    private void WarnDuplicateIds()
    {
      var seen = new HashSet<string>();
      foreach (InventoryItemDefinition item in _items)
      {
        if (item == null || string.IsNullOrEmpty(item.Id)) continue;

        if (!seen.Add(item.Id))
        {
          Debug.LogWarning($"[InventoryItemCatalog] '{name}' has duplicate id '{item.Id}'.", this);
        }
      }
    }
#endif
    #endregion
  }
}
