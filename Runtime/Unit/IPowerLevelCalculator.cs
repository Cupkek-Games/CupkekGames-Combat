namespace CupkekGames.Combat
{
    public interface IPowerLevelCalculator
    {
        int GetATK(int bonusLevel = 0);
        int GetDEF(int bonusLevel = 0);
        /// <summary>Rolls a crit with the fight's <paramref name="random"/>.</summary>
        bool TryCritical(CombatRandom random);
        float ApplyCritical(float damage);
    }
}
