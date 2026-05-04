namespace CupkekGames.Combat
{
    /// <summary>
    /// Composable status-effect behavior — multiple per <see cref="StatusEffectSO"/>.
    /// Authored as <c>[SerializeReference]</c> entries on the status effect; each behavior
    /// implements one or more lifecycle hooks (<see cref="OnStart"/>, <see cref="OnTick"/>,
    /// <see cref="OnEnd"/>) and runs concurrently with others on the same status effect.
    /// </summary>
    public interface IStatusEffectBehaviorFeature
    {
        void OnStart(ICombatSettings combatSettings, ICombatManager manager,
            CombatUnit caster, CombatUnit target, int skillLevel);

        void OnTick(ICombatSettings combatSettings, ICombatManager manager,
            CombatUnit caster, CombatUnit target, int skillLevel);

        void OnEnd(ICombatSettings combatSettings, ICombatManager manager,
            CombatUnit caster, CombatUnit target, int skillLevel);
    }
}
