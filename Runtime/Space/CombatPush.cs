namespace CupkekGames.Combat
{
    /// <summary>
    /// Pushes a unit through its fight's space (<see cref="ICombatSpace.Push"/>): a fallen unit
    /// or one that shrugs off pushes (<see cref="IControlImmunity"/>, <see cref="CombatControl.Push"/>)
    /// does not move; a push that moves it is reported (<see cref="EventDatabaseCombat.OnPushed"/>).
    /// </summary>
    public static class CombatPush
    {
        /// <summary>The seconds a pushed body takes to reach its new place.</summary>
        public const float DefaultDuration = 0.15f;

        /// <summary>Pushes <paramref name="unit"/> away from <paramref name="by"/>; returns how far it moved.</summary>
        public static int Push(ICombatManager manager, CombatUnit unit, CombatUnit by, int tiles, float duration = DefaultDuration)
        {
            if (manager == null) throw new System.ArgumentNullException(nameof(manager));
            if (unit == null) throw new System.ArgumentNullException(nameof(unit));
            if (tiles < 1 || !unit.IsAlive || (unit.ImmuneControls & CombatControl.Push) != CombatControl.None) return 0;

            int moved = manager.UnitManager.Space.Push(unit, by, tiles, duration);
            if (moved > 0) manager.EventDatabase.InvokeOnPushed(unit, by, moved);
            return moved;
        }
    }
}
