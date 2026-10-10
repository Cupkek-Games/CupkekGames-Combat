namespace CupkekGames.Combat
{
    /// <summary>How a hit's critical roll goes.</summary>
    public enum CombatCritPolicy
    {
        /// <summary>Rolled on the attacker's critical chance.</summary>
        Roll,
        /// <summary>A sure critical hit.</summary>
        Always,
        /// <summary>Never a critical hit.</summary>
        Never,
    }

    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that force or
    /// forbid their unit's critical hits (every fourth shot, the first seconds of a fight).
    /// Asked for every hit <see cref="CombatDamageCalculator.CalculateAttackDamage"/> deals:
    /// a policy the call names wins; otherwise any feature's Never beats any feature's Always,
    /// and with neither the hit rolls.
    /// </summary>
    public interface ICritModifier
    {
        CombatCritPolicy GetCritPolicy(CombatUnit attacker, CombatUnit target, CombatSource source);
    }
}
