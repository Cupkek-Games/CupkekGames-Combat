using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
  [Serializable]
  public class CombatStage
  {
    [SerializeField] private int _stage;
    [SerializeField] private List<CombatWave> _combatWaves;
    // Private setter is load-bearing: this type is in the JSON save registry
    // and the pipeline's PrivateSetterContractResolver can only deserialize a
    // property that HAS a setter. A get-only expression body serialized out
    // but silently loaded back as 0.
    public int Stage { get => _stage; private set => _stage = value; }
    public List<CombatWave> CombatWaves { get => _combatWaves; private set => _combatWaves = value; }

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

    public CombatStage(CombatStage other)
    {
      _combatWaves = new List<CombatWave>();
      if (other == null)
        return;
      _stage = other._stage;
      if (other._combatWaves == null)
        return;
      foreach (CombatWave wave in other._combatWaves)
        _combatWaves.Add(wave != null ? new CombatWave(wave) : null);
    }

    public List<CombatUnitReference> GetAllUnits()
    {
      List<CombatUnitReference> result = new();

      foreach (CombatWave wave in CombatWaves)
      {
        foreach (CombatWave.Entry entry in wave.Entries)
        {
          result.Add(entry.Unit);
        }
      }

      return result;
    }
  }
}