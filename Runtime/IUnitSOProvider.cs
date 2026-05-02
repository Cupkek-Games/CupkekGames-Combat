using CupkekGames.Units;

namespace CupkekGames.Combat
{
  /// <summary>
  /// Abstracts the lookup of <see cref="UnitDefinitionSO"/> instances by key and team.
  /// Team is a caller-defined faction id; the provider may use it as a partition key
  /// (e.g. separate catalogs per faction).
  /// </summary>
  public interface IUnitSOProvider
  {
    bool ContainsKey(string key, int teamId);
    UnitDefinitionSO GetUnitDefinition(string key, int teamId);
  }
}
