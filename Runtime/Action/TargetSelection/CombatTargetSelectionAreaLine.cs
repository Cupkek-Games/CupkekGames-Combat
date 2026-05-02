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
            Transform center = caster.CombatUnitGameObject.transform;
            Vector3 direction = center.forward;
            Vector3 endPoint = center.position + direction * Length;

            // Find colliders in a box along the line
            List<Collider> colliders = TargetAreaColliderExtensions.FindCollidersInLine(
                center.position,
                endPoint,
                Width);

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
            Indicator indicator = indicatorPool.ShowLineRegion(
                position, rotation, Length, Width, color);

            if (duration > 0)
            {
                indicator.AnimateFill(duration, ct.Value, timeBundle).Forget();
            }

            return indicator;
        }
    }
}