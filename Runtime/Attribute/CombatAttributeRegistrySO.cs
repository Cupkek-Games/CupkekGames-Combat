using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.RPGStats;
using CupkekGames.InventorySystem;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Per-game registry that names which <see cref="AttributeDefinitionSO"/> plays each combat role.
    /// <para>
    /// The consuming game creates <see cref="AttributeDefinitionSO"/> assets for its own stats (with custom
    /// <c>DisplayName</c>, <c>Icon</c>, <c>Beautify</c>) and wires each one into the role slot below that
    /// matches its purpose. The Combat package's formulas (Health, Mana, damage, defense, crit, turn order)
    /// consult roles by slot, never by name — so a game can call its HP stat "Vitality" and its SPEED stat
    /// "Initiative" without changing any framework code.
    /// </para>
    /// <para>
    /// <b>Role contract:</b>
    /// </para>
    /// <list type="bullet">
    ///   <item><description><c>HP</c> and <c>SPEED</c> are <b>required</b> — Health and turn-order systems will null-ref without them.</description></item>
    ///   <item><description>Every other role is <b>optional</b> — formulas null-check each slot and skip the contribution if unwired.</description></item>
    /// </list>
    /// <para>
    /// Extra attributes that aren't part of the standard combat role contract (e.g. Stamina, Resolve)
    /// can still be added to <see cref="All"/> for display/snapshots; formulas will simply ignore them.
    /// </para>
    /// </summary>
    [CreateAssetMenu(fileName = "CombatAttributeRegistry", menuName = "CupkekGames/Combat/Attribute Registry")]
    public class CombatAttributeRegistrySO : ScriptableObject
    {
        [SerializeField] private List<AttributeDefinitionSO> _attributes = new();

        [Header("Combat Role Slots — wire your game's AttributeDefinitionSO into each role")]
        [SerializeField] private AttributeDefinitionSO _hp;
        [SerializeField] private AttributeDefinitionSO _mp;
        [SerializeField] private AttributeDefinitionSO _atk;
        [SerializeField] private AttributeDefinitionSO _matk;
        [SerializeField] private AttributeDefinitionSO _def;
        [SerializeField] private AttributeDefinitionSO _mdef;
        [SerializeField] private AttributeDefinitionSO _speed;
        [SerializeField] private AttributeDefinitionSO _critChance;
        [SerializeField] private AttributeDefinitionSO _critDmg;

        /// <summary>All attributes the game uses (drives display, snapshots, beautify lists). Includes role-mapped and unmapped entries alike.</summary>
        public IReadOnlyList<AttributeDefinitionSO> All => _attributes;

        /// <summary>Health pool. <b>Required.</b> Read by <c>CombatUnitHealth</c>, <c>CombatUnitBuffSystem</c>, DOT behaviors, and defense formulas.</summary>
        public AttributeDefinitionSO HP => _hp;
        /// <summary>Mana / resource pool. Optional — <c>CombatUnitMana</c> stays inert if null; power-level formula skips the mana multiplier.</summary>
        public AttributeDefinitionSO MP => _mp;
        /// <summary>Physical attack power. Optional — power-level formula contributes 0 if null.</summary>
        public AttributeDefinitionSO ATK => _atk;
        /// <summary>Magical attack power. Optional — power-level formula contributes 0 if null.</summary>
        public AttributeDefinitionSO MATK => _matk;
        /// <summary>Physical defense. Optional — defense-rating formula skips the physical contribution if null.</summary>
        public AttributeDefinitionSO DEF => _def;
        /// <summary>Magical defense. Optional — defense-rating formula skips the magical contribution if null.</summary>
        public AttributeDefinitionSO MDEF => _mdef;
        /// <summary>Turn / action speed. <b>Required.</b> Read by <c>CombatUnit</c> for turn cooldown.</summary>
        public AttributeDefinitionSO SPEED => _speed;
        /// <summary>Critical-hit roll chance (0–1). Optional — crits never fire if null.</summary>
        public AttributeDefinitionSO CritChance => _critChance;
        /// <summary>Critical-hit damage multiplier. Optional — crits use 1× damage if null.</summary>
        public AttributeDefinitionSO CritDmg => _critDmg;

        public List<string> GetAttributeNames()
        {
            var result = new List<string>(_attributes.Count);
            foreach (var attr in _attributes)
            {
                result.Add(attr.DisplayName);
            }
            return result;
        }

        public List<Sprite> GetAttributeIcons()
        {
            var result = new List<Sprite>(_attributes.Count);
            foreach (var attr in _attributes)
            {
                result.Add(attr.Icon);
            }
            return result;
        }

        public List<Func<float, string>> GetBeautifyList()
        {
            var result = new List<Func<float, string>>(_attributes.Count);
            foreach (var attr in _attributes)
            {
                var a = attr;
                result.Add(value => a.Beautify(value));
            }
            return result;
        }

        /// <summary>Snapshot with zeroed values for every registered combat attribute.</summary>
        public AttributeSet GetAttributeSetEmpty()
        {
            var result = new AttributeSet();
            foreach (AttributeDefinitionSO attr in _attributes)
            {
                result.SetValue(attr, 0f);
            }

            return result;
        }
    }
}
