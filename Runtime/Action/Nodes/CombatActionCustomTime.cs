using UnityEngine;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;

namespace CupkekGames.Combat
{
  public class CombatActionCustomTime : CombatActionNodeWithTarget
  {
    [SerializeField] private float _timeScale = 1f;

    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        target.TimeBundle.TimeContext.TimeScale = _timeScale;
      }

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {

    }
  }
}