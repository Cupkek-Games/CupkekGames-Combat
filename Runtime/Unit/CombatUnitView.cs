using UnityEngine;
using System.Collections.Generic;
using CupkekGames.Animations;
using CupkekGames.Services;
using CupkekGames.VFX;
using CupkekGames.Character;
using CupkekGames.Units;

namespace CupkekGames.Combat
{
  /// <summary>
  /// Combat-specific view extending <see cref="UnitView"/>.
  /// Adds AI, combat animations, highlight, action runners.
  /// </summary>
  public class CombatUnitView : UnitView
  {
    // Combat-specific components
    private IAnimationStateController _animationController;
    public IAnimationStateController AnimationController => _animationController;
    private IAnimationEngine _animationEngine;
    public IAnimationEngine AnimationEngine => _animationEngine;
    private IAnimationTimeController _animationTimeController;
    public IAnimationTimeController AnimationTimeController => _animationTimeController;
    private CombatUnitHighlightController _highlightController;
    public CombatUnitHighlightController HighlightController => _highlightController;
    [SerializeField] private Transform _healthBarPosition;
    public Transform HealthBarTransform => _healthBarPosition;
    [Tooltip("The animation kind this unit plays while it moves in a fight on a grid (Walk, or Run for a unit that runs to its targets).")]
    [SerializeField] private string _moveAnimationKind = AnimationKinds.Walk;
    /// <summary>The animation kind played while the unit moves between tiles.</summary>
    public string MoveAnimationKind => _moveAnimationKind;
    private CharacterVisualAccessories _equipments;
    public CharacterVisualAccessories Equipments => _equipments;
    private ShaderColorController _shaderColorController;
    public ShaderColorController ShaderColorController => _shaderColorController;
    private ShaderEmissionController _shaderEmissionController;
    public ShaderEmissionController ShaderEmissionController => _shaderEmissionController;
    private Collider _collider;

    // References
    private EventDatabaseCombat _eventDatabaseCombat;
    private CombatUnit _combatUnit = null;

    public CombatUnit CombatUnit => _combatUnit;

    // Navigation
    private CombatUnitAI _combatUnitAI;
    public CombatUnitAI CombatUnitAI => _combatUnitAI;

    // Actions
    private Dictionary<CombatActionSO, CombatActionRunner> _runners =
      new Dictionary<CombatActionSO, CombatActionRunner>();

    public Dictionary<CombatActionSO, CombatActionRunner> Runners => _runners;

    protected override void Awake()
    {
      base.Awake(); // UnitView.Awake — refreshes renderer cache

      _animationController = GetComponentInChildren<IAnimationStateController>();
      _animationEngine = GetComponentInChildren<IAnimationEngine>();
      _animationTimeController = GetComponentInChildren<IAnimationTimeController>();
      _equipments = GetComponent<CharacterVisualAccessories>();
      _shaderColorController = GetComponentInChildren<ShaderColorController>();
      _shaderEmissionController = GetComponentInChildren<ShaderEmissionController>();
      _combatUnitAI = GetComponent<CombatUnitAI>();
      _highlightController = GetComponent<CombatUnitHighlightController>();
      if (_highlightController == null)
      {
        Debug.LogError($"[CombatUnitView] {gameObject.name}: Missing CombatUnitHighlightController component.", this);
      }

      _collider = GetComponent<Collider>();
      if (_collider != null)
      {
        _collider.enabled = false;
      }
    }

    public virtual void OnDisable()
    {
      // A hit's squash or nudge still running on a body that leaves (despawned, or the fight torn down).
      if (_animationController != null)
      {
        SquashAndStretch.Stop(_animationController.Transform);
        Recoil.Stop(_animationController.Transform);
      }

      UnRegisterCombatUnit();
    }

