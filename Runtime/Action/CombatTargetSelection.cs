using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CupkekGames.ShapeDrawing;
using CupkekGames.TimeSystem;
using UnityEngine;

namespace CupkekGames.Combat
{
  [Serializable]
  public class CombatTargetSelection
  {
    public float Range;
    public bool Self;
    public bool Ally;
    public bool Enemy;
    public static bool IsInRange(Transform caster, Transform target, float range, float tolerance, bool debug)
    {
      float rangeSqr = (range + tolerance) * (range + tolerance);
      float distanceSqr = (caster.position - target.position).sqrMagnitude;
      bool isInRange = distanceSqr <= rangeSqr;
      if (debug)
      {
        Debug.Log($"IsInRange: {isInRange} {caster.position} {target.position} {rangeSqr} {distanceSqr} (tolerance: {tolerance})");
      }
      return isInRange;
    }
    public bool IsInRange(Transform caster, Transform target, float tolerance, bool debug)
    {
      return IsInRange(caster, target, Range, tolerance, debug);
    }

    public virtual List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget, bool debug)
    {
      List<CombatUnit> result = new List<CombatUnit>(combatUnitManager.CombatUnitsAlly);
      result.AddRange(combatUnitManager.CombatUnitsEnemy);

      return FilterTargets(caster, result);
    }

    public List<CombatUnit> FilterTargets(CombatUnit caster, List<CombatUnit> targets)
    {
      List<CombatUnit> result = new();

      foreach (CombatUnit target in targets)
      {
        if (CanSelect(caster, target, Self, Ally, Enemy))
        {
          result.Add(target);
        }
      }

      return result;
    }

    public static bool CanSelect(CombatUnit caster, CombatUnit target, bool self, bool ally, bool enemy)
    {
      if (caster == null || target == null)
      {
        return false;
      }

      if (caster.ID == target.ID)
      {
        return self;
      }

      if (caster.IsAllyOf(target))
      {
        return ally;
      }
      else
      {
        return enemy;
      }
    }

    public virtual Indicator ShowIndicator(IIndicatorPool indicatorPool, Vector3 position, Quaternion rotation, Color? color = null)
    {
      return null;
    }
    public virtual Indicator ShowIndicator(
      IIndicatorPool indicatorPool,
      Vector3 position,
      Quaternion rotation,
      float duration,
      CancellationToken? ct,
      TimeBundle timeBundle,
      Color? color = null)
    {
      return null;
    }
    public List<CombatUnit> FromColliders(CombatUnit caster, List<Collider> colliders)
    {
      List<CombatUnit> result = new();

      IEnumerable<Collider> ordered = colliders
        .Where(collider =>
          {
            if (collider == null)
            {
              return false;
            }

            CombatUnitView combatUnitGameObject = collider.gameObject.GetComponent<CombatUnitView>();

            return combatUnitGameObject != null;
          }
        );
      // .OrderBy(collider => (collider.transform.position - center).sqrMagnitude);

      foreach (Collider collider in ordered)
      {
        result.Add(collider.gameObject.GetComponent<CombatUnitView>().CombatUnit);
      }

      return FilterTargets(caster, result);
    }
  }
}
