using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
  /// <summary>A slice of a circle around the caster, centred on the way to its target.</summary>
  [Serializable]
  public class CombatTargetSelectionAreaArc : CombatTargetSelection
  {
    public float Radius;
    public float Angle;

    public override bool HasArea => true;

    public override List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget, bool debug)
      => TryGetArea(combatUnitManager, caster, primaryTarget, null, out CombatArea area)
        ? FromArea(combatUnitManager, caster, area)
        : new List<CombatUnit>();

    public override bool TryGetArea(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget,
      Vector3? impact, out CombatArea area)
    {
      area = default;
      if (primaryTarget?.View == null) return false;

      area = CombatArea.Arc(caster.View.transform.position, primaryTarget.View.transform.position, Radius, Angle);
      return true;
    }
  }
}
