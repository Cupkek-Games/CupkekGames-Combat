using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;

namespace CupkekGames.Combat
{
    public class CombatActionDelayNode : DelayNode
    {
        // [SerializeField] private bool _failIfNoTargets = true;
        protected override bool OnDelayComplete(GraphFrame frame)
        {
            // List<CombatUnit> targets = CombatActionNodeTargetUpdate.UpdateTargetList(frame);
            CombatActionNodeTargetUpdate.UpdateTargetList(frame);

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
