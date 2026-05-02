using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
  [Serializable]
  public class CombatAttributeScalingEntry
  {
    public AttributeDefinitionSO Attribute;
    public CombatAttributeScalingTierSO Scaling;
  }

  [Serializable]
  public class CombatAttributeScaling
  {
    [SerializeField] private List<CombatAttributeScalingEntry> _entries = new();

    [NonSerialized] private Dictionary<AttributeDefinitionSO, CombatAttributeScalingTierSO> _lookup;

    public float GetStatMultiplier(AttributeDefinitionSO attribute, int level, float tierMultiplier)
    {
      EnsureLookup();
      if (_lookup.TryGetValue(attribute, out var tier) && tier != null)
        return tier.GetMultiplier(level, tierMultiplier);
      return 1f;
    }

    private void EnsureLookup()
    {
      if (_lookup == null)
      {
        _lookup = new Dictionary<AttributeDefinitionSO, CombatAttributeScalingTierSO>();
        foreach (var entry in _entries)
        {
          if (entry.Attribute != null)
            _lookup[entry.Attribute] = entry.Scaling;
        }
      }
    }
  }
}