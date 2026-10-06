using System;
using System.Collections.Generic;
using System.Threading;
using CupkekGames.ShapeDrawing;
using CupkekGames.TimeSystem;
using UnityEngine;

namespace CupkekGames.Combat
{
  [Serializable]
  public class CombatTargetSelectionAreaArc : CombatTargetSelection
  {
    public float Radius;
    public float Angle;
    public override List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget, bool debug)
    {
      Transform center = caster.View.transform;

      return FromArea(combatUnitManager, caster, CombatArea.Arc(center.position, center.forward, Radius, Angle));
    }

    public override Indicator ShowIndicator(
      ICombatSpace space,
      IIndicatorPool indicatorPool,
      Vector3 position,
      Quaternion rotation,
      float duration,
      CancellationToken? ct,
      TimeBundle timeBundle,
      Color? color = null)
    {
      Indicator indicator = indicatorPool.ShowArcRegion(position, rotation, space.ToWorld(Radius), Angle, color);

      if (duration > 0)
      {
        indicator.AnimateFill(duration, ct.Value, timeBundle).Forget();
      }

      return indicator;
    }
  }
}
