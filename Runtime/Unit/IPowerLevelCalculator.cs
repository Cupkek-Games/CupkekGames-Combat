namespace CupkekGames.Combat
{
    public interface IPowerLevelCalculator
    {
        int GetATK(int bonusLevel = 0);
        int GetDEF(int bonusLevel = 0);
        bool TryCritical();
        float ApplyCritical(float damage);
    }
}
