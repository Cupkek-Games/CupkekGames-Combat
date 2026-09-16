using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
  [Serializable]
  public class CombatTargetSelectionPrimaryTarget : CombatTargetSelection
  {
    public override List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster,
      CombatUnit primaryTarget, bool debug)
    {
      List<CombatUnit> result = new List<CombatUnit>();

      if (caster == null)
      {
        if (debug) Debug.Log("caster null");
        return result;
      }

      if (caster.View == null)
      {
        if (debug) Debug.Log("caster.View null");
        return result;
      }

      if (primaryTarget == null)
      {
        if (debug) Debug.Log("primaryTarget null");
        return result;
      }

      if (primaryTarget.View == null)
      {
        if (debug) Debug.Log("primaryTarget.View null");
        return result;
      }

      if (IsInRange(caster.View.transform, primaryTarget.View.transform, 0.1f, debug))
      {
        result.Add(primaryTarget);
      }

      return result;
    }
  }
}
