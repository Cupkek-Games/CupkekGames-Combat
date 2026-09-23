using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using CupkekGames.TimeSystem;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
using CupkekGames.Transforms;

using CupkekGames.VFX;
using CupkekGames.Pool;

namespace CupkekGames.Combat
{
  [System.Serializable]
  public class Projectile
  {
    private static readonly int DISTANCE_REFERENCE_MAX = 30;
    [SerializeField] private float _projectileDuration = 0.5f;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Vector3 _offset = Vector3.zero;
    [SerializeField] private Quaternion _rotation = Quaternion.identity;
    [SerializeField] private Vector3 _scale = Vector3.one;
    [Header("Collision")][SerializeField] private bool _withCollision = false;
    [SerializeField] private bool _destroyOnCollision = false;
    [SerializeField] private float _collisionRadius = 0.5f;
    [SerializeField] private Vector3 _tweenPosition = Vector3.zero;
    [SerializeField] private float _tweenDuration = 0.5f;
    [SerializeField] private Ease _tweenEase = Ease.OutSine;

    [Header("VFX")][SerializeField] private VFXBundle _hitPrefab;
    [SerializeField] private VFXBundle _flashPrefab;

    private GameObjectPool _projectilePool = null;

    public void Prewarm(GameObject parent)
    {
      _projectilePool = new GameObjectPool(_projectilePrefab, 2, 4, false);
      _hitPrefab?.Prewarm(parent);
      _flashPrefab?.Prewarm(parent);

      _projectilePool.OnCreateEvent += OnCreateEvent;
      _projectilePool.OnDestroyObjectEvent += OnDestroyEvent;
      _projectilePool.Prewarm();
    }

    private void OnCreateEvent(GameObject gameObject)
    {
      RenderFeatureManager manager = ServiceLocator.Get<RenderFeatureManager>(true);
      manager?.Register(gameObject, true);
    }

    private void OnDestroyEvent(GameObject gameObject)
    {
      RenderFeatureManager manager = ServiceLocator.Get<RenderFeatureManager>(true);
      if (manager == null)
      {
        return;
      }

      manager.Unregister(gameObject, true);
    }

    public void Dispose()
    {
      _projectilePool.Pool.Dispose();
      _hitPrefab?.Dispose();
      _flashPrefab?.Dispose();
      _projectilePool = null;
    }

    /// <summary>
    /// Flies the projectile and runs <paramref name="child"/> (the payload) where it lands.
    /// A homing projectile follows its target while it stands and, if the target falls
    /// mid-flight, flies on to where it last was and lands there: the payload still runs,
    /// with <see cref="CombatActionContext.ImpactPosition"/> set, so an area payload hits
    /// around the landing point and a dead target simply takes nothing. A collision
    /// projectile runs the payload on each unit it touches.
    /// </summary>
    public async UniTask PlayProjectile(
      CombatUnit caster,
      CombatUnit target,
      GraphFrame frame,
      BTNode child,
      CancellationToken globalCancelToken,
      TimeBundle timeBundle,
      RenderFeatureManager renderFeatureManager)
    {
      if (_projectilePool == null)
      {
        throw new Exception("_projectilePool is not initialized");
      }

      // Nothing to aim a homing shot at: the target fell before it left the hand.
      if (!_withCollision && !target.IsAlive)
      {
        return;
      }

      Transform spawnTransform = caster.View.Center.transform;
      spawnTransform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);

      Vector3 targetPosition;
      if (_withCollision)
      {
        Vector3 worldSpaceDisplacement = startRotation * _tweenPosition;

        targetPosition = startPosition + worldSpaceDisplacement;
      }
      else if (target != null && target.View != null)
      {
        targetPosition = target.View.Center.position;
      }
      else
      {
        Debug.LogError("Target is null for projectile without collision. Cannot play projectile.");
        return;
      }

      Vector3 spawnPos = startPosition + startRotation * _offset;
      Quaternion lookRot = Quaternion.LookRotation(targetPosition - spawnPos);
      Quaternion spawnRot = lookRot * _rotation;

      // Play flash VFX
      if (_flashPrefab != null)
      {
        _flashPrefab?.Play(caster.View.gameObject, spawnPos, spawnRot, globalCancelToken,
          caster.TimeBundle, renderFeatureManager).Forget();
      }

