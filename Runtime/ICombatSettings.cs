namespace CupkekGames.Combat
{
    /// <summary>
    /// Composite settings interface. Implemented by game-specific settings classes.
    /// Inherits focused sub-interfaces so consumers can depend on only what they need.
    /// </summary>
    public interface ICombatSettings : ICombatRules, ICombatVisualSettings, IAutobattlerSettings
    {
    }
}
