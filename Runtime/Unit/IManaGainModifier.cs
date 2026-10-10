namespace CupkekGames.Combat
{
    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that speed up or
    /// slow down how fast their unit's mana fills: a multiplier on what its actions and the
    /// damage it takes give (<see cref="CombatUnitMana"/>), multiplied across features, 0 for
    /// none. A set amount (<see cref="CombatUnitMana.Increase"/>, a potion) stays exact.
    /// </summary>
    public interface IManaGainModifier
    {
        float GetManaGainMultiplier(CombatUnit unit);
    }
}
