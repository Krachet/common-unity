using System;
using System.Collections.Generic;

namespace Com.Krackhet.Runtime.UserData
{
    /// <summary>Balances keyed by currency id. <see cref="DefaultCurrency"/> also raises <c>GameEvents.OnCurrencyChanged</c>.</summary>
    [Serializable]
    [SaveModule("economy")]
    public class EconomyModule : SaveModule
    {
        public const string DefaultCurrency = "coins";

        public Dictionary<string, int> balances = new Dictionary<string, int>();

        public event Action<string, int> Changed;

        public int Get(string currency = DefaultCurrency)
        {
            return balances.TryGetValue(currency, out int amount) ? amount : 0;
        }

        public void Add(int amount, string currency = DefaultCurrency)
        {
            Set(currency, Get(currency) + amount);
        }

        public bool TrySpend(int amount, string currency = DefaultCurrency)
        {
            int balance = Get(currency);
            if (balance < amount) return false;

            Set(currency, balance - amount);
            return true;
        }

        private void Set(string currency, int amount)
        {
            balances[currency] = amount;
            RequestSave();
            Changed?.Invoke(currency, amount);
            if (currency == DefaultCurrency) GameEvents.RaiseCurrencyChanged(amount);
        }
    }
}
