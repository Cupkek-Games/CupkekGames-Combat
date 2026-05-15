using System.Collections.Generic;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Decorator that overrides the active target list for its sub-tree
    /// only. Pushes a child <see cref="GraphFrame"/> on first invocation
    /// and writes the new list as a local on that frame — sibling
    /// branches in a parallel composite never see the scoped change,
    /// solving the "two target-selections clobber each other" issue that
    /// the flat-blackboard design had pre-migration.
    /// </summary>
    public class CombatActionNodeTargetSelection : BTNodeDecorator
    {
        [SerializeReference] private CombatTargetSelection _targetSelection;
        [SerializeField] private bool _clear = true;

        [System.NonSerialized] GraphFrame _scopedFrame;
        [System.NonSerialized] CombatActionContext _scopedContext;

        public CombatActionNodeTargetSelection(CombatTargetSelection targetSelection, bool clear)
        {
            _targetSelection = targetSelection;
            _clear = clear;
        }

        protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
        {
            // First call: push a child frame so this branch's TargetList
            // doesn't leak to siblings. Subsequent calls (child returned
            // Running) reuse the cached scope.
            if (_scopedFrame == null)
            {
                _scopedFrame = frame.Push();
                _scopedContext = new CombatActionContext(_scopedFrame);
                _scopedFrame.SetLocal("Context", _scopedContext);

                var parentCtx = CombatActionContext.From(frame);
                var parentList = parentCtx?.TargetList;
                var copy = parentList != null
                    ? new List<CombatUnit>(parentList)
                    : new List<CombatUnit>();
                if (_clear) copy.Clear();

                copy.AddRange(_targetSelection.GetTargets(
                    parentCtx.CombatManager.UnitManager,
                    parentCtx.Caster,
                    parentCtx.PrimaryTarget,
                    false));

                _scopedContext.TargetList = copy;

                if (copy.Count == 0)
                {
                    ClearScope();
                    return BTNodeRuntimeState.Fail;
                }
            }

            var child = GetChild();
            if (child == null)
            {
                ClearScope();
                return BTNodeRuntimeState.Success;
            }

            var state = child.UpdateNode(_scopedFrame, deltaTime);
            if (state == BTNodeRuntimeState.Success || state == BTNodeRuntimeState.Fail)
                ClearScope();
            return state;
        }

        protected override void OnReset()
        {
            ClearScope();
        }

        void ClearScope()
        {
            _scopedFrame = null;
            _scopedContext = null;
        }
    }
}
