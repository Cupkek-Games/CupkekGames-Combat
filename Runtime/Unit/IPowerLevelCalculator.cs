namespace CupkekGames.Combat
{
    /// <summary>A unit's power as the game shows it: its attack and its defense as single numbers.</summary>
    public interface IPowerLevelCalculator
    {
        int GetATK(int bonusLevel = 0);
        int GetDEF(int bonusLevel = 0);
    }
}
