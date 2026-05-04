using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
    [Serializable]
    public class CombatRoleEntry
    {
        [Tooltip("Role key. Use CombatRoles constants for standard slots (HP/MP/ATK/MATK/DEF/MDEF/SPEED/CritChance/CritDmg). Game-specific roles can use any string.")]
        public string Role;

        [Tooltip("The AttributeDefinitionSO that plays this role for this game.")]
        public AttributeDefinitionSO Attribute;
    }

    /// <summary>
    /// Per-game registry that maps combat roles → <see cref="AttributeDefinitionSO"/> instances.
    /// Combat framework formulas look up roles by string key (see <see cref="CombatRoles"/> for reserved keys);
    /// game-specific roles are added by appending a new <see cref="CombatRoleEntry"/> in the inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatAttributeRegistry", menuName = "CupkekGames/Combat/Attribute Registry")]
    public class CombatAttributeRegistrySO : ScriptableObject
    {
        [Tooltip("All attributes the game uses. Drives display, snapshots, and beautify lists.")]
        [SerializeField] private List<AttributeDefinitionSO> _attributes = new();

        [Tooltip("Role → attribute mapping. Combat formulas look up by role string (see CombatRoles).")]
        [SerializeField] private List<CombatRoleEntry> _roles = new();

        /// <summary>All attributes the game uses (display ordering for beautify lists, etc.).</summary>
        public IReadOnlyList<AttributeDefinitionSO> All => _attributes;

        /// <summary>Get the attribute mapped to <paramref name="role"/>, or null if unmapped.</summary>
        public AttributeDefinitionSO GetByRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return null;
            for (int i = 0; i < _roles.Count; i++)
            {
                if (_roles[i].Role == role) return _roles[i].Attribute;
            }
            return null;
        }

        // Convenience accessors for reserved combat roles — delegate to GetByRole(string).

        public AttributeDefinitionSO HP => GetByRole(CombatRoles.HP);
        public AttributeDefinitionSO MP => GetByRole(CombatRoles.MP);
        public AttributeDefinitionSO ATK => GetByRole(CombatRoles.ATK);
        public AttributeDefinitionSO MATK => GetByRole(CombatRoles.MATK);
        public AttributeDefinitionSO DEF => GetByRole(CombatRoles.DEF);
        public AttributeDefinitionSO MDEF => GetByRole(CombatRoles.MDEF);
        public AttributeDefinitionSO SPEED => GetByRole(CombatRoles.SPEED);
        public AttributeDefinitionSO CritChance => GetByRole(CombatRoles.CritChance);
        public AttributeDefinitionSO CritDmg => GetByRole(CombatRoles.CritDmg);

        public List<string> GetAttributeNames()
        {
            var result = new List<string>(_attributes.Count);
            foreach (AttributeDefinitionSO attr in _attributes) result.Add(attr.DisplayName);
            return result;
        }

        public List<Sprite> GetAttributeIcons()
        {
            var result = new List<Sprite>(_attributes.Count);
            foreach (AttributeDefinitionSO attr in _attributes) result.Add(attr.Icon);
            return result;
        }

        public List<Func<float, string>> GetBeautifyList()
        {
            var result = new List<Func<float, string>>(_attributes.Count);
            foreach (AttributeDefinitionSO attr in _attributes)
            {
                AttributeDefinitionSO captured = attr;
                result.Add(value => captured.Beautify(value));
            }
            return result;
        }

        /// <summary>Snapshot with zeroed values for every registered combat attribute.</summary>
        public AttributeSet GetAttributeSetEmpty()
        {
            var result = new AttributeSet();
            foreach (AttributeDefinitionSO attr in _attributes) result.SetValue(attr, 0f);
            return result;
        }
    }
}
