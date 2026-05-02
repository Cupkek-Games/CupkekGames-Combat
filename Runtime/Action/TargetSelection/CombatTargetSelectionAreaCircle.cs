using System;
using System.Collections.Generic;
using System.Threading;
using CupkekGames.ShapeDrawing;
using CupkekGames.TimeSystem;
using UnityEngine;

namespace CupkekGames.Combat
{
  [Serializable]
  public class CombatTargetSelectionAreaCircle : CombatTargetSelection
  {
    public float Radius;
    public override List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget, bool debug)
    {
      Transform center = caster.CombatUnitGameObject.transform;

      List<Collider> colliders = TargetAreaColliderExtensions.FindCollidersInSphere(center.position, Radius);

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
