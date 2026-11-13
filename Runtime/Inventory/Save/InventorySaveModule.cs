using System;
using System.Collections.Generic;
using Com.Krackhet.Runtime.Save;
using Newtonsoft.Json;

namespace Com.Krackhet.Runtime.Inventory
{
  /// <summary>
  /// Persisted inventory state. Plain data only; rules live in InventoryService.
  /// Amounts only hold items whose amount differs from their default.
  /// </summary>
  [Serializable]
  public class InventorySaveModule : SaveModule
  {
    #region Interfaces & Properties
    [JsonIgnore]
    public override string Key => "Inventory";

    /// <summary>Item id to amount.</summary>
    [JsonIgnore]
    public IReadOnlyDictionary<string, int> Amounts => _amounts;

    /// <summary>Slot to equipped item id.</summary>
    [JsonIgnore]
    public IReadOnlyDictionary<string, string> Equipped => _equipped;

    [JsonIgnore]
    internal Dictionary<string, int> MutableAmounts => _amounts;

    [JsonIgnore]
    internal Dictionary<string, string> MutableEquipped => _equipped;
    #endregion

    #region Private Fields
    [JsonProperty("amounts")]
    private Dictionary<string, int> _amounts = new Dictionary<string, int>();

    [JsonProperty("equipped")]
    private Dictionary<string, string> _equipped = new Dictionary<string, string>();
    #endregion

    #region Public Methods
    public override void OnLoaded()
    {
      _amounts ??= new Dictionary<string, int>();
      _equipped ??= new Dictionary<string, string>();
    }
    #endregion
  }
}
