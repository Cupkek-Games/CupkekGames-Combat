using System;
using System.Collections.Generic;
using System.Threading;
using CupkekGames.ShapeDrawing;
using CupkekGames.TimeSystem;
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

    public override List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget, bool debug)
      => GetTargets(combatUnitManager, caster, primaryTarget, null, debug);

    public override List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster,
      CombatUnit primaryTarget, Vector3? impact, bool debug)
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

      List<Collider> colliders = TargetAreaColliderExtensions.FindCollidersInSphere(center, Radius);

      return FromColliders(caster, colliders);
    }


    public override Indicator ShowIndicator(
      IIndicatorPool indicatorPool,
      Vector3 position,
      Quaternion rotation,
      float duration,
      CancellationToken? ct,
      TimeBundle timeBundle,
      Color? color = null)
    {
      Indicator indicator = indicatorPool.ShowCircleRegion(position, Radius, color);

      if (duration > 0)
      {
        indicator.AnimateFill(duration, ct.Value, timeBundle).Forget();
      }

      return indicator;
    }
  }
}
