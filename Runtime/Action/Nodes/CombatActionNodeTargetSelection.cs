using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.BehaviourTrees;

namespace CupkekGames.Combat
{
  public class CombatActionNodeTargetSelection : BTNodeDecorator
  {
    [SerializeReference] private CombatTargetSelection _targetSelection;
    [SerializeField] private bool _clear = true;
    public CombatActionNodeTargetSelection(CombatTargetSelection targetSelection, bool clear)
    {
      _targetSelection = targetSelection;
      _clear = clear;
    }

    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {
      var ctx = CombatActionContext.From(Blackboard);

      List<CombatUnit> copy = new List<CombatUnit>(ctx.TargetList);
      if (_clear)
      {
        copy.Clear();
      }

      copy.AddRange(_targetSelection.GetTargets(ctx.CombatManager.UnitManager, ctx.Caster, ctx.PrimaryTarget, false));

      ctx.TargetList = copy;

      if (copy.Count == 0) return BTNodeRuntimeState.Fail;

      return Child.UpdateNode(ref Blackboard, deltaTime);
    }

    protected override void OnReset()
    {

    }
  }
}