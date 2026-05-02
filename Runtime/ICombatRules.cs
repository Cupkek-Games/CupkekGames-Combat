using System.Collections.Generic;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
    public interface ICombatRules
    {
        // Core combat formula inputs
        int AttackSpeedBase { get; }
        float BaseCritDmg { get; }

        // Element System
        ElementRelationshipTableSO ElementRelationshipTable { get; }

        // Attribute Registry
        CombatAttributeRegistrySO Attributes { get; }

        // Attribute Display
        AttributeDisplayConfigSO AttributeDisplayConfig { get; }

        // Damage modifiers (empty by default — games add custom modifiers)
        IReadOnlyList<IDamageModifier> DamageModifiers { get; }

        // Methods
        float GetDefenseReduction(float totalDefense);
        float GetBaseValue(AttributeDefinitionSO attribute);
    }
}
