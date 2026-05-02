using System.Collections.Generic;
using UnityEngine;
using CupkekGames.KeyValueDatabases;

namespace CupkekGames.Combat
{
  [CreateAssetMenu(fileName = "CombatWaveEnemySO", menuName = "CupkekGames/Combat/Level/EnemyWave")]
  public class CombatWaveSO : ScriptableObject
  {
    [SerializeField] public KeyValueDatabase<Vector2Int, CombatUnitReference> Grid = new KeyValueDatabase<Vector2Int, CombatUnitReference>();

    public CombatWave GetCombatWave()
    {
      CombatWave combatWave = new CombatWave();

      foreach (Vector2Int key in Grid.Keys)
      {
        CombatUnitReference value = Grid.GetValue(key);
        combatWave.Wave.Add(key, value);
      }

      return combatWave;
    }
  }
}