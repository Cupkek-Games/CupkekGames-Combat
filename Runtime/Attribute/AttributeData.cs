using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
  /// <summary>
  /// Ordered combat attribute snapshot (indices align with <see cref="CombatAttributeRegistrySO.All"/>).
  /// </summary>
  [Serializable]
  public class AttributeData
  {
    [SerializeField] private List<float> _values = new();

    public AttributeData() { }

    public AttributeData(AttributeData other)
    {
      if (other?._values != null)
        _values = new List<float>(other._values);
    }

    public int Count => _values.Count;

    public float GetValue(int index)
    {
      if (index < 0 || index >= _values.Count)
        return 0f;
      return _values[index];
    }

    public void AddValue(float value) => _values.Add(value);

    public void Clear() => _values.Clear();

    public void EnsureCount(int count)
    {
      while (_values.Count < count)
        _values.Add(0f);
      if (_values.Count > count)
        _values.RemoveRange(count, _values.Count - count);
    }

    public void SetValue(int index, float value)
    {
      if (index < 0)
        return;
      while (_values.Count <= index)
        _values.Add(0f);
      _values[index] = value;
    }
  }
}
