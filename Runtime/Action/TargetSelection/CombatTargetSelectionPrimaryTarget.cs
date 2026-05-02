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

      if (caster.CombatUnitGameObject == null)
      {
        if (debug) Debug.Log("caster.CombatUnitGameObject null");
        return result;
      }

      if (primaryTarget == null)
      {
        if (debug) Debug.Log("primaryTarget null");
        return result;
      }

      if (primaryTarget.CombatUnitGameObject == null)
      {
        if (debug) Debug.Log("primaryTarget.CombatUnitGameObject null");
        return result;
      }

      if (IsInRange(caster.CombatUnitGameObject.transform, primaryTarget.CombatUnitGameObject.transform, 0.1f, debug))
      {
        result.Add(primaryTarget);
      }

      return result;
    }
  }
}
