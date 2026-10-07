using UnityEngine;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using System.Threading;
using PrimeTween;

namespace CupkekGames.Combat
{
    public class CombatActionNodeDash : CombatActionNodeWithTarget
    {
        [SerializeField] private Vector3[] _dashDistance;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private Ease _ease = Ease.OutSine;
        [SerializeField] private CombatDashMode _mode = CombatDashMode.Push;

        protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
        {
            var ctx = CombatActionContext.From(frame);
            if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

            CancellationToken cancellationToken = ctx.LinkedCancelToken;

            // The dash is in combat units, turned the way each target faces; its space moves it.
            Vector3 dash = GetDashVector(ctx.SkillLevel);

            foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
            {
                Vector3 offset = target.View.transform.rotation * dash;
                target.View.CombatUnitAI.Mover.Dash(offset, _duration, _ease, _mode, cancellationToken);
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