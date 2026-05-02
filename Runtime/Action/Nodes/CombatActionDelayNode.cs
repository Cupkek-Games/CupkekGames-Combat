using System.Collections.Generic;
using CupkekGames.BehaviourTrees;
using UnityEngine;

namespace CupkekGames.Combat
{
    public class CombatActionDelayNode : DelayNode
    {
        // [SerializeField] private bool _failIfNoTargets = true;
        protected override bool OnDelayComplete(ref Dictionary<string, object> Blackboard)
        {
            // List<CombatUnit> targets = CombatActionNodeTargetUpdate.UpdateTargetList(ref Blackboard);
            CombatActionNodeTargetUpdate.UpdateTargetList(ref Blackboard);

            // if (_failIfNoTargets)
            // {
            //     if (targets.Count == 0)
            //     {
            //         return false;
            //     }
            // }

            return true;
        }
    }
}
