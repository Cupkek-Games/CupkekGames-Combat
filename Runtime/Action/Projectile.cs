using UnityEngine;
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
    [Header("Pierce")]
    [Tooltip("Flies the run's locked line (its warning's area) to the far end and hits each target on it as it passes, instead of homing on one.")]
    [SerializeField] private bool _pierce = false;
    [Tooltip("Seconds a pierce takes to fly a long line (shorter lines fly faster, as homing shots do).")]
    [SerializeField] private float _pierceSeconds = 0.5f;

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
    /// around the landing point and a dead target simply takes nothing. A pierce flies the
    /// run's locked line and runs the payload on each of the run's targets as it passes them.
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
      if (!_pierce && !target.IsAlive)
      {
        return;
      }

      // The payload runs where the shot lands, maybe after the runner began its
      // next run: it keeps what the launch knew, its source above all.
      CombatActionContext launch = CombatActionContext.Capture(frame);

      Transform spawnTransform = caster.View.Center.transform;
      spawnTransform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);

      Vector3 targetPosition;
      Vector3 way = Vector3.zero;
      if (_pierce)
      {
        CombatArea area = launch.Area ?? throw new InvalidOperationException(
          "[Projectile] a pierce flies its run's locked line: put it under a warning (CombatActionNodeShowIndicator) on a line selection.");
        // To the far edge of the line's last cell.
        way = area.Forward;
        targetPosition = startPosition + way * launch.CombatManager.UnitManager.Space.ToWorld(area.Length + 0.5f);
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

      renderFeatureManager.UnDarkenAsync(projectile, true).Forget();

      projectile.SetActive(true);

      float distanceSqr = (spawnPos - targetPosition).sqrMagnitude;
      float durationMul = Mathf.Clamp(distanceSqr / DISTANCE_REFERENCE_MAX, 0.1f, 1f);

      bool isCanceled;
      Vector3 landing = targetPosition;
      if (_pierce)
      {
        float projectileDuration = Mathf.Max(0.1f, _pierceSeconds * durationMul);
        isCanceled =
          await Pierce(launch, child, projectile, spawnPos, targetPosition, way, projectileDuration, globalCancelToken,
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

      if (!_pierce)
      {
        launch.Frame.SetLocal(CombatActionContext.ImpactPositionKey, landing);
        child.UpdateNode(launch.Frame, 0);
      }
    }

    /// <summary>
    /// A pierce's flight at an even speed along its line: each of the run's targets takes
    /// the payload, on a branch of its own, as the shot passes it (one that fell meanwhile
    /// takes nothing).
    /// </summary>
    private async UniTask Pierce(
      CombatActionContext launch,
      BTNode child,
      GameObject projectile,
      Vector3 from,
      Vector3 to,
      Vector3 way,
      float projectileDuration,
      CancellationToken ct,
      TimeBundle timeBundle)
    {
      List<CombatUnit> ahead = new List<CombatUnit>(launch.TargetList ?? new List<CombatUnit>());
      ahead.Sort((a, b) => Along(a, from, way).CompareTo(Along(b, from, way)));
      float length = Vector3.Distance(from, to);

      float elapsedTime = 0f;
      int next = 0;
      while (true)
      {
        float t = Mathf.Clamp01(elapsedTime / projectileDuration);
        projectile.transform.position = Vector3.Lerp(from, to, t);

        while (next < ahead.Count && Along(ahead[next], from, way) <= t * length)
        {
          HitAsItPasses(launch, child, projectile, ahead[next++]);
        }

        if (t >= 1f) break;

        elapsedTime += timeBundle?.TimeContext.DeltaTime ?? Time.deltaTime;
        await UniTask.Yield(cancellationToken: ct);
      }
    }

    // How far along the shot's way a unit stands.
    private static float Along(CombatUnit unit, Vector3 from, Vector3 way)
    {
      if (unit?.View == null) return 0f;
      Vector3 offset = unit.View.transform.position - from;
      offset.y = 0f;
      return Vector3.Dot(offset, way);
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

    // Each unit the shot passes is its payload's one target, on a branch of its own.
    private static void HitAsItPasses(CombatActionContext launch, BTNode child, GameObject projectile, CombatUnit targetUnit)
    {
      if (targetUnit == null || !targetUnit.IsAlive) return;

      CombatActionContext hit = launch.Branch();
      hit.TargetList = new List<CombatUnit> { targetUnit };
      hit.Frame.SetLocal(CombatActionContext.ImpactPositionKey, projectile.transform.position);

      child.UpdateNode(hit.Frame, 0);
    }
  }
}