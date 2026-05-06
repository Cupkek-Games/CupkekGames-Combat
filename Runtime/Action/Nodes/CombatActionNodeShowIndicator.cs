using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.BehaviourTrees;
using CupkekGames.ShapeDrawing;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;

using CupkekGames.VFX;

namespace CupkekGames.Combat
{
  public class CombatActionNodeShowIndicator : BTNodeDecorator
  {
    [SerializeField] private float _durationFill;
    [SerializeField] private float _durationDisable;

    [SerializeField] private Color _indicatorColor = new Color(0, 1, 1, 0.3f);

    // State
    private Indicator _indicator = null;
    private float _passed = -1f;
    private CancellationToken? _cancellationToken;
    private bool _isFilled = false;

    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {
      var ctx = CombatActionContext.From(Blackboard);

      if (!_cancellationToken.HasValue)
      {
        if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

        _cancellationToken = ctx.LinkedCancelToken;
      }

      if (_cancellationToken.Value.IsCancellationRequested)
      {
        return BTNodeRuntimeState.Fail;
      }

      CombatUnit caster = ctx.Caster;

      if (_indicator == null)
      {
        _passed = deltaTime;

        GameObject casterGO = caster.CombatUnitGameObject.gameObject;
        Transform transform = casterGO.transform;
        transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);

        _indicator = ctx.ActionSO.ShowIndicator(
          ctx.CombatManager.IndicatorPool,
          position,
          rotation,
          _durationFill,
          _cancellationToken,
          caster.TimeBundle,
          _indicatorColor);

        RenderFeatureManager renderFeatureManager = ServiceLocator.Get<RenderFeatureManager>();
        renderFeatureManager.UnDarkenAsync(_indicator.gameObject, true).Forget();
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

          // Update Targets
          CombatActionNodeTargetUpdate.UpdateTargetList(ref Blackboard);

          _indicator.DelayedDisable(_durationDisable, caster.TimeBundle).Forget();
        }

        BTNodeRuntimeState state = BTNodeRuntimeState.Success;

        if (Child != null)
        {
          state = Child.UpdateNode(ref Blackboard, deltaTime);
        }

        if (state == BTNodeRuntimeState.Success || state == BTNodeRuntimeState.Fail)
        {
          // Reset for next call
          _passed = -1f;
          _indicator = null;
          _cancellationToken = null;
        }

        return state;
      }

      return BTNodeRuntimeState.Running;
    }

    protected override void OnReset()
    {
      _passed = -1f;
      _indicator = null;
      _cancellationToken = null;
      _isFilled = false;
    }
  }
}