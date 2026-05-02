using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
  public class CombatUnitThreatTable
  {
    // Dictionary to store units and their corresponding threat levels
    private readonly Dictionary<CombatUnit, int> _threatTable = new Dictionary<CombatUnit, int>();

    // Method to add or update a unit's threat in the threat table
    public void AddThreat(CombatUnit target, int threatAmount)
    {
      if (target == null) return;

      if (_threatTable.ContainsKey(target))
      {
        // Increase the existing threat level
        _threatTable[target] += threatAmount;
      }
      else
      {
        // Add the unit with the initial threat amount
        _threatTable[target] = threatAmount;
        target.OnDeathEvent += OnUnitDeath;
      }
    }

    // Method to get the unit with the highest threat level
    public CombatUnit GetHighestThreatTarget()
    {
      CombatUnit highestThreatUnit = null;
      float highestThreat = float.MinValue;

      foreach (var entry in _threatTable)
      {
        if (entry.Value > highestThreat && entry.Key.CombatUnitGameObject != null)
        {
          highestThreat = entry.Value;
          highestThreatUnit = entry.Key;
        }
      }

      return highestThreatUnit;
    }

    // Method to reset the threat table (e.g., at the start of a new combat)
    public void ResetThreatTable()
    {
      _threatTable.Clear();
    }

    // Method to print the threat table for debugging
    public void PrintThreatTable()
    {
      foreach (var entry in _threatTable)
      {
        Debug.Log($"Unit: {entry.Key.ID}, Threat: {entry.Value}");
      }
    }

    private void OnUnitDeath(CombatUnit unit)
    {
      _threatTable.Remove(unit);
    }
  }
}