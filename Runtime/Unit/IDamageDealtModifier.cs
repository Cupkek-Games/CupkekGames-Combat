namespace CupkekGames.Combat
{
    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that change the
    /// damage their unit's hits deal, in feature order, before the target's defense
    /// (<see cref="CombatDamageCalculator.CalculateAttackDamage"/>). Exact damage skips it.
    /// </summary>
    public interface IDamageDealtModifier
    {
        float ModifyDamageDealt(CombatUnit attacker, CombatUnit target, float damage, DamageContext ctx);
    }
}
