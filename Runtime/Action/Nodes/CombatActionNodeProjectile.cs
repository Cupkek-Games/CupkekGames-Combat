using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.BehaviourTrees;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;

using CupkekGames.VFX;

namespace CupkekGames.Combat
{
  public class CombatActionNodeProjectile : BTNodeDecorator
  {
    [SerializeField] private Projectile _projectile;
    public override void Prewarm(GameObject parent)
    {
      base.Prewarm(parent);

      _projectile.Prewarm(parent);
    }
    public override void Dispose()
    {
      base.Dispose();

      _projectile.Dispose();
    }
    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {
      var ctx = CombatActionContext.From(Blackboard);
      if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

      if (ctx.TargetList.Count == 0)
      {
        return BTNodeRuntimeState.Fail;
      }

      CombatUnit target = ctx.TargetList[0];

      RenderFeatureManager renderFeatureManager = ServiceLocator.Get<RenderFeatureManager>();

      if (target == null || target.DeathToken == null || target.CombatUnitGameObject == null)
      {
        return BTNodeRuntimeState.Fail;
      }

      _projectile.PlayProjectile(ctx.Caster, target, Blackboard, Child, ctx.CombatCancelToken, target.DeathToken.Token, ctx.Caster.TimeBundle, renderFeatureManager).Forget();

      return BTNodeRuntimeState.Success;
    }

    protected override void OnReset()
    {

    }
  }
}