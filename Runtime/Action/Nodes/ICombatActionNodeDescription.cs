namespace CupkekGames.Combat
{
    public interface ICombatActionNodeDescription
    {
        public string GetDescription(int skillLevel, CombatUnit caster);
        public string GetDescriptionDuration(int skillLevel, CombatUnit caster);
    }
}