namespace CupkekGames.Combat
{
    /// <summary>
    /// A node that contributes a sentence fragment to its action's
    /// description ({nodeN} and {nodeN_dur} placeholders). Colours, icons and
    /// number formatting come from <c>style</c>, the style of the surface the
    /// text is read on (<see cref="ICombatRules.DescriptionStyle"/> unless the
    /// caller of <see cref="CombatActionSO.GetDescription"/> names another),
    /// and the damage type definitions, never from the authored text.
    /// </summary>
    public interface ICombatActionNodeDescription
    {
        public string GetDescription(int skillLevel, CombatUnit caster, ICombatRules rules, CombatDescriptionStyleSO style);
        public string GetDescriptionDuration(int skillLevel, CombatUnit caster, ICombatRules rules, CombatDescriptionStyleSO style);
    }
}
