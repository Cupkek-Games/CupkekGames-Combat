using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.BehaviourTrees;

namespace CupkekGames.Combat
{
  public class CombatActionNodeSpawnUnit : BTNodeAction
  {
    [SerializeField] CombatUnitReference _combatWaveEnemy;
    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {
      var ctx = CombatActionContext.From(Blackboard);

      ctx.CombatManager.UnitManager.SpawnEnemy(_combatWaveEnemy);

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {

    }
  }
}