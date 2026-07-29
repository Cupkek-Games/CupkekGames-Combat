using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
  [Serializable]
  public class CombatWave
  {
    [Serializable]
    public class Entry
    {
      [SerializeField] private Vector2Int _position;
      [SerializeField] private CombatUnitReference _unit;

      public Vector2Int Position { get => _position; private set => _position = value; }
      public CombatUnitReference Unit { get => _unit; private set => _unit = value; }

      public Entry() { }

      public Entry(Vector2Int position, CombatUnitReference unit)
      {
        _position = position;
        _unit = unit;
      }
    }

    // Source of truth: pair list, so BOTH serializers see the wave (Unity can't
    // serialize dictionaries; the old Dictionary field was invisible in the
    // inspector). The dictionary below is a lazy runtime cache only.
    [SerializeField] private List<Entry> _entries = new();

    [NonSerialized] private Dictionary<Vector2Int, CombatUnitReference> _cache;

    public List<Entry> Entries
    {
      get => _entries;
      private set
      {
        _entries = value ?? new List<Entry>();
        _cache = null;
      }
    }

    private Dictionary<Vector2Int, CombatUnitReference> Cache
    {
      get
      {
        if (_cache == null)
        {
          _cache = new Dictionary<Vector2Int, CombatUnitReference>(_entries.Count);
          foreach (Entry entry in _entries)
            _cache.TryAdd(entry.Position, entry.Unit);
        }
        return _cache;
      }
    }

    public void Add(Vector2Int position, CombatUnitReference unit)
    {
      Cache.Add(position, unit);
      _entries.Add(new Entry(position, unit));
    }

    public bool ContainsKey(Vector2Int position) => Cache.ContainsKey(position);

    public CombatUnitReference GetUnit(Vector2Int position) => Cache[position];

    public bool TryGetUnit(Vector2Int position, out CombatUnitReference unit) =>
      Cache.TryGetValue(position, out unit);

    public CombatWave() { }

    public CombatWave(CombatWave other)
    {
      if (other?._entries == null)
        return;
      foreach (Entry entry in other._entries)
        _entries.Add(new Entry(entry.Position, entry.Unit != null ? new CombatUnitReference(entry.Unit) : null));
    }
  }
}
