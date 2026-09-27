namespace CupkekGames.Combat
{
    /// <summary>
    /// Composable status-effect behavior — multiple per <see cref="StatusEffectSO"/>.
    /// Authored as <c>[SerializeReference]</c> entries on the status effect; each behavior
    /// implements one or more lifecycle hooks (<see cref="OnStart"/>, <see cref="OnTick"/>,
    /// <see cref="OnEnd"/>) and runs concurrently with others on the same status effect.
    /// The effect carries its level, the unit that put it on (<see cref="StatusEffect.Applier"/>)
    /// and the source its hits name (<see cref="StatusEffect.Source"/>).
    /// </summary>
    public interface IStatusEffectBehaviorFeature
    {
        void OnStart(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer);

        void OnTick(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer);

        void OnEnd(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer);
    }
}
