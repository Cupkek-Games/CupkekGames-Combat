using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.Units;

namespace CupkekGames.Combat
{
  public class CombatReportStats
  {
    private Dictionary<Guid, CombatReportStat> _unitToReport = new();
    public Dictionary<Guid, CombatReportStat> UnitToReport => _unitToReport;
    private Dictionary<Guid, UnitDefinitionSO> _unitToData = new();
    public Dictionary<Guid, UnitDefinitionSO> UnitToData => _unitToData;
    private Dictionary<Guid, int> _unitToTeam = new();
    public Dictionary<Guid, int> UnitToTeam => _unitToTeam;

    private CombatReportStat _highest = new();
    public CombatReportStat Highest => _highest;

    public void Register(EventDatabaseCombat eventDatabaseCombat)
    {
      eventDatabaseCombat.OnUnitSpawned += OnUnitSpawned;
    }

    public void Unregister(EventDatabaseCombat eventDatabaseCombat)
    {
      eventDatabaseCombat.OnUnitSpawned -= OnUnitSpawned;
    }

    private void OnUnitSpawned(Vector2Int pos, CombatUnit unit)
    {
      RegisterCombatUnit(unit);
    }

    private void RegisterCombatUnit(CombatUnit combatUnit)
    {
      combatUnit.OnDeathEvent += OnDeath;
      combatUnit.Health.OnTakeDamage += OnTakeDamage;
      combatUnit.Health.OnHeal += OnHeal;

      if (!_unitToData.ContainsKey(combatUnit.ID))
      {
        _unitToData.Add(combatUnit.ID, combatUnit.Data);
      }
      if (!_unitToTeam.ContainsKey(combatUnit.ID))
      {
        _unitToTeam.Add(combatUnit.ID, combatUnit.TeamId);
      }
      if (!_unitToReport.ContainsKey(combatUnit.ID))
      {
        _unitToReport.Add(combatUnit.ID, new CombatReportStat());
      }
    }

    private CombatReportStat GetCombatReportStat(CombatUnit combatUnit)
    {
      if (combatUnit == null)
      {
        return null;
      }

      Guid id = combatUnit.ID;

      if (_unitToReport.ContainsKey(id))
      {
        return _unitToReport[id];
      }
      else
      {
        CombatReportStat combatReportStat = new();
        _unitToReport[id] = combatReportStat;
        return combatReportStat;
      }
    }

    public void OnDamageDeal(CombatUnit combatUnit, int amount)
    {
      CombatReportStat combatReportStat = GetCombatReportStat(combatUnit);

      if (combatReportStat == null) return;

      combatReportStat.DamageDeal += amount;

      if (combatReportStat.DamageDeal > _highest.DamageDeal)
      {
        _highest.DamageDeal = combatReportStat.DamageDeal;
      }
    }

    public void OnDamageTake(CombatUnit combatUnit, int amount)
    {
      CombatReportStat combatReportStat = GetCombatReportStat(combatUnit);

      if (combatReportStat == null) return;

      combatReportStat.DamageTaken += amount;

      if (combatReportStat.DamageTaken > _highest.DamageTaken)
      {
        _highest.DamageTaken = combatReportStat.DamageTaken;
      }
    }

    public void OnHeal(CombatUnit combatUnit, int amount)
    {
      CombatReportStat combatReportStat = GetCombatReportStat(combatUnit);

      if (combatReportStat == null) return;

      combatReportStat.Heal += amount;

      if (combatReportStat.Heal > _highest.Heal)
      {
        _highest.Heal = combatReportStat.Heal;
      }
    }

    private void OnDeath(CombatUnit combatUnit)
    {
      combatUnit.OnDeathEvent -= OnDeath;
      combatUnit.Health.OnTakeDamage -= OnTakeDamage;
      combatUnit.Health.OnHeal -= OnHeal;
    }

    private void OnTakeDamage(CombatUnit defender, int amount, CombatUnit damager)
    {
      OnDamageDeal(damager, amount);
      OnDamageTake(defender, amount);
    }

    private void OnHeal(int amount, CombatUnit healer)
    {
      OnHeal(healer, amount);
    }

    public Guid? GetHighestDamagerGuid()
    {
      Guid? highestGuid = null;
      int maxDamage = int.MinValue;
      foreach (var pair in _unitToReport)
      {
        if (pair.Value.DamageDeal > maxDamage)
        {
          maxDamage = pair.Value.DamageDeal;
          highestGuid = pair.Key;
        }
      }
      return highestGuid;
    }

    public Guid? GetHighestTankerGuid()
    {
      Guid? highestGuid = null;
      int maxTaken = int.MinValue;
      foreach (var pair in _unitToReport)
      {
        if (pair.Value.DamageTaken > maxTaken)
        {
          maxTaken = pair.Value.DamageTaken;
          highestGuid = pair.Key;
        }
      }
      return highestGuid;
    }

    public Guid? GetHighestHealerGuid()
    {
      Guid? highestGuid = null;
      int maxHeal = int.MinValue;
      foreach (var pair in _unitToReport)
      {
        if (pair.Value.Heal > maxHeal)
        {
          maxHeal = pair.Value.Heal;
          highestGuid = pair.Key;
        }
      }
      return highestGuid;
    }
  }
}