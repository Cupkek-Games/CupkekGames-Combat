using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.BehaviourTrees;
using Cysharp.Threading.Tasks;
using System.Threading;
using PrimeTween;

namespace CupkekGames.Combat
{
    public class CombatActionNodeRise : CombatActionNodeWithTarget
    {
        [SerializeField] private float _durationRise;
        [SerializeField] private Ease _easeRise = Ease.OutSine;
        [SerializeField] private float _durationDelay;
        [SerializeField] private float _durationFall;
        [SerializeField] private Ease _easeFall = Ease.InSine;
        [SerializeField] private float _riseHeight = 3f;
        [SerializeField] private bool _targetCustomTimeScale = false;

        protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
        {
            var ctx = CombatActionContext.From(Blackboard);
            if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

            CancellationToken cancellationToken = ctx.LinkedCancelToken;

            foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
            {
                RiseAndFallAsync(target, cancellationToken).Forget();
            }

            return BTNodeRuntimeState.Success;
        }

        private async UniTaskVoid RiseAndFallAsync(CombatUnit unit, CancellationToken ct)
        {
            Transform transform = unit.CombatUnitGameObject.VisualsRoot.transform;
            Vector3 startPosition = transform.position;
            Vector3 peakPosition = startPosition + Vector3.up * _riseHeight;

            // Rise
            Tween tween = Tween.Position(transform, peakPosition, _durationRise, ease: _easeRise);

            if (_targetCustomTimeScale)
            {
                unit.TimeBundle.TimeScaleTween.Add(tween);
            }

            await tween.ToYieldInstruction().ToUniTask(cancellationToken: ct);

            // Delay
            if (_durationDelay > 0)
            {
                await unit.TimeBundle.TimeContext.DelayAsync(_durationDelay, cancellationToken: ct);
            }

            // Fall
            tween = Tween.Position(transform, startPosition, _durationFall, ease: _easeFall);

            if (_targetCustomTimeScale)
            {
                unit.TimeBundle.TimeScaleTween.Add(tween);
            }

            await tween.ToYieldInstruction().ToUniTask(cancellationToken: ct);

            // Ensure final position is exact
            transform.position = startPosition;
        }

        protected override void OnReset()
        {
        }
    }
}