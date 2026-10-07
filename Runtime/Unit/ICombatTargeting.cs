namespace CupkekGames.Combat
{
    /// <summary>
    /// How a fight's units pick whom they go after. A game names one per fight through
    /// <see cref="ICombatUnitManager.Targeting"/>; <see cref="CombatThreatTargeting"/> is the
    /// package's own (whoever drew the most threat).
    /// </summary>
    public interface ICombatTargeting
    {
        /// <summary>Seconds between asks while a unit walks or waits; 0 asks on every step of its AI.</summary>
        float Interval { get; }

        /// <summary>
        /// Whom <paramref name="caster"/> goes after. Asked when it has no target or its target
        /// fell (<paramref name="current"/> is then null), right after each action it takes, and
        /// every <see cref="Interval"/> while it walks or waits. <paramref name="range"/> is the
        /// range of the action it means to take. Return <paramref name="current"/> to keep it;
        /// null when nobody is left to go after.
        /// </summary>
        CombatUnit Pick(CombatUnit caster, CombatUnit current, float range);
    }
}
