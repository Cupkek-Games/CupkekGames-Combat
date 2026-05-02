using System;
using System.Collections.Generic;
using System.Linq;
using CupkekGames.Units;
using UnityEngine;

namespace CupkekGames.Combat
{
  [Serializable]
  public class CombatStage
  {
    [SerializeField] private int _stage;
    [SerializeField] private List<CombatWave> _combatWaves;
    public int Stage => _stage;
    public List<CombatWave> CombatWaves => _combatWaves;

    public CombatStage()
    {
      _stage = 0;
      _combatWaves = new List<CombatWave>();
    }

    public CombatStage(int stage, List<CombatWave> combatWaves = null)
    {
      _stage = stage;

      if (combatWaves == null)
      {
        _combatWaves = new List<CombatWave>();
      }
      else
      {
        _combatWaves = combatWaves;
      }
    }

    public List<CombatUnitReference> GetAllUnits()
    {
      List<CombatUnitReference> result = new();

      foreach (CombatWave wave in CombatWaves)
      {
        foreach (System.Collections.Generic.KeyValuePair<Vector2Int, CombatUnitReference> kvp in wave.Wave)
        {
          result.Add(kvp.Value);
        }
      }

      return result;
    }



    public void GenerateCombatWaves(IUnitSOProvider unitSOProvider, Vector2Int gridSize,
      string[] enemyPool, int minLevel, int maxLevel, int wavesMin, int wavesMax,
      int enemyPerWaveMin, int enemyPerWaveMax)
    {
      CombatWaves.Clear();

      int gridCapacity = gridSize.x * gridSize.y;
      if (enemyPerWaveMax > gridCapacity)
      {
        enemyPerWaveMax = gridCapacity;
      }

      int numWaves = UnityEngine.Random.Range(wavesMin, wavesMax);
      for (int i = 0; i < numWaves; i++)
      {
        int enemyCount = UnityEngine.Random.Range(enemyPerWaveMin, enemyPerWaveMax);

        CombatWave combatWave = new CombatWave();
        for (int j = 0; j < enemyCount; j++)
        {
          int enemyIndex = UnityEngine.Random.Range(0, enemyPool.Length);
          string enemyKey = enemyPool[enemyIndex];

          UnitDefinitionSO enemySO = unitSOProvider.GetUnitDefinition(enemyKey, 1);

          int enemyLevel = UnityEngine.Random.Range(minLevel, maxLevel);
          CombatUnitReference combatUnitRef = new CombatUnitReference(1, enemySO.name, enemyLevel);

          List<Vector2Int> availablePositions = new List<Vector2Int>();
          for (int x = 0; x < gridSize.x; x++)
          {
            for (int y = 0; y < gridSize.y; y++)
            {
              Vector2Int position = new Vector2Int(x, y);
              if (!combatWave.Wave.ContainsKey(position))
              {
                availablePositions.Add(position);
              }
            }
          }

          Vector2Int randomPosition = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];
          combatWave.Wave.Add(randomPosition, combatUnitRef);
        }

        CombatWaves.Add(combatWave);
      }
    }
  }
}