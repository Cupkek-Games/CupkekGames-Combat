using System.Collections.Generic;
using UnityEngine;
using CupkekGames.Animations;
using CupkekGames.BehaviourTrees;
using System.Threading;

namespace CupkekGames.Combat
{
  public class CombatActionNodeAnimation : CombatActionNodeWithTarget
  {
    [SerializeField] private AnimationClip _animationClip;
    [SerializeField] private float _fadeDuration = 0.25f;

    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {
      var ctx = CombatActionContext.From(Blackboard);
      if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        CancellationToken cancellationToken = ctx.CreateTargetLinkedToken(target.DeathToken.Token, target.InterruptToken.Token);

        IAnimationStateController animController = target.CombatUnitGameObject.AnimationController;
        animController?.PlayClipWithReturnToIdle(_animationClip, _fadeDuration, cancellationToken);
      }

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {

    }
  }
}