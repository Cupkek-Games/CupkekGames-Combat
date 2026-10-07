using System;
using UnityEngine;
using CupkekGames.BehaviourTrees;

namespace CupkekGames.Combat
{
  [RequireComponent(typeof(CapsuleCollider))]
  public class CombatUnitAI : MonoBehaviour
  {
    // References
    private ICombatSettings _combatSettings;
    private ICombatManager _combatManager;

    public bool IsSetup => _combatSettings != null && _combatManager != null;

    // Movement, through the fight's space (made in SetupAI).
    private ICombatSpace _space;
    private ICombatMover _mover;
    // Steps OnUpdate on the space's clock while the AI runs.
    private IDisposable _drive;

    /// <summary>How this unit moves; null until <see cref="SetupAI"/>.</summary>
    public ICombatMover Mover => _mover;

    // State
    private bool _running = false;
    public bool IsRunning => _running;
    private CombatUnit _caster = null;
    private CombatUnit _primaryTarget = null;
    /// <summary>Whom the unit goes after; null while it has nobody.</summary>
    public CombatUnit Target => _primaryTarget;
    private CombatUnitThreatTable _combatUnitThreatTable = new CombatUnitThreatTable();
    public CombatUnitThreatTable CombatUnitThreatTable => _combatUnitThreatTable;
    // How the fight's units pick whom they go after, and the seconds since this one last asked.
    private ICombatTargeting _targeting;
    private float _sinceTargeting;
    private CombatActionRunner _runner;
    /// <summary>An action's run is set up: from the tick it starts (an ultimate's wait for its queue turn included) to its end.</summary>
    public bool IsActing => _runner != null;
    private int _actionType;
    // The action selected for the current run (reported with the ultimate's phases).
    private CombatActionSO _action;
    // The selected ultimate has begun to cast (a silence no longer drops it).
    private bool _ultimateStarted;

    private float _cooldown = 0;

    // Mechanics
    private bool _silenced = false;

    // Debug
    [SerializeField] bool _debug = false;

    public void SetupAI(ICombatSettings combatSettings, ICombatManager combatManager,
      CombatUnit caster)
    {
      _space = combatManager.UnitManager.Space
        ?? throw new InvalidOperationException($"[CombatUnitAI] '{gameObject.name}': the fight has no space (ICombatUnitManager.Space is null); give the fight a CombatSpace.");
      _mover = _space.CreateMover(caster.View);
      _targeting = combatManager.UnitManager.Targeting
        ?? throw new InvalidOperationException($"[CombatUnitAI] '{gameObject.name}': the fight has no targeting (ICombatUnitManager.Targeting is null); name one, e.g. CombatThreatTargeting.");

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
      // A unit without a death token was never initialized (SetupAI) or was
      // already killed/disposed - starting it would NRE on every tick.
      if (_caster == null || _caster.DeathToken == null)
      {
        Debug.LogError($"[CombatUnitAI] StartAI refused on '{gameObject.name}': the unit is not fully initialized (no death token). Call SetupAI on an initialized unit before StartAI.", this);
        return;
      }

      if (_debug)
      {
        Debug.Log("StartAI " + gameObject.name);
      }

      _caster.OnDeathEvent += OnDeath;
      _caster.Health.OnHit += OnHit;
      _drive = _space.Drive(_caster.TimeBundle.TimeContext, OnUpdate);
      _running = true;

      _mover.Start(_caster.TimeBundle);
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
        _mover.Stop();

        _caster.OnDeathEvent -= OnDeath;
        _caster.Health.OnHit -= OnHit;
        _drive?.Dispose();
        _drive = null;

        // A stopped unit keeps no run: a selected ultimate is dropped (and
        // reported), any other run starts over when the unit moves again.
        CancelSelectedUltimate();
        _combatManager.CombatUltimateManager.RemoveFromQueue(_caster);
        _runner = null;
        _action = null;

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
      _sinceTargeting = 0f;
    }

