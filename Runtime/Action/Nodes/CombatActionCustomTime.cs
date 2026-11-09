using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using CupkekGames.TimeSystem;

namespace CupkekGames.Combat
{
  /// <summary>
  /// Scales its targets' time by a factor on a layer this node owns, until the action runs again
  /// or is let go. Other holders' layers (the ultimate freeze) stay in force.
  /// </summary>
  public class CombatActionCustomTime : CombatActionNodeWithTarget
  {
    [SerializeField] private float _timeScale = 1f;

    [NonSerialized] private readonly List<IDisposable> _layers = new();

    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);

      ReleaseLayers();
      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        TimeContext time = target.TimeBundle.TimeContext;
        if (!time.HasLayer(this)) _layers.Add(time.Scale(this, _timeScale));
      }

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {
      ReleaseLayers();
    }

    public override void Dispose()
    {
      ReleaseLayers();
      base.Dispose();
    }

    private void ReleaseLayers()
    {
      for (int i = 0; i < _layers.Count; i++) _layers[i].Dispose();
      _layers.Clear();
    }
  }
}
