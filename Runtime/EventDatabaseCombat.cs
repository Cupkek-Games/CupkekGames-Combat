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

    // Invoke helpers — only the declaring class may invoke an event
    public void InvokeSetupEvent() => SetupEvent?.Invoke();
    public void InvokeContinueEvent() => ContinueEvent?.Invoke();
    public void InvokePauseEvent() => PauseEvent?.Invoke();
    public void InvokeWaveClearEvent() => WaveClearEvent?.Invoke();
    public void InvokeNextWaveEvent() => NextWaveEvent?.Invoke();
    public void InvokeWinEvent() => WinEvent?.Invoke();
    public void InvokeLoseEvent() => LoseEvent?.Invoke();
    public void InvokeOnUnitSpawned(Vector2Int pos, CombatUnit unit) => OnUnitSpawned?.Invoke(pos, unit);
    public void InvokeOnUnitDeath(CombatUnit unit) => OnUnitDeath?.Invoke(unit);
    public void InvokeOnCriticalHitEvent(CombatUnit attacker, CombatUnit defender, float damage) => OnCriticalHitEvent?.Invoke(attacker, defender, damage);
  }
}