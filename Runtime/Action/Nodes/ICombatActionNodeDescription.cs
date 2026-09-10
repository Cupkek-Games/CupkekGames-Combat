namespace CupkekGames.Combat
{
    /// <summary>
    /// A node that contributes a sentence fragment to its action's
    /// description ({nodeN} and {nodeN_dur} placeholders). Colours, icons and
    /// number formatting come from <see cref="ICombatRules.DescriptionStyle"/>
    /// and the damage type definitions, never from the authored text.
    /// </summary>
    public interface ICombatActionNodeDescription
    {
        public string GetDescription(int skillLevel, CombatUnit caster, ICombatRules rules);
        public string GetDescriptionDuration(int skillLevel, CombatUnit caster, ICombatRules rules);
    }
}
