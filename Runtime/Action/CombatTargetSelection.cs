using System;
using System.Collections.Generic;
using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.Combat
{
  /// <summary>
  /// Polymorphic target-selection slot on a <see cref="CombatActionSO"/>. Implements <see cref="IFeature"/>
  /// so Luna's generic <c>FeatureDrawer</c> handles the inspector dropdown — no custom drawer needed.
  /// </summary>
  [Serializable]
  public class CombatTargetSelection : IFeature
  {
    public float Range;
    public bool Self;
    public bool Ally;
    public bool Enemy;

    /// <summary>Whether this selection may ever pick anyone: false when it takes no side (an action authored as an empty placeholder).</summary>
    public virtual bool CanSelectAnyone => Self || Ally || Enemy;

    /// <summary>Whether this selection covers an area of the field (so a warning can show it), not its unit's own target or the whole field.</summary>
    public virtual bool HasArea => false;

    /// <summary>
    /// The area this selection covers for a cast now, aimed where its caster aims; false
    /// for a selection without one (<see cref="HasArea"/>) and for an aimed area with
    /// nobody to aim at. <paramref name="impact"/> is where a carrying projectile landed.
    /// </summary>
    public virtual bool TryGetArea(ICombatUnitManager combatUnitManager, CombatUnit caster, CombatUnit primaryTarget,
      Vector3? impact, out CombatArea area)
    {
      area = default;
      return false;
    }

    /// <summary>The units standing in <paramref name="area"/> now (an area locked when a cast started), filtered to the ones this selection may pick.</summary>
    public List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster, in CombatArea area)
      => FromArea(combatUnitManager, caster, area);

    /// <summary>The units <paramref name="area"/> covers in the fight's space, filtered to the ones this selection may pick.</summary>
    protected List<CombatUnit> FromArea(ICombatUnitManager combatUnitManager, CombatUnit caster, in CombatArea area)
    {
      ICombatSpace space = combatUnitManager.Space
        ?? throw new InvalidOperationException("[CombatTargetSelection] the fight has no space (ICombatUnitManager.Space is null).");
      List<CombatUnit> covered = new List<CombatUnit>();
      space.Collect(area, covered);
      return FilterTargets(caster, covered);
    }

    /// <summary>
    /// Targets for a payload that may know where its projectile landed
    /// (<paramref name="impact"/>, null otherwise). Selections that do not care
    /// about an impact point answer as <see cref="GetTargets(ICombatUnitManager, CombatUnit, CombatUnit, bool)"/>.
    /// </summary>
    public virtual List<CombatUnit> GetTargets(ICombatUnitManager combatUnitManager, CombatUnit caster,
      CombatUnit primaryTarget, Vector3? impact, bool debug)
      => GetTargets(combatUnitManager, caster, primaryTarget, debug);

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

    public virtual IFeature CloneFeature()
    {
      // Target selection has no per-instance mutable state at runtime — sharing the authored instance is safe.
      return this;
    }
  }
}
