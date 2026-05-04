using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
    /// <summary>Per-attribute value pair, keyed by stable <see cref="AttributeDefinitionSO"/> name.</summary>
    [Serializable]
    public class AttributeValueEntry
    {
        public string AttributeKey;
        public float Value;
    }

    /// <summary>
    /// Combat attribute snapshot keyed by stable <see cref="AttributeDefinitionSO"/> name (catalog key).
    /// Replaces the old positional <c>List&lt;float&gt;</c> shape so reordering the attribute registry
    /// doesn't corrupt saved snapshots.
    /// </summary>
    [Serializable]
    public class AttributeData
    {
        [SerializeField] private List<AttributeValueEntry> _values = new();
        public IReadOnlyList<AttributeValueEntry> Values => _values;
        public int Count => _values.Count;

        public AttributeData() { }

        public AttributeData(AttributeData other)
        {
            if (other?._values == null) return;
            foreach (AttributeValueEntry e in other._values)
                _values.Add(new AttributeValueEntry { AttributeKey = e.AttributeKey, Value = e.Value });
        }

        public float GetValue(string attributeKey)
        {
            if (string.IsNullOrEmpty(attributeKey)) return 0f;
            for (int i = 0; i < _values.Count; i++)
            {
                if (_values[i].AttributeKey == attributeKey) return _values[i].Value;
            }
            return 0f;
        }

        public float GetValue(AttributeDefinitionSO attribute) =>
            attribute != null ? GetValue(attribute.name) : 0f;

        public void SetValue(string attributeKey, float value)
        {
            if (string.IsNullOrEmpty(attributeKey)) return;
            for (int i = 0; i < _values.Count; i++)
            {
                if (_values[i].AttributeKey == attributeKey)
                {
                    _values[i].Value = value;
                    return;
                }
            }
            _values.Add(new AttributeValueEntry { AttributeKey = attributeKey, Value = value });
        }

        public void SetValue(AttributeDefinitionSO attribute, float value)
        {
            if (attribute != null) SetValue(attribute.name, value);
        }

        public void Clear() => _values.Clear();
    }
}
