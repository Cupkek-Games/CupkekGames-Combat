namespace CupkekGames.Combat
{
    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that can keep
    /// their unit from casting its ultimate: while any says no, a full bar selects the normal
    /// action instead (<see cref="CombatUnit.CanCastUltimate"/>). Unlike a silence, it never
    /// drops an ultimate already selected.
    /// </summary>
    public interface IUltimateGate
    {
        bool CanCastUltimate(CombatUnit unit);
    }
}
