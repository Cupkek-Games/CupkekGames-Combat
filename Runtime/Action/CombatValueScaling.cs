namespace CupkekGames.Combat
{
    /// <summary>
    /// What a damage or heal amount scales off: <c>Flat + base × Multiplier</c>,
    /// where the base is the caster's attack attribute, or each target's own
    /// max health (a share of it: Multiplier 0.2 is a fifth).
    /// </summary>
    public enum CombatValueScaling
    {
        CasterAttribute,
        TargetMaxHealth,
    }
}
