using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.BehaviourTrees;
using CupkekGames.TimeSystem;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;

using CupkekGames.VFX;

namespace CupkekGames.Combat
{
  public class CombatActionNodeVFX : CombatActionNodeWithTarget
  {
    [SerializeField] private VFXBundle _vfxBundle;

    public override void Prewarm(GameObject parent)
    {
      base.Prewarm(parent);

      _vfxBundle.Prewarm(parent);
    }

    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {
      var ctx = CombatActionContext.From(Blackboard);
      if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

      RenderFeatureManager renderFeatureManager = ServiceLocator.Get<RenderFeatureManager>();

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        GameObject go = target.CombatUnitGameObject.Center.gameObject;

        // TimeBundle timeBundle = _useCasterTimeScale ? caster.TimeBundle : target.TimeBundle;
        // NOTE: VFXBundle is using caster.TimeBundle for now as it is default and most desired behavior
        // If there is a need to use target.TimeBundle, we can add a parameter

        _vfxBundle.Play(go, go.transform.position, go.transform.rotation, ctx.CombatCancelToken, ctx.Caster.TimeBundle, renderFeatureManager)
          .Forget();
      }

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {
    }
  }
}