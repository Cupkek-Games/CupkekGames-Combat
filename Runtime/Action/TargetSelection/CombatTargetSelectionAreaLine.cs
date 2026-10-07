using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
    /// <summary>A strip from the caster, the way to its target and on past it to its length.</summary>
    [Serializable]
    public class CombatTargetSelectionAreaLine : CombatTargetSelection
    {
        public float Length = 5f;
        public float Width = 1f;

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

            area = CombatArea.Line(caster.View.transform.position, primaryTarget.View.transform.position, Length, Width);
            return true;
        }
    }
}