      // Play projectile
      GameObject projectile = _projectilePool.Pool.Get();
      projectile.transform.SetPositionAndRotation(spawnPos, spawnRot);
      projectile.transform.localScale = _scale;
      TransformUtils.SetScaleRecursive(projectile.transform, _scale);

      if (_withCollision)
      {
        ProjectileCollisionHandler collisionHandler = projectile.GetComponent<ProjectileCollisionHandler>();
        collisionHandler.Setup(caster, (_, hit) => OnProjectileCollision(frame, child, projectile, hit),
          _destroyOnCollision, _collisionRadius);
      }

      renderFeatureManager.UnDarkenAsync(projectile, true).Forget();

      projectile.SetActive(true);

      float distanceSqr = (spawnPos - targetPosition).sqrMagnitude;
      float durationMul = Mathf.Clamp(distanceSqr / DISTANCE_REFERENCE_MAX, 0.1f, 1f);

      bool isCanceled;
      Vector3 landing = targetPosition;
      if (_withCollision)
      {
        float projectileDuration = _tweenDuration * durationMul;
        if (projectileDuration <= 0)
        {
          projectileDuration = 0.1f;
        }

        isCanceled =
          await TweenProjectile(projectile, targetPosition, projectileDuration, _tweenEase, globalCancelToken,
            timeBundle).SuppressCancellationThrow();
      }
      else
      {
        float projectileDuration = _projectileDuration * durationMul;
        if (projectileDuration <= 0)
        {
          projectileDuration = 0.1f;
        }

        // Only the fight ending stops a homing shot: its target falling does not.
        (isCanceled, landing) =
          await ProjectileFollowCombatUnit(target, projectile, projectileDuration, spawnPos, targetPosition,
            globalCancelToken, timeBundle).SuppressCancellationThrow();
      }

      if (projectile != null && projectile.activeSelf)
      {
        projectile.SetActive(false);
      }

      if (isCanceled)
      {
        return;
      }

      // Play hit VFX
      if (_hitPrefab != null && caster.View != null && caster.View.gameObject != null)
      {
        _hitPrefab.Play(caster.View.gameObject, projectile.transform, globalCancelToken,
          caster.TimeBundle, renderFeatureManager).Forget();
      }

      if (!_withCollision)
      {
        frame.SetLocal(CombatActionContext.ImpactPositionKey, landing);
        child.UpdateNode(frame, 0);
      }
    }

    /// <summary>
    /// Tween projectile to a fixed location
    /// Can be canceled with token
    /// Fails if target cant be reached
    /// </summary>
    private async UniTask TweenProjectile(
      GameObject projectile,
      Vector3 targetPosition,
      float projectileDuration,
      Ease ease,
      CancellationToken ct,
      TimeBundle timeBundle)
    {
      Tween tween = Tween.Position(projectile.transform, targetPosition, projectileDuration, ease);

      timeBundle.TimeScaleTween.Add(tween);

      await tween.ToYieldInstruction().ToUniTask(cancellationToken: ct);
    }


    /// <summary>
    /// Homing flight: follows the target while it stands; once it falls (or its view
    /// is gone) the shot flies on to the last place it saw it. Returns where it landed.
    /// </summary>
    private async UniTask<Vector3> ProjectileFollowCombatUnit(
      CombatUnit target,
      GameObject projectile,
      float projectileDuration,
      Vector3 spawnPos,
      Vector3 targetPosition,
      CancellationToken ct,
      TimeBundle timeBundle)
    {
      float elapsedTime = 0f;
      while (elapsedTime < projectileDuration && projectile != null && projectile.activeSelf)
      {
        if (target.IsAlive && target.View != null)
        {
          targetPosition = target.View.Center.position;
        }

        float t = elapsedTime / projectileDuration;
        projectile.transform.position = Vector3.Lerp(spawnPos, targetPosition, t);

        elapsedTime += timeBundle?.TimeContext.DeltaTime ?? Time.deltaTime;
        await UniTask.Yield(cancellationToken: ct);
      }

      return targetPosition;
    }

    private void OnProjectileCollision(GraphFrame frame, BTNode child, GameObject projectile, CombatUnit targetUnit)
    {
      List<CombatUnit> targetList = new List<CombatUnit>()
      {
        targetUnit
      };
      frame.SetGlobal("TargetList", targetList);
      frame.SetLocal(CombatActionContext.ImpactPositionKey, projectile.transform.position);

      child.UpdateNode(frame, 0);
    }
  }
}