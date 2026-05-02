using System.Collections.Generic;
using CupkekGames.BehaviourTrees;

namespace CupkekGames.Combat
{
  public class CombatActionNodeTargetUpdate : BTNodeDecorator
  {
    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {

      List<CombatUnit> targetList = UpdateTargetList(ref Blackboard);

      if (targetList.Count == 0) return BTNodeRuntimeState.Fail;

      return Child.UpdateNode(ref Blackboard, deltaTime);
    }

    protected override void OnReset()
    {

    }

    public static List<CombatUnit> UpdateTargetList(ref Dictionary<string, object> Blackboard)
    {
      var ctx = CombatActionContext.From(Blackboard);

      List<CombatUnit> targetList = ctx.ActionSO.GetTargets(ctx.CombatManager.UnitManager, ctx.Caster, ctx.PrimaryTarget, false);
      ctx.TargetList = targetList;

      return targetList;
    }
  }
}