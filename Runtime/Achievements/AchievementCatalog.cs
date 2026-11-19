using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Achievements
{
    [CreateAssetMenu(menuName = "Krommon/Achievements/Catalog")]
    public sealed class AchievementCatalog : ScriptableObject
    {
        [SerializeField] private List<AchievementDefinition> _definitions = new List<AchievementDefinition>();
        public IReadOnlyList<AchievementDefinition> Definitions => _definitions;

        public void SetDefinitions(IEnumerable<AchievementDefinition> definitions) =>
            _definitions = new List<AchievementDefinition>(definitions);
    }
}
