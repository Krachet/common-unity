using System;
using System.Collections.Generic;

namespace Com.Krackhet.Runtime.UserData
{
    /// <summary>Unlocked ids and the selected id per category (e.g. skin type -> skin index).</summary>
    [Serializable]
    [SaveModule("unlocks")]
    public class UnlocksModule : SaveModule
    {
        public Dictionary<int, List<int>> unlocked = new Dictionary<int, List<int>>();
        public Dictionary<int, int> selected = new Dictionary<int, int>();

        public bool IsUnlocked(int category, int id)
        {
            return unlocked.TryGetValue(category, out List<int> ids) && ids.Contains(id);
        }

        /// <returns>False if it was already unlocked.</returns>
        public bool TryUnlock(int category, int id)
        {
            if (!unlocked.TryGetValue(category, out List<int> ids))
            {
                ids = new List<int>();
                unlocked[category] = ids;
            }

            if (ids.Contains(id)) return false;

            ids.Add(id);
            RequestSave();
            return true;
        }

        public int GetSelected(int category, int defaultId = 0)
        {
            return selected.TryGetValue(category, out int id) ? id : defaultId;
        }

        public void Select(int category, int id)
        {
            selected[category] = id;
            RequestSave();
        }
    }
}
