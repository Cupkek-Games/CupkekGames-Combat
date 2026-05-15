using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.Luna;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;

namespace CupkekGames.Combat
{
  public class CombatActionNodeToggleAI : CombatActionNodeWithTarget
  {
    [SerializeField] private bool _toggle = false;
    [SerializeField] private bool _disable = true;
    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        bool running = target.IsAIRunning;
        if (_toggle)
        {
          if (running)
          {
            target.StopAI(false, true);
          }
          else
          {
            target.StartAI();
          }
        }
        else
        {
          if (_disable && running)
          {
            target.StopAI(false, true);
          }
          else if (!_disable && !running)
          {
            target.StartAI();
          }
        }
      }

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {

    }
  }
}