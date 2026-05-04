using UnityEngine;
using System.Collections.Generic;
using CupkekGames.Animations;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
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
    private CharacterVisualAccessories _equipments;
    public CharacterVisualAccessories Equipments => _equipments;
    private ShaderColorController _shaderColorController;
    public ShaderColorController ShaderColorController => _shaderColorController;
    private ShaderEmissionController _shaderEmissionController;
    public ShaderEmissionController ShaderEmissionController => _shaderEmissionController;
    private Collider _collider;

    // References
    private RenderFeatureManager _renderFeatureManager;
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

      _renderFeatureManager = ServiceLocator.Get<RenderFeatureManager>();
    }

    public virtual void OnEnable()
    {
      _renderFeatureManager?.Register(Renderers);
    }

    public virtual void OnDisable()
    {
      _renderFeatureManager?.Unregister(Renderers);

      UnRegisterCombatUnit();
    }

    public void RegisterCombatUnit(CombatUnit combatUnit)
    {
      if (_combatUnit != null)
      {
        Debug.LogError("CombatUnitGameObject already registered");
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
        combatUnit.UnregisterCombatUnitGameObject();
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
      if (_animationController != null)
      {
        SquashAndStretch.TakeDamage(_animationController.Transform, bumpAmount, 0.1f);
      }
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
