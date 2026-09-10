using System.Collections.Generic;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
    public interface ICombatRules
    {
        // Core combat formula inputs
        int AttackSpeedBase { get; }

        // Element System
        ElementRelationshipTableSO ElementRelationshipTable { get; }

        // Attribute Registry
        CombatAttributeRegistrySO Attributes { get; }

        // Attribute Display
        AttributeDisplayConfigSO AttributeDisplayConfig { get; }

        // Skill description colours, inline icons, number format
        CombatDescriptionStyleSO DescriptionStyle { get; }

        // Damage modifiers (empty by default — games add custom modifiers)
        IReadOnlyList<IDamageModifier> DamageModifiers { get; }

        // Methods

        /// <summary>
        /// Fraction of incoming damage that gets through at the given defense
        /// value: 1 at defense 0, approaching 0 as defense grows. Final damage
        /// is <c>attack * thisValue</c>. Renamed from GetDefenseReduction in
        /// 0.4.1 — the damage path always consumed it as the damage-THROUGH
        /// multiplier, and the old name inverted its meaning.
        /// </summary>
        float GetDamageTakenMultiplier(float totalDefense);

        float GetBaseValue(AttributeDefinitionSO attribute);
    }
}
