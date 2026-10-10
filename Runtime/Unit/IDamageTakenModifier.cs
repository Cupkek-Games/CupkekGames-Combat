namespace CupkekGames.Combat
{
    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that change the
    /// damage hits on their unit deal, in feature order, after the attacker's
    /// <see cref="IDamageDealtModifier"/>s and before defense. Exact damage skips it.
    /// </summary>
    public interface IDamageTakenModifier
    {
        float ModifyDamageTaken(CombatUnit target, CombatUnit attacker, float damage, DamageContext ctx);
    }
}
