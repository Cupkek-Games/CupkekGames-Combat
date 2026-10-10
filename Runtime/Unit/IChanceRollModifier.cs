namespace CupkekGames.Combat
{
    /// <summary>
    /// Implemented by <see cref="CupkekGames.Units.IUnitFeature"/> instances that make their
    /// unit lucky: every chance it rolls (<see cref="CombatUnit.RollChance"/>: its critical
    /// hits, its dodges, a game's own procs) rolls twice and keeps the better result.
    /// </summary>
    public interface IChanceRollModifier
    {
        bool RollsTwice(CombatUnit unit);
    }
}
