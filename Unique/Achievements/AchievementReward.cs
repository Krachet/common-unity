using Com.Krackhet.Runtime.Inventory;
using UnityEngine;

namespace Com.Krackhet.Unique.Achievements
{
    /// <summary>
    /// Runtime services a reward may grant into. Supplied by the game when an achievement is claimed so the shared
    /// achievement layer never reaches into game singletons.
    /// </summary>
    public interface IAchievementRewardContext
    {
        /// <summary>Initialized runtime inventory, or null while it is unavailable (claims then fail and stay claimable).</summary>
        InventoryBase Inventory { get; }

        /// <summary>
        /// Requests persistence of the state changed by granted rewards. Called once per applied claim, before the
        /// claimed flag is saved. Return false with an error when the save could not be requested (e.g. no save system).
        /// </summary>
        bool SaveGrantedRewards(out string error);
    }

    /// <summary>
    /// Immutable reward configuration attached to an <see cref="AchievementDefinition"/>. Rewards are never granted
    /// automatically: <see cref="AchievementService.TryClaim"/> preflights every reward with <see cref="CanGrant"/>
    /// and only then calls <see cref="Grant"/> on each of them.
    /// </summary>
    public abstract class AchievementReward : ScriptableObject
    {
        /// <summary>
        /// False when at most one reward of this exact type may be attached to an achievement. Each reward preflights
        /// against the current state independently, so types whose grants could interact (e.g. the same capped item)
        /// must keep all their entries in one asset to make the preflight cover the combined result.
        /// </summary>
        protected internal virtual bool AllowMultiplePerAchievement => true;

        /// <summary>Context-free configuration check, run while the service builds its catalog and before each claim.</summary>
        protected virtual bool Validate(out string error)
        {
            error = null;
            return true;
        }

        /// <summary>Side-effect-free check that <see cref="Grant"/> will fully succeed in <paramref name="context"/>.</summary>
        protected abstract bool CanGrant(IAchievementRewardContext context, out string error);

        /// <summary>Grants the reward. Only called after <see cref="CanGrant"/> returned true for every reward of the claim.</summary>
        protected abstract bool Grant(IAchievementRewardContext context, out string error);

        internal bool ValidateConfiguration(out string error) => Validate(out error);
        internal bool CanGrantTo(IAchievementRewardContext context, out string error) => CanGrant(context, out error);
        internal bool GrantTo(IAchievementRewardContext context, out string error) => Grant(context, out error);
    }
}
