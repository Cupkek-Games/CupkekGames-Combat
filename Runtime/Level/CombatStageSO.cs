using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
  [CreateAssetMenu(fileName = "CombatStage", menuName = "CupkekGames/Combat/Level/CombatStage")]
  public class CombatStageSO : ScriptableObject
  {
    public int StageIndex;
    public List<CombatWaveSO> CombatWaves;

    public CombatStage GetCombatStage()
    {
      CombatStage campaignStage = new CombatStage(StageIndex);
      foreach (CombatWaveSO combatWaveSO in CombatWaves)
      {
        campaignStage.CombatWaves.Add(combatWaveSO.GetCombatWave());
      }
      return campaignStage;
    }
  }
}