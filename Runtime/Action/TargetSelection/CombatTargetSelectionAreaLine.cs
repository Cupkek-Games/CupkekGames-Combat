using System;
using System.Collections.Generic;
using System.Threading;
using CupkekGames.ShapeDrawing;
using CupkekGames.TimeSystem;
using UnityEngine;

namespace CupkekGames.Combat
{
    [Serializable]
    public class CombatTargetSelectionAreaLine : CombatTargetSelection
    {
        public float Length = 5f;
        public float Width = 1f;

        public override List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget, bool debug)
        {
            Transform center = caster.View.transform;
            return FromArea(combatUnitManager, caster, CombatArea.Line(center.position, center.forward, Length, Width));
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
            Indicator indicator = indicatorPool.ShowLineRegion(
                position, rotation, space.ToWorld(Length), space.ToWorld(Width), color);

            if (duration > 0)
            {
                indicator.AnimateFill(duration, ct.Value, timeBundle).Forget();
            }

            return indicator;
        }
    }
}