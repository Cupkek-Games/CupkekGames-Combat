using UnityEngine;
using CupkekGames.Animations;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using System.Threading;

namespace CupkekGames.Combat
{
  public class CombatActionNodeAnimation : CombatActionNodeWithTarget
  {
    [SerializeField] private AnimationClip _animationClip;
    [SerializeField] private float _fadeDuration = 0.25f;

    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);
      if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        // The clip plays out when the fight ends under it (the killing swing lands before the win pose, which
        // waits for it: CombatUnitView.OnWin) and when its unit falls (the death kind plays over it). Only a real
        // interrupt (a stun: CombatUnit.InterruptAI) cuts it to idle.
        CancellationToken cancellationToken = target.InterruptToken.Token;

        IAnimationStateController animController = target.View.AnimationController;
        animController?.PlayClipWithReturnToIdle(_animationClip, _fadeDuration, cancellationToken);
      }

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {

    }
  }
}