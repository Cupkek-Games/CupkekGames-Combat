namespace CupkekGames.Combat
{
    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that keep a
    /// status off their unit (<see cref="CombatUnitStatusSystem.Add"/> refuses it).
    /// </summary>
    public interface IStatusImmunity
    {
        bool IsImmune(CombatUnit unit, StatusEffectSO status);
    }
}
