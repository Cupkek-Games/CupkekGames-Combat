namespace CupkekGames.Combat
{
    /// <summary>How a dash moves its unit through the fight's space.</summary>
    public enum CombatDashMode
    {
        /// <summary>Slides along the way and stops before the first place it cannot take.</summary>
        Push,
        /// <summary>Passes over whoever is in the way and lands on the furthest free place it reaches.</summary>
        Through,
        /// <summary>Only the body moves, out and back: the unit keeps its place.</summary>
        Cosmetic,
    }
}
