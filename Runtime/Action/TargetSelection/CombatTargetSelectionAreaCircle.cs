using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
  public enum CombatAreaCenter
  {
    /// <summary>Around the caster.</summary>
    Caster,
    /// <summary>Around where the carrying projectile landed (only inside a projectile's payload).</summary>
    Impact,
  }

  [Serializable]
  public class CombatTargetSelectionAreaCircle : CombatTargetSelection
  {
    public float Radius;
    public CombatAreaCenter Center = CombatAreaCenter.Caster;

    public override bool HasArea => true;

    public override List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget, bool debug)
      => GetTargets(combatUnitManager, caster, primaryTarget, null, debug);

    public override List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster,
      CombatUnit primaryTarget, Vector3? impact, bool debug)
    {
      TryGetArea(combatUnitManager, caster, primaryTarget, impact, out CombatArea area);
      return FromArea(combatUnitManager, caster, area);
    }

    public override bool TryGetArea(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget,
      Vector3? impact, out CombatArea area)
    {
      Vector3 center;
      if (Center == CombatAreaCenter.Impact)
      {
        center = impact ?? throw new InvalidOperationException(
          "[CombatTargetSelectionAreaCircle] Center is Impact, but no projectile landed: an impact circle belongs inside a projectile's payload.");
      }
      else
      {
        center = caster.View.transform.position;
      }

      area = CombatArea.Circle(center, Radius);
      return true;
    }
  }
}
