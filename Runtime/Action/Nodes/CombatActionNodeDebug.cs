using UnityEngine;
using System.Threading;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
  public class CombatActionNodeDebug : CombatActionNodeWithTarget
  {
    [SerializeField] private int _baseValue;
    [SerializeField] private float _attrMultiplier;
    [SerializeField] private DamageTypeDefinitionSO _damageType;

    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);

      // Debugging outputs
      Debug.Log("Debugging OnUpdate Method:");
      Debug.Log("actionSO: " + ctx.ActionSO);
      Debug.Log("combatSettings: " + ctx.CombatSettings);
      Debug.Log("manager: " + ctx.CombatManager);
      Debug.Log("caster: " + ctx.Caster);
      Debug.Log("primaryTarget: " + ctx.PrimaryTarget);
      Debug.Log("skillLevel: " + ctx.SkillLevel);
      Debug.Log("targetList Count: " + (ctx.TargetList != null ? ctx.TargetList.Count.ToString() : "null"));

      // Optionally log each CombatUnit in the target list if it's not null
      if (ctx.TargetList != null)
      {
        for (int i = 0; i < ctx.TargetList.Count; i++)
        {
          Debug.Log($"targetList[{i}]: {ctx.TargetList[i]}");
        }
      }

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {

    }

  }
}