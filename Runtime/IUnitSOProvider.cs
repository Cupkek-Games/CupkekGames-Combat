using CupkekGames.Units;

namespace CupkekGames.Combat
{
  /// <summary>
  /// Abstracts the lookup of <see cref="UnitDefinitionSO"/> instances by key and team.
  /// Team 0 = heroes, other teams = enemies.
  /// </summary>
  public interface IUnitSOProvider
  {
    bool ContainsKey(string key, int teamId);
    UnitDefinitionSO GetUnitDefinition(string key, int teamId);
  }
}
