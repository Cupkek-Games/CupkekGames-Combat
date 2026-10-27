namespace CupkekGames.Combat
{
  /// <summary>
  /// The kinds of fragment a skill description is built from. Each role maps
  /// to one colour and one inline icon on <see cref="CombatDescriptionStyleSO"/>.
  /// Damage carries its own colour and icon on the damage type definition
  /// (a style may recolour a type for its surface); attribute buffs and
  /// debuffs read the attribute display config the style names.
  /// </summary>
  public enum CombatDescriptionRole
  {
    Heal,
    Shield,
    Status,
    Duration,
  }
}
