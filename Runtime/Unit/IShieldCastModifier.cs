using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that
    /// change the shields their unit casts (<see cref="CombatActionNodeShield"/>),
    /// in feature order: the amount, and an attribute effect the shield carries
    /// to its target for exactly as long as the shield lasts.
    /// </summary>
    public interface IShieldCastModifier
    {
        int ModifyShieldAmount(CombatUnit caster, int amount);

        /// <summary>An effect the cast shield carries, combined with the action's own; null for none.</summary>
        AttributeEffect GetShieldEffect(CombatUnit caster);
    }
}
