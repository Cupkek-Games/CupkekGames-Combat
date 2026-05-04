using UnityEngine;
using Cysharp.Threading.Tasks;
using CupkekGames.Data;
using CupkekGames.Luna;
using System.Threading;
using System;
using CupkekGames.TimeSystem;

using CupkekGames.VFX;

namespace CupkekGames.Combat
{
  /// <summary>
  /// Runtime status effect instance attached to a <see cref="CombatUnit"/>.
  /// Implements <see cref="IData"/> so it can JSON-roundtrip through saves if a game ever
  /// persists buffs across sessions. The serialized payload is the <see cref="DefinitionKey"/> +
  /// duration/level; the <see cref="StatusEffectSO"/> is resolved at access time via
  /// <see cref="StatusEffectResolver"/> against the registered <see cref="StatusEffectCatalog"/>.
  /// </summary>
  [System.Serializable]
  public class StatusEffect : IData
  {
    public static float INTERVAL_VISUAL = 0.1f;
    public static float INTERVAL_EXECUTE = 1f;

    public CatalogKey DefinitionKey;
    public float StartDuration;
    public int Level;

    [NonSerialized] private StatusEffectSO _definition;

    /// <summary>
    /// Resolved <see cref="StatusEffectSO"/> for this effect. Lazily looked up via
    /// <see cref="StatusEffectResolver"/> on first access.
    /// </summary>
    public StatusEffectSO Definition
    {
      get
      {
        if (_definition == null && !DefinitionKey.IsEmpty)
        {
          _definition = StatusEffectResolver.GetOrNull(DefinitionKey.Key);
        }
        return _definition;
      }
    }

    public event Action<StatusEffect> OnEnd;
    private CancellationToken _skillCancelToken;
    private CountdownTimeContext _countdown;
    public CountdownTimeContext Countdown => _countdown;
    public float Duration
    {
      get
      {
        return _countdown.Value;
      }
      set
      {
        _countdown.Value = value;
      }
    }
    private float _timeSinceLastExecute = 0;

    public StatusEffect() { }

    public StatusEffect(StatusEffectSO definition, float duration, int level, CancellationToken skillCancelToken)
    {
      _definition = definition;
      DefinitionKey = new CatalogKey
      {
        Catalog = CombatConstants.StatusEffectsCatalogId,
        Key = definition != null ? definition.name : null,
      };
      StartDuration = duration;
      _countdown = new CountdownTimeContext(TimeManager.Instance.Global, StartDuration, 0, INTERVAL_VISUAL, skillCancelToken);
      Level = level;
      _skillCancelToken = skillCancelToken;
    }

    public void StartExecuteLoop(ICombatSettings combatSettings,
      ICombatManager combatManager, CombatUnit caster)
    {
      _countdown.Dispose();

      Definition.OnStart(combatSettings, combatManager, caster, caster, Level);

      _timeSinceLastExecute = 0;

      _countdown = new CountdownTimeContext(caster.TimeBundle.TimeContext, StartDuration, 0, INTERVAL_VISUAL, _skillCancelToken, false);
      _countdown.OnTick += (interval) =>
      {
        OnTick(interval, combatSettings, combatManager, caster);
      };
      _countdown.OnComplete += () =>
      {
        OnComplete(combatSettings, combatManager, caster);
      };
      _countdown.Start();
    }

    private void OnTick(float interval, ICombatSettings combatSettings,
      ICombatManager combatManager, CombatUnit caster)
    {
      _timeSinceLastExecute += interval;

      if (_timeSinceLastExecute >= INTERVAL_EXECUTE)
      {
        _timeSinceLastExecute = 0;
        Definition.OnTick(combatSettings, combatManager, caster, caster, Level);
      }
    }

    private void OnComplete(ICombatSettings combatSettings,
      ICombatManager combatManager, CombatUnit caster)
    {
      Definition.OnEnd(combatSettings, combatManager, caster, caster, Level);
      OnEnd?.Invoke(this);

      Dispose();
    }

    public UIColor GetColor()
    {
      return Definition.Color;
    }

    public void Dispose()
    {
      _countdown.Dispose();

      if (Definition != null && Definition.VFXBundle != null)
      {
        Definition.VFXBundle.Dispose();
      }
    }

    // ── IData ────────────────────────────────────────────────────

    public bool Validate() => true;

    public void OnAfterDeserialize() { }

    public IData CloneData()
    {
      StatusEffect clone = new StatusEffect(Definition, StartDuration, Level, CancellationToken.None);
      if (_countdown != null && clone._countdown != null)
      {
        clone._countdown.Value = _countdown.Value;
      }
      return clone;
    }

    public virtual UniTask PlayVFX(GameObject parent, Vector3 position, Quaternion rotation, CancellationToken? ct, TimeBundle timeBundle, RenderFeatureManager renderFeatureManager)
    {
      if (Definition.VFXBundle != null)
      {
        return Definition.VFXBundle.Play(parent, position, rotation, ct, timeBundle, renderFeatureManager, true);
      }

      return UniTask.CompletedTask;
    }
  }
}
