using UnityEngine;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using Cysharp.Threading.Tasks;
using System.Threading;
using PrimeTween;

namespace CupkekGames.Combat
{
    public class CombatActionNodeDash : CombatActionNodeWithTarget
    {
        [SerializeField] private Vector3[] _dashDistance;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private Ease _ease = Ease.OutSine;
        [SerializeField] private int _avoidancePriority = 51;

        protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
        {
            var ctx = CombatActionContext.From(frame);
            if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

            CancellationToken cancellationToken = ctx.LinkedCancelToken;

            Vector3 dash = GetDashVector(ctx.SkillLevel);

            foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
            {
                target.CombatUnitGameObject.transform.GetPositionAndRotation(out Vector3 startPosition,
                    out Quaternion objectWorldRotation);

                Vector3 dashWorldSpaceDisplacement = objectWorldRotation * dash;

                Vector3 endPosition = startPosition + dashWorldSpaceDisplacement;

                target.CombatUnitGameObject.CombatUnitAI.NavMeshAgentController
                    .MoveAgentManually(endPosition, _duration, _avoidancePriority, cancellationToken, _ease).Forget();
            }

            return BTNodeRuntimeState.Success;
        }

        private Vector3 GetDashVector(int skillLevel)
        {
            return CombatArrayUtils.GetArrayElementOrLast(_dashDistance, skillLevel - 1);
        }

        protected override void OnReset()
        {
        }
    }
}