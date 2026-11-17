using System;
using System.Collections.Generic;
using Com.Krackhet.Runtime.Save;
using Newtonsoft.Json;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Persisted inventory amounts keyed by item id. Plain data only;
    /// rules and catalog sync live in InventoryService.
    /// </summary>
    [Serializable]
    public class InventorySaveModule : SaveModule
    {
        #region Interfaces & Properties
        [JsonIgnore]
        public override string Key => "Inventory";

        [JsonIgnore]
        protected override int CurrentDataVersion => 1;

        [JsonIgnore]
        public IReadOnlyDictionary<string, int> Amounts => _amounts;
        #endregion

        #region Private Fields
        [JsonProperty("amounts")]
        private Dictionary<string, int> _amounts = new Dictionary<string, int>();
        #endregion

        #region Public Methods
        public override void OnLoaded()
        {
            _amounts ??= new Dictionary<string, int>();
        }
        #endregion
    }
}
