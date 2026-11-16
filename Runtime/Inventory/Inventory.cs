using System;
using UnityEngine;

namespace Com.Krackhet.Runtime.Inventory
{
    public abstract class Inventory : ISerializationCallbackReceiver
    {
        public void OnAfterDeserialize()
        {
            throw new NotImplementedException();
        }

        public void OnBeforeSerialize()
        {
            throw new NotImplementedException();
        }
    }
}