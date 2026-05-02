using UnityEngine;
using CupkekGames.TimeSystem;
using CupkekGames.Units;

namespace CupkekGames.Combat
{
  [System.Serializable]
  public class CombatUnitReference
  {
    [SerializeField] public int TeamId;
    [SerializeField] public string Key;
    [SerializeField] public int Level = 1;

    public CombatUnitReference(int teamId, string key, int level)
    {
      TeamId = teamId;
      Key = key;
      Level = level;
    }

    public UnitDefinitionSO GetUnitDefinition(IUnitSOProvider unitSOProvider)
    {
      if (unitSOProvider == null)
      {
        Debug.LogError("unitSOProvider is null");
        return null;
      }

      if (!unitSOProvider.ContainsKey(Key, TeamId))
      {
        Debug.LogError($"unitSOProvider.ContainsKey is false for {Key} (teamId={TeamId})");
        return null;
      }

      return unitSOProvider.GetUnitDefinition(Key, TeamId);
    }

    public CombatUnit CreateCombatUnit(ICombatSettings combatSettings, IUnitSOProvider unitSOProvider, TimeManager timeManager)
    {
      return new CombatUnit(this, combatSettings, unitSOProvider, timeManager);
    }
  }
}
