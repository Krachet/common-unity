using System.Collections.Generic;
using UnityEngine;

namespace Com.Krackhet.Runtime.Achievements
{
    [CreateAssetMenu(menuName = "Krommon/Achievements/Definition")]
    public sealed class AchievementDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private AchievementRequirementPolicy _policy;
        [SerializeField] private List<AchievementRequirement> _requirements = new List<AchievementRequirement>();
        [SerializeField] private List<AchievementReward> _rewards = new List<AchievementReward>();
        public string Id => _id;
        public AchievementRequirementPolicy Policy => _policy;
        public IReadOnlyList<AchievementRequirement> Requirements => _requirements;
        public IReadOnlyList<AchievementReward> Rewards => _rewards;

        public void Configure(string id, AchievementRequirementPolicy policy,
            IEnumerable<AchievementRequirement> requirements, IEnumerable<AchievementReward> rewards = null)
        {
            _id = id;
            _policy = policy;
            _requirements = requirements == null ? new List<AchievementRequirement>() : new List<AchievementRequirement>(requirements);
            _rewards = rewards == null ? new List<AchievementReward>() : new List<AchievementReward>(rewards);
        }

        public static AchievementDefinition Create(string id, AchievementRequirementPolicy policy,
            params AchievementRequirement[] requirements)
        {
            var definition = CreateInstance<AchievementDefinition>();
            definition.Configure(id, policy, requirements);
            return definition;
        }

#if UNITY_EDITOR
        private void Reset() => EnsureEditorId();
        private void OnValidate() => EnsureEditorId();
        private void EnsureEditorId()
        {
            if (string.IsNullOrWhiteSpace(_id))
                _id = UnityEditor.GUID.Generate().ToString();
        }
#endif
    }
}
