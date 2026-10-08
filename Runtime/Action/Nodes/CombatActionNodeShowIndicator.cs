using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;


namespace CupkekGames.Combat
{
  /// <summary>
  /// The warning before an area hits: when the cast starts it locks the action's area (where
  /// the caster aims now) into the run, and the fight's space draws exactly those cells. The
  /// fill rises over <c>_durationFill</c>; then the targets are taken from the locked area
  /// (whoever stands on the warned cells) and the child runs.
  /// </summary>
  public class CombatActionNodeShowIndicator : BTNodeDecorator
  {
    [SerializeField] private float _durationFill;
    [SerializeField] private float _durationDisable;

    [SerializeField] private Color _indicatorColor = new Color(0, 1, 1, 0.3f);

    // State
    private CombatAreaMark _mark = null;
    private float _passed = -1f;
    private CancellationToken? _cancellationToken;
    private bool _isFilled = false;

    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);

      if (!_cancellationToken.HasValue)
      {
        if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

        _cancellationToken = ctx.LinkedCancelToken;
      }

      if (_cancellationToken.Value.IsCancellationRequested)
      {
        HideNow();
        return BTNodeRuntimeState.Fail;
      }

      CombatUnit caster = ctx.Caster;

      if (_mark == null)
      {
        CombatTargetSelection selection = ctx.ActionSO.TargetSelection;
        if (!selection.HasArea)
        {
          throw new InvalidOperationException($"[CombatActionNodeShowIndicator] '{ctx.ActionSO.name}' warns of an area, but its target selection ({selection.GetType().Name}) has none: give it an area selection.");
        }

        ICombatUnitManager units = ctx.CombatManager.UnitManager;
        if (!selection.TryGetArea(units, caster, ctx.PrimaryTarget, ctx.ImpactPosition, out CombatArea area))
        {
          ResetState();
          return BTNodeRuntimeState.Fail;
        }

        ctx.Area = area;
        _passed = deltaTime;
        _mark = units.Space.ShowArea(area, _indicatorColor);
        _mark.Fill(_durationFill, caster.TimeBundle, _cancellationToken.Value);
      }
      else
      {
        _passed += deltaTime;
      }

      if (_passed > _durationFill)
      {
        if (!_isFilled)
        {
          _isFilled = true;

          // The hit lands on the locked area's cells.
          CombatActionNodeTargetUpdate.UpdateTargetList(frame);

          _mark.HideAfter(_durationDisable, caster.TimeBundle).Forget();
        }

        BTNodeRuntimeState state = BTNodeRuntimeState.Success;

        var child = GetChild();
        if (child != null)
        {
          state = child.UpdateNode(frame, deltaTime);
        }

        if (state == BTNodeRuntimeState.Success || state == BTNodeRuntimeState.Fail)
        {
          ResetState();
        }

        return state;
      }

      return BTNodeRuntimeState.Running;
    }

    protected override void OnReset()
    {
      // A run cut short (a stun, the fight's end) takes its warning down.
      if (!_isFilled) HideNow();
      ResetState();
    }

    private void HideNow()
    {
      if (_mark != null) _mark.Hide();
    }

    private void ResetState()
    {
      _passed = -1f;
      _mark = null;
      _cancellationToken = null;
      _isFilled = false;
    }
  }
}