    private void OnUpdate(float deltaTime)
    {
      if (!_running)
      {
        return;
      }

      // The unit was killed/disposed under a still-running AI (KillAI without
      // StopAI) - stop loudly once instead of NRE-ing per tick in the runner.
      if (_caster == null || _caster.DeathToken == null)
      {
        Debug.LogError($"[CombatUnitAI] '{gameObject.name}' ticked on a dead or uninitialized unit (death token missing) - stopping AI. StopAI must run before the unit is killed/disposed.", this);
        StopAI(false);
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

      if (_runner == null && !TrySelectNextAction(deltaTime))
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
    /// Picks whom to go after and the next action, and sets up the action's run. Returns
    /// <c>true</c> if a run was set up; <c>false</c> while there is nobody to go after, while
    /// the unit is not settled on its place (walking or pushed), or while the action reaches
    /// nobody yet.
    /// </summary>
    private bool TrySelectNextAction(float deltaTime)
    {
      if (_primaryTarget != null && _primaryTarget.Health.Current <= 0) _primaryTarget = null;

      CombatActionSO action = SelectAction();
      _sinceTargeting += deltaTime;
      if (_primaryTarget == null || _sinceTargeting >= _targeting.Interval)
      {
        CombatUnit before = _primaryTarget;
        Retarget(action.TargetSelection.Range);

        // The pick among a slot's actions can turn on the target.
        if (_primaryTarget != before) action = SelectAction();
      }

      if (_primaryTarget == null)
      {
        return false;
      }

      _mover.SetReach(action.TargetSelection.Range);

      // An action starts only once the unit stands settled on its place and the action
      // reaches someone: until then it walks or turns toward its target, and an ultimate is
      // neither queued (no freeze) nor reported.
      if (!_mover.IsSettled || action.GetTargets(_combatManager.UnitManager, _caster, _primaryTarget, _debug).Count == 0)
      {
        return false;
      }

      int skillLevel = _caster.EffectiveSkillRank;

      _action = action;
      _runner = _caster.View.Runners[action];

      // A fresh source per run: every hit of this run names it.
      _runner.Setup(_caster, _primaryTarget, skillLevel, _combatSettings, _combatManager, CombatSource.ForAction(action, _caster));

      if (_debug)
      {
        Debug.Log("Action Setup: " + _actionType + " target: " + _primaryTarget.DataReference.Key);
      }

      if (_actionType == CombatActionType.Ultimate)
      {
        _ultimateStarted = false;
        _combatManager.CombatUltimateManager.Enqueue(_caster);
        _combatManager.EventDatabase.InvokeOnUltimate(_caster, action, UltimatePhase.Selected);
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
      if (_actionType == CombatActionType.Ultimate) _ultimateStarted = true;

      CombatActionSO done = _action;
      BTNodeRuntimeState state = _runner.UpdateTree(_combatManager.UnitManager, _primaryTarget, deltaTime, _debug);

      // The run's own hits can end it: the last enemy falls and the fight stops every unit,
      // which clears the run and reports a selected ultimate. Nothing is left to finish.
      if (_runner == null)
      {
        return;
      }

      if (state == BTNodeRuntimeState.Fail)
      {
        // Failed to execute action, try to move
        if (_actionType == CombatActionType.Ultimate)
        {
          _combatManager.CombatUltimateManager.Dequeue();
          _combatManager.EventDatabase.InvokeOnUltimate(_caster, done, UltimatePhase.Cancelled);

          if (_debug)
          {
            Debug.Log($"Ultimate failed: {_caster.DataReference.Key}");
          }
        }

        _runner = null;
        _action = null;
        _ultimateStarted = false;
        _actionType = CombatActionType.Skip;
      }
      else if (state == BTNodeRuntimeState.Success)
      {
        if (_actionType == CombatActionType.Ultimate)
        {
          _combatManager.CombatUltimateManager.Dequeue();
          _combatManager.EventDatabase.InvokeOnUltimate(_caster, done, UltimatePhase.Completed);

          if (_debug)
          {
            Debug.Log($"Ultimate completed: {_caster.DataReference.Key}");
          }
        }

        _runner = null;
        _action = null;
        _ultimateStarted = false;

        if (_caster != null)
        {
          _cooldown = _caster.GetActionCooldownSeconds();
          _caster.Mana.OnTakeAction(_actionType);
          Retarget(done.TargetSelection.Range);
        }
      }
      else
      {
        // Currently casting skill
        _mover.Hold();
      }

      if (_debug)
      {
        Debug.Log("CombatUnitAI FixedUpdate: " + _actionType + " state: " + state);
      }
    }

    // The action for this turn: the ultimate at full mana unless silenced. An ultimate that
    // can never reach anyone (its slot holds no action that picks someone) gives way to the
    // normal action, so a placeholder never locks its unit out.
    private CombatActionSO SelectAction()
    {
      int normal = _combatSettings.DefaultActionTypeId;
      _actionType = _silenced ? normal : _caster.Mana.GetNextActionType();
      CombatActionSO action = _caster.GetCombatAction(_actionType, _combatManager, _caster, _primaryTarget);
      if (action == null && _actionType != normal)
      {
        _actionType = normal;
        action = _caster.GetCombatAction(normal, _combatManager, _caster, _primaryTarget);
      }

      return action ?? throw new InvalidOperationException($"[CombatUnitAI] '{_caster.DataReference.Key}' has no action that can pick anyone in its normal slot ({normal}).");
    }

    // Asks the fight's targeting whom to go after, and follows it.
    private void Retarget(float range)
    {
      _sinceTargeting = 0f;
      _primaryTarget = _targeting.Pick(_caster, _primaryTarget, range);
      _mover.Follow(_primaryTarget);

      if (_debug)
      {
        Debug.Log("Target: " + (_primaryTarget != null ? _primaryTarget.DataReference.Key : "none"));
      }
    }

    private void OnDeath(CombatUnit combatUnit)
    {
      _mover.Halt();
    }

    private void OnHit(CombatUnit defender, CombatHit hit)
    {
      _combatUnitThreatTable.AddThreat(hit);
    }

    // Mechanics

    /// <summary>
    /// A silenced unit selects only its normal action. A silence also drops an
    /// ultimate selected but not yet started; one already casting runs on.
    /// </summary>
    public void SetSilenced(bool silence)
    {
      _silenced = silence;

      if (silence && !_ultimateStarted) CancelSelectedUltimate();
    }

    /// <summary>
    /// Drops the ultimate this unit has selected: off the queue (the freeze
    /// ends with it), the run cleared, the turn skipped, and reported as
    /// cancelled. Its mana stays full, so it selects again when it can (a
    /// silenced unit attacks instead). Nothing selected: nothing happens.
    /// </summary>
    public void CancelSelectedUltimate()
    {
      if (_runner == null || _actionType != CombatActionType.Ultimate || _caster == null) return;

      CombatUnit caster = _caster;
      CombatActionSO action = _action;

      _runner = null;
      _action = null;
      _ultimateStarted = false;
      _actionType = CombatActionType.Skip;

      _combatManager.CombatUltimateManager.RemoveFromQueue(caster);
      _combatManager.EventDatabase.InvokeOnUltimate(caster, action, UltimatePhase.Cancelled);
    }
  }
}