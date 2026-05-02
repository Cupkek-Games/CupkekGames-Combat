using UnityEngine;
using System.Collections.ObjectModel;
using CupkekGames.BehaviourTrees;
using CupkekGames.Navigation;

namespace CupkekGames.Combat
{
  [RequireComponent(typeof(NavMeshAgentController))]
  [RequireComponent(typeof(CapsuleCollider))]
  public class CombatUnitAI : MonoBehaviour
  {
    // References
    private ICombatSettings _combatSettings;
    private ICombatManager _combatManager;

    public bool IsSetup => _combatSettings != null && _combatManager != null;

    // Navigation
    private NavMeshAgentController _navMeshAgentController;

    public NavMeshAgentController NavMeshAgentController => _navMeshAgentController;

    // State
    private bool _running = false;
    public bool IsRunning => _running;
    private CombatUnit _caster = null;
    private CombatUnit _primaryTarget = null;
    private CombatUnitThreatTable _combatUnitThreatTable = new CombatUnitThreatTable();
    public CombatUnitThreatTable CombatUnitThreatTable => _combatUnitThreatTable;
    private float _threatTableCheckCooldown = 0;
    private CombatActionRunner _runner;
    private int _actionType;

    private float _cooldown = 0;

    // Mechanics
    private bool _silenced = false;

    // Debug
    [SerializeField] bool _debug = false;

    private void Awake()
    {
      _navMeshAgentController = GetComponent<NavMeshAgentController>();
    }

    public void SetupAI(ICombatSettings combatSettings, ICombatManager combatManager,
      CombatUnit caster)
    {
      _combatSettings = combatSettings;

      CapsuleCollider collider = GetComponent<CapsuleCollider>();
      collider.radius = combatSettings.AIColliderRadius;
      collider.height = combatSettings.AIColliderHeight;
      collider.center = combatSettings.AIColliderCenter;

      if (_debug)
      {
        Debug.Log("SetupAI " + gameObject.name);
      }

      _combatManager = combatManager;
      _caster = caster;
      _caster.SetupAI();
    }

    public void StartAI()
    {
      if (_debug)
      {
        Debug.Log("StartAI " + gameObject.name);
      }

      _caster.OnDeathEvent += OnDeath;
      _caster.Health.OnTakeDamage += OnTakeDamage;
      _caster.TimeBundle.TimeContext.OnUpdate += OnUpdate;
      _running = true;

      _navMeshAgentController.RegisterTimeContext(_caster.TimeBundle);
      _navMeshAgentController.StartMonitoringAgent();
    }

    public void StopAI(bool dead)
    {
      if (_debug)
      {
        Debug.Log("StopAI " + gameObject.name);
      }

      _running = false;
      if (_caster != null)
      {
        _navMeshAgentController.UnRegisterTimeContext();
        _navMeshAgentController.StopAll();

        _caster.OnDeathEvent -= OnDeath;
        _caster.Health.OnTakeDamage -= OnTakeDamage;
        _caster.TimeBundle.TimeContext.OnUpdate -= OnUpdate;

        _combatManager.CombatUltimateManager.RemoveFromQueue(_caster);

        if (dead)
        {
          _caster.KillAI();
          _caster = null;

          _runner = null;
        }
        else
        {
          _caster.InterruptAI();
        }
      }

      _primaryTarget = null;
    }

    private void OnUpdate(float deltaTime)
    {
      if (!_running)
      {
        return;
      }

      if (_cooldown > 0)
      {
        _cooldown -= deltaTime;
        return;
      }

      if (deltaTime < float.Epsilon)
      {
        return;
      }

      if (_runner == null && !TrySelectNextAction())
      {
        return;
      }

      if (_actionType == CombatActionType.Ultimate && !IsUltimateQueueTurn())
      {
        return;
      }

      ExecuteCurrentAction(deltaTime);
    }

    /// <summary>
    /// Finds a target, selects the next action, and sets up the action runner.
    /// Returns <c>true</c> if a runner was successfully created; <c>false</c> if no valid target exists.
    /// </summary>
    private bool TrySelectNextAction()
    {
      if (_primaryTarget == null || _primaryTarget.Health.Current <= 0)
      {
        FindAndFollowTarget();
      }

      // If still no valid target after searching (e.g., combat ended), skip this update
      if (_primaryTarget == null)
      {
        return false;
      }

      // Action Selection
      if (_silenced)
      {
        _actionType = CombatActionType.Normal;
      }
      else
      {
        _actionType = _caster.Mana.GetNextActionType();
      }

      CombatActionSO action = _caster.GetCombatAction(_actionType, _combatManager, _caster, _primaryTarget);

      OnActionSelect(action);

      int skillLevel = _caster.Level;

      _runner = _caster.CombatUnitGameObject.Runners[action];

      _runner.Setup(_caster, _primaryTarget, skillLevel, _combatSettings, _combatManager);

      if (_debug)
      {
        Debug.Log("Action Setup: " + _actionType + " target: " + _primaryTarget.DataReference.Key);
      }

      if (_actionType == CombatActionType.Ultimate)
      {
        _combatManager.CombatUltimateManager.Enqueue(_caster);
      }

      return true;
    }

    /// <summary>
    /// Checks whether this unit is the next in the ultimate queue.
    /// Returns <c>true</c> if it is this unit's turn (or the action is not an ultimate).
    /// </summary>
    private bool IsUltimateQueueTurn()
    {
      if (!_combatManager.CombatUltimateManager.HasNext)
      {
        if (_debug)
        {
          Debug.LogWarning($"Ultimate queue is empty for {_caster.DataReference.Key}");
        }

        return false;
      }

      CombatUnit nextCombatUnit = _combatManager.CombatUltimateManager.Peek();

      if (nextCombatUnit != _caster)
      {
        if (_debug)
        {
          Debug.Log($"Ultimate queue: {_caster.DataReference.Key} waiting for {nextCombatUnit?.DataReference.Key}");
        }

        return false;
      }

      if (_debug)
      {
        Debug.Log($"Ultimate executing: {_caster.DataReference.Key}");
      }

      return true;
    }

    /// <summary>
    /// Runs the current action's behaviour tree for one tick and handles the result.
    /// </summary>
    private void ExecuteCurrentAction(float deltaTime)
    {
      BTNodeRuntimeState state = _runner.UpdateTree(_combatManager.UnitManager, _primaryTarget, deltaTime, _debug);

      if (state == BTNodeRuntimeState.Fail)
      {
        // Failed to execute action, try to move
        if (_actionType == CombatActionType.Ultimate)
        {
          _combatManager.CombatUltimateManager.Dequeue();

          if (_debug)
          {
            Debug.Log($"Ultimate failed: {_caster.DataReference.Key}");
          }
        }

        _runner = null;
        _actionType = CombatActionType.Skip;

        ThreatTableCheck(deltaTime, false);
      }
      else if (state == BTNodeRuntimeState.Success)
      {
        if (_actionType == CombatActionType.Ultimate)
        {
          _combatManager.CombatUltimateManager.Dequeue();

          if (_debug)
          {
            Debug.Log($"Ultimate completed: {_caster.DataReference.Key}");
          }
        }

        _runner = null;

        if (_caster != null)
        {
          _cooldown = _caster.GetActionCooldownSeconds();
          _caster.Mana.OnTakeAction(_actionType);
        }

        ThreatTableCheck(deltaTime, true);
      }
      else
      {
        // Currently casting skill
        _navMeshAgentController.StopMonitoringAgent();
        _navMeshAgentController.StopFollow();
      }

      if (_debug)
      {
        Debug.Log("CombatUnitAI FixedUpdate: " + _actionType + " state: " + state);
      }
    }

    private void ThreatTableCheck(float deltaTime, bool force)
    {
      _threatTableCheckCooldown += deltaTime;
      if (force || _threatTableCheckCooldown >= _combatSettings.ThreatTableCheckInterval)
      {
        if (_debug)
        {
          Debug.Log("ThreatTableCheck executing...");
        }

        _threatTableCheckCooldown = 0;
        FindAndFollowTarget();
      }
      else if (_debug)
      {
        Debug.Log("ThreatTableCheck skipped");
      }
    }

    private void OnActionSelect(CombatActionSO action)
    {
      _navMeshAgentController.SetStoppingDistance(action.TargetSelection.Range);
    }

    private void FindAndFollowTarget()
    {
      if (_caster == null)
      {
        if (_debug)
        {
          Debug.Log("Caster is null, cannot find target.");
        }

        return;
      }

      // Enemy collection
      ReadOnlyCollection<CombatUnit> targets;

      if (_combatManager.UnitManager.CombatUnitsAlly.Contains(_caster))
      {
        targets = _combatManager.UnitManager.CombatUnitsEnemy;
      }
      else
      {
        targets = _combatManager.UnitManager.CombatUnitsAlly;
      }

      if (targets.Count == 0)
      {
        if (_debug)
        {
          Debug.Log("No targets found for " + _caster.DataReference.Key);
        }

        return;
      }
      else if (targets.Count == 1)
      {
        _combatUnitThreatTable.AddThreat(targets[0], _combatSettings.DistanceThreat);
      }
      else
      {
        (CombatUnit closest, float distanceSqr) = FindClosestEnemy(targets);
        _combatUnitThreatTable.AddThreat(closest, (int)(_combatSettings.DistanceThreat / distanceSqr));
      }

      UpdatePrimaryTarget();
    }

    private (CombatUnit, float) FindClosestEnemy(ReadOnlyCollection<CombatUnit> targets)
    {
      Vector3 referencePosition = _caster.CombatUnitGameObject.transform.position;

      CombatUnit closest = null;
      float closestDistanceSqr = Mathf.Infinity;

      foreach (CombatUnit target in targets)
      {
        if (target != null && target.CombatUnitGameObject != null && target.Health.Current > 0)
        {
          Vector3 directionToTarget = target.CombatUnitGameObject.transform.position - referencePosition;
          float dSqrToTarget = directionToTarget.sqrMagnitude;

          if (dSqrToTarget < closestDistanceSqr)
          {
            closestDistanceSqr = dSqrToTarget;
            closest = target;
          }
        }
      }

      return (closest, closestDistanceSqr);
    }

    private void OnDeath(CombatUnit combatUnit)
    {
      _navMeshAgentController.StopAll();
    }

    private void OnTakeDamage(CombatUnit defender, int damage, CombatUnit damager)
    {
      _combatUnitThreatTable.AddThreat(damager, damage);
    }

    private void UpdatePrimaryTarget()
    {
      CombatUnit newTarget = _combatUnitThreatTable.GetHighestThreatTarget();

      _primaryTarget = newTarget;

      _navMeshAgentController.FollowTarget(_primaryTarget.CombatUnitGameObject.transform, true, _debug);
      _navMeshAgentController.StartMonitoringAgent();

      if (_debug)
      {
        Debug.Log("Primary target updated: " + (_primaryTarget != null ? _primaryTarget.DataReference.Key : "null"));
      }
    }

    // Mechanics
    public void SetSilenced(bool silence)
    {
      if (silence)
      {
        if (_silenced)
        {
          // already silenced
          return;
        }

        _silenced = true;

        // No scenario where a unit is in the queue when _actionType is not Ultimate
      }
      else
      {
        _silenced = false;
      }
    }
  }
}