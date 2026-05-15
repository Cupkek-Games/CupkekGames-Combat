using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;

namespace CupkekGames.Combat
{
  public class CombatActionNodeSpawnUnit : BTNodeAction
  {
    [SerializeField] CombatUnitReference _combatWaveEnemy;
    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);

      ctx.CombatManager.UnitManager.SpawnEnemy(_combatWaveEnemy);

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {

    }
  }
}