namespace CupkekGames.Combat
{
    /// <summary>
    /// What an ultimate is doing (<see cref="EventDatabaseCombat.OnUltimate"/>):
    /// selected once, then completed or cancelled once.
    /// </summary>
    public enum UltimatePhase
    {
        /// <summary>Chosen at full mana and queued; the freeze, when it has one, starts at its queue turn.</summary>
        Selected,
        /// <summary>Cast to the end.</summary>
        Completed,
        /// <summary>Dropped before its end: stunned, silenced before it started, dead, or failed to find a target.</summary>
        Cancelled,
    }
}
