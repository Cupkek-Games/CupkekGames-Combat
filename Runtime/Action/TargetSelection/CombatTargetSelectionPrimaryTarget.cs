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

      ICombatSpace space = combatUnitManager.Space
        ?? throw new InvalidOperationException("[CombatTargetSelectionPrimaryTarget] the fight has no space (ICombatUnitManager.Space is null).");
      bool inRange = space.InRange(caster, primaryTarget, Range);
      if (debug) Debug.Log($"InRange: {inRange} (range {Range})");
      if (inRange)
      {
        result.Add(primaryTarget);
      }

      return result;
    }
  }
}
