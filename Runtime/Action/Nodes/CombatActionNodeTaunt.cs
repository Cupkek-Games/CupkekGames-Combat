using System.Collections.Generic;
using UnityEngine;
using CupkekGames.BehaviourTrees;

namespace CupkekGames.Combat
{
  public class CombatActionNodeTaunt : CombatActionNodeWithTarget
  {
    [SerializeField] private int[] _threat;

    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {
      var ctx = CombatActionContext.From(Blackboard);

      int threat = GetThreat(ctx.SkillLevel);

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        // Add threat to target for caster
        target.AddThreat(ctx.Caster, threat);
      }

      return BTNodeRuntimeState.Success;
    }
    private int GetThreat(int skillLevel)
    {
      return CombatArrayUtils.GetArrayElementOrLast(_threat, skillLevel - 1);
    }

    protected override void OnReset()
    {

    }
  }
}