using System.Collections.Generic;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Decorator that recomputes the target list at the current scope
    /// (whichever frame it's invoked with). Does NOT push a new scope —
    /// if you want a fresh target list visible only to a sub-branch use
    /// <see cref="CombatActionNodeTargetSelection"/> instead.
    /// </summary>
    public class CombatActionNodeTargetUpdate : BTNodeDecorator
    {
        protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
        {
            var targetList = UpdateTargetList(frame);
            if (targetList == null || targetList.Count == 0)
                return BTNodeRuntimeState.Fail;

            var child = GetChild();
            if (child == null) return BTNodeRuntimeState.Success;
            return child.UpdateNode(frame, deltaTime);
        }

        protected override void OnReset()
        {
        }

        /// <summary>
        /// Static so other nodes (delay-then-update, indicator-shown-then-update)
        /// can recompute the target list in place. Writes to the
        /// <see cref="CombatActionContext"/>'s bound frame — which means the
        /// scope is whatever frame the caller currently holds (root for
        /// non-scoping callers, the local frame for descendants of a
        /// <see cref="CombatActionNodeTargetSelection"/>).
        /// </summary>
        public static List<CombatUnit> UpdateTargetList(GraphFrame frame)
        {
            var ctx = CombatActionContext.From(frame);
            if (ctx == null) return null;

            var targets = ctx.ActionSO.GetTargets(
                ctx.CombatManager.UnitManager,
                ctx.Caster,
                ctx.PrimaryTarget,
                false);
            ctx.TargetList = targets;
            return targets;
        }
    }
}
