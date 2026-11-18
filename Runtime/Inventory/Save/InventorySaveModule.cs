using System;
using System.Collections.Generic;
using Com.Krackhet.Runtime.Save;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
    /// <summary>
    /// Persisted inventory state keyed by item id. Plain data only;
    /// rules and catalog synchronization live in the runtime inventory.
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
        public IReadOnlyDictionary<string, InventoryItemSaveData> Items => _items;
        #endregion

        #region Private Fields
        [JsonProperty("items")]
        private Dictionary<string, InventoryItemSaveData> _items =
            new Dictionary<string, InventoryItemSaveData>();

        [JsonProperty("amounts", NullValueHandling = NullValueHandling.Ignore)]
        private Dictionary<string, int> _legacyAmounts;
        #endregion

        #region Public Methods
        public bool TryGetItem(string itemId, out InventoryItemSaveData item)
        {
            item = null;
            return _items != null && _items.TryGetValue(itemId, out item) && item != null;
        }

        public void SetAmount(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            _items ??= new Dictionary<string, InventoryItemSaveData>();
            if (!_items.TryGetValue(itemId, out InventoryItemSaveData item) || item == null)
            {
                item = new InventoryItemSaveData();
                _items[itemId] = item;
            }

            item.SetAmount(amount);
        }

        public override void OnLoaded()
        {
            _items ??= new Dictionary<string, InventoryItemSaveData>();

            foreach (string itemId in new List<string>(_items.Keys))
            {
                InventoryItemSaveData item = _items[itemId];
                if (item == null)
                {
                    _items[itemId] = new InventoryItemSaveData();
                    continue;
                }

                item.EnsureInitialized();
            }
        }
        #endregion

        #region Protected Methods
        protected override void MigrateData(int fromVersion)
        {
            if (fromVersion < 2 && _legacyAmounts != null)
            {
                _items ??= new Dictionary<string, InventoryItemSaveData>();

                foreach (KeyValuePair<string, int> legacyAmount in _legacyAmounts)
                {
                    SetAmount(legacyAmount.Key, legacyAmount.Value);
                }
            }

            _legacyAmounts = null;
        }
        #endregion
    }

    [Serializable]
    public sealed class InventoryItemSaveData
    {
        #region Interfaces & Properties
        [JsonProperty("amount")]
        public int Amount { get; private set; }

        [JsonIgnore]
        public IReadOnlyDictionary<string, JToken> Traits => _traits;
        #endregion

        #region Private Fields
        [JsonProperty("traits")]
        private Dictionary<string, JToken> _traits =
            new Dictionary<string, JToken>();
        #endregion

        #region Public Methods
        public void SetAmount(int amount)
        {
            Amount = Mathf.Max(0, amount);
        }

        public void SetTrait<TValue>(string key, TValue value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            _traits ??= new Dictionary<string, JToken>();
            _traits[key] = value is null
                ? JValue.CreateNull()
                : JToken.FromObject(value);
        }

        public bool TryGetTrait<TValue>(string key, out TValue value)
        {
            value = default;
            if (string.IsNullOrWhiteSpace(key) || _traits == null)
            {
                return false;
            }

            if (!_traits.TryGetValue(key, out JToken token) ||
                token == null ||
                token.Type == JTokenType.Null)
            {
                return false;
            }

            try
            {
                value = token.ToObject<TValue>();
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public void EnsureInitialized()
        {
            _traits ??= new Dictionary<string, JToken>();
            SetAmount(Amount);
        }
        #endregion
    }
}