    public void RegisterCombatUnit(CombatUnit combatUnit)
    {
      if (_combatUnit != null)
      {
        Debug.LogError("View already registered");
        return;
      }

      if (combatUnit == null)
      {
        Debug.LogError("CombatUnit is null");
        return;
      }

      _combatUnit = combatUnit;
      _combatUnit.OnDeathEvent += OnDeath;

      if (_collider != null)
      {
        _collider.enabled = true;
      }

      if (_animationTimeController != null)
      {
        _animationTimeController.RegisterTimeContext(_combatUnit.TimeBundle.TimeContext);
      }

      ICombatSettings combatSettings = ServiceLocator.Get<ICombatSettings>();
      ICombatManager combatManager = ServiceLocator.Get<ICombatManager>(true);

      if (_combatUnitAI != null && combatSettings != null && combatManager != null)
      {
        _combatUnitAI.SetupAI(combatSettings, combatManager, _combatUnit);
      }

      _eventDatabaseCombat = ServiceLocator.Get<EventDatabaseCombat>(true);
      if (_eventDatabaseCombat != null)
      {
        _eventDatabaseCombat.WinEvent += OnWin;
      }

      CombatAttributesDefinition combatData = _combatUnit.CombatData;
      if (combatData != null)
      {
        foreach (CombatActionSO action in combatData.GetAllActions())
        {
          if (_runners.ContainsKey(action))
          {
            continue;
          }

          _runners.Add(action, new CombatActionRunner(_combatUnit, action));
        }
      }
    }

    public void UnRegisterCombatUnit()
    {
      if (_collider != null)
      {
        _collider.enabled = false;
      }

      if (_combatUnitAI != null && _combatUnitAI.IsSetup)
      {
        _combatUnitAI.StopAI(true);
      }

      if (_eventDatabaseCombat != null)
      {
        _eventDatabaseCombat.WinEvent -= OnWin;
      }

      if (_combatUnit != null)
      {
        CombatUnit combatUnit = _combatUnit;
        _combatUnit = null;

        combatUnit.OnDeathEvent -= OnDeath;
        combatUnit.UnregisterView();
      }

      if (_animationTimeController != null)
      {
        _animationTimeController.UnRegisterTimeContext();
      }
    }

    private void OnDeath(CombatUnit combatUnit)
    {
      UnRegisterCombatUnit();

      if (gameObject == null)
      {
        return;
      }

      _animationController?.Play(AnimationKinds.Death);
    }

    private void OnWin()
    {
      if (gameObject == null || _animationController == null)
      {
        return;
      }

      _animationController.Play(AnimationKinds.Win);
    }

    public void TakeDamageSquashAndStretch(float bumpAmount)
    {
      TakeDamageSquashAndStretch(bumpAmount, 0.3f);
    }

    /// <summary>
    /// A hit's squash over <paramref name="seconds"/>: a quick squash, a stretch and a longer settle (a fifth, a
    /// third and the rest of it).
    /// </summary>
    public void TakeDamageSquashAndStretch(float bumpAmount, float seconds)
    {
      if (_animationController != null)
      {
        SquashAndStretch.TakeDamage(_animationController.Transform, bumpAmount, seconds * 0.2f, seconds * 0.3f,
          seconds * 0.5f);
      }
    }

    /// <summary>
    /// A hit's nudge: the model pushed <paramref name="distance"/> away from <paramref name="from"/> (along the
    /// ground) and springing back, over <paramref name="seconds"/>. The model moves, not the unit, so its walking and
    /// its animations are left alone.
    /// </summary>
    public void TakeDamageNudge(Vector3 from, float distance, float seconds)
    {
      if (_animationController == null) return;
      Vector3 away = transform.position - from;
      away.y = 0f;
      if (away.sqrMagnitude < 0.0001f) return;
      Recoil.Push(_animationController.Transform, away.normalized * distance, seconds * 0.3f, seconds * 0.7f);
    }

    /// <summary>
    /// Sets visibility including combat-specific UI (health bar).
    /// </summary>
    public new void SetVisible(bool visible)
    {
      base.SetVisible(visible);

      // Combat-specific: health bar world space UI
      if (_healthBarPosition != null)
      {
        for (int i = 0; i < _healthBarPosition.childCount; i++)
        {
          _healthBarPosition.GetChild(i).gameObject.SetActive(visible);
        }
      }
    }
  }
}
