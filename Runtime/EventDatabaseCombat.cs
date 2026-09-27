using System;
using UnityEngine;
using CupkekGames.Combat;

namespace CupkekGames.Combat
{
  public class EventDatabaseCombat : MonoBehaviour
  {
    // State-machine events
    public event Action SetupEvent;
    public event Action ContinueEvent;
    public event Action PauseEvent;
    public event Action WaveClearEvent;
    public event Action NextWaveEvent;
    public event Action WinEvent;
    public event Action LoseEvent;

    // Unit lifecycle events (unified — consumers check CombatUnit.TeamId)
    public event Action<Vector2Int, CombatUnit> OnUnitSpawned;
    public event Action<CombatUnit> OnUnitDeath;
    public event Action<CombatUnit, CombatUnit, float> OnCriticalHitEvent;

    /// <summary>
    /// A hit landed on a unit announced by <see cref="InvokeOnUnitSpawned"/>,
    /// raised before the death it causes (the death's handlers read
    /// <see cref="CombatUnit.LastHit"/> for the killer).
    /// </summary>
    public event Action<CombatUnit, CombatHit> OnHit;

    /// <summary>An ultimate was selected, then completed or cancelled (once each).</summary>
    public event Action<CombatUnit, CombatActionSO, UltimatePhase> OnUltimate;

    // Invoke helpers — only the declaring class may invoke an event
    public void InvokeSetupEvent() => SetupEvent?.Invoke();
    public void InvokeContinueEvent() => ContinueEvent?.Invoke();
    public void InvokePauseEvent() => PauseEvent?.Invoke();
    public void InvokeWaveClearEvent() => WaveClearEvent?.Invoke();
    public void InvokeNextWaveEvent() => NextWaveEvent?.Invoke();
    public void InvokeWinEvent() => WinEvent?.Invoke();
    public void InvokeLoseEvent() => LoseEvent?.Invoke();
    public void InvokeOnUnitSpawned(Vector2Int pos, CombatUnit unit)
    {
      // Every unit in the fight reports its hits here: one stream for the fight's log.
      unit.Health.OnHit -= RaiseHit;
      unit.Health.OnHit += RaiseHit;
      OnUnitSpawned?.Invoke(pos, unit);
    }

    public void InvokeOnUltimate(CombatUnit caster, CombatActionSO action, UltimatePhase phase) => OnUltimate?.Invoke(caster, action, phase);

    private void RaiseHit(CombatUnit unit, CombatHit hit) => OnHit?.Invoke(unit, hit);
    public void InvokeOnUnitDeath(CombatUnit unit) => OnUnitDeath?.Invoke(unit);
    public void InvokeOnCriticalHitEvent(CombatUnit attacker, CombatUnit defender, float damage) => OnCriticalHitEvent?.Invoke(attacker, defender, damage);
  }
}