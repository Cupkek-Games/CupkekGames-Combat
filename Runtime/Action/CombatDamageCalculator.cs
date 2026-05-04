using System.Collections.Generic;
using UnityEngine;
using CupkekGames.RPGStats;
using CupkekGames.TextPopup;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Centralised combat math that was previously duplicated across
    /// <see cref="CombatActionNodeDamage"/>, <see cref="CombatActionNodeHeal"/>,
    /// <see cref="CombatActionNodeShield"/>, and <see cref="CombatActionSO.AttackTarget"/>.
    /// </summary>
    public static class CombatDamageCalculator
    {
        /// <summary>
        /// Resolves a <see cref="AttributeModifier"/> against a caster's attack attribute.
        /// Shared by Damage, Heal, and Shield nodes.
        /// </summary>
        /// <returns><c>modifier.Flat + caster.GetAttributeValue(attackAttribute) * modifier.Multiplier</c></returns>
        public static float CalculateScaledValue(CombatUnit caster, AttributeModifier modifier, AttributeDefinitionSO attackAttribute)
        {
            return modifier.Flat + caster.GetAttributeValue(attackAttribute) * modifier.Multiplier;
        }

        /// <summary>
        /// The portion of <see cref="CalculateScaledValue"/> that comes from the caster's attribute.
        /// Useful for description text that shows "base + addition".
        /// </summary>
        public static float CalculateAttributeAddition(CombatUnit caster, AttributeModifier modifier, AttributeDefinitionSO attackAttribute)
        {
            return caster.GetAttributeValue(attackAttribute) * modifier.Multiplier;
        }

        /// <summary>
        /// Pure-math portion of dealing damage: applies crit, element, defense, and
        /// user-provided <see cref="IDamageModifier"/> hooks in order.
        /// Returns the final integer damage and whether a crit occurred.
        /// Does NOT apply damage to the target or trigger any visual effects.
        /// </summary>
        public static DamageResult CalculateAttackDamage(
          ICombatRules combatRules,
          CombatUnit attacker,
          CombatUnit target,
          float rawAttack,
          DamageTypeDefinitionSO damageType)
        {
            bool isCrit = attacker.PowerLevel.TryCritical();
            float attack = isCrit ? attacker.PowerLevel.ApplyCritical(rawAttack) : rawAttack;

            float elementMultiplier = combatRules.ElementRelationshipTable.GetMultiplier(
                attacker.CombatData?.Element, target.CombatData?.Element);
            attack *= elementMultiplier;

            float defense = target.GetAttributeValue(damageType.DefenseAttribute);
            float reduction = combatRules.GetDefenseReduction(defense);

            DamageContext ctx = new DamageContext
            {
                IsCrit = isCrit,
                ElementMultiplier = elementMultiplier,
                DefenseReduction = reduction,
                DamageType = damageType,
            };

            // Apply raw damage modifiers (before defense)
            IReadOnlyList<IDamageModifier> modifiers = combatRules.DamageModifiers;
            if (modifiers != null)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    attack = modifiers[i].ModifyRawDamage(attack, attacker, target, ctx);
                }
            }

            int damage = (int)(attack * reduction + 0.5f);

            // Apply final damage modifiers (after defense)
            if (modifiers != null)
            {
                for (int i = 0; i < modifiers.Count; i++)
                {
                    damage = modifiers[i].ModifyFinalDamage(damage, attacker, target, ctx);
                }
            }

            return new DamageResult
            {
                Damage = damage,
                IsCrit = isCrit,
                ElementMultiplier = elementMultiplier,
            };
        }

        /// <summary>
        /// Applies damage to the target and plays all associated visual and audio effects.
        /// Separated from <see cref="CalculateAttackDamage"/> for clean calc/visual split.
        /// </summary>
        public static void ApplyDamageAndVisuals(
          ICombatVisualSettings visualSettings,
          ICombatManager manager,
          CombatUnit attacker,
          CombatUnit target,
          DamageResult result)
        {
            target.Health.TakeDamage(result.Damage, attacker);

            CombatUnitView combatUnitGameObject = target.CombatUnitGameObject;
            Vector3 targetPos = combatUnitGameObject.HealthBarTransform.position;

            manager.PopupManager.Show(
                PopupKinds.DamageVariant(result.ElementMultiplier),
                targetPos,
                result.Damage,
                new DamagePopupContext { IsCrit = result.IsCrit });

            combatUnitGameObject.ShaderColorController
              .AddColor(visualSettings.HitColor, visualSettings.HitColorWeight, visualSettings.HitColorDurationMS).Forget();
            combatUnitGameObject.ShaderEmissionController
              .AddColor(visualSettings.HitColorEmission, visualSettings.HitColorWeight, visualSettings.HitColorDurationMS).Forget();

            combatUnitGameObject.TakeDamageSquashAndStretch(visualSettings.HitSquashAndStretchBumpAmount);

            if (result.IsCrit)
            {
                manager.EventDatabase.InvokeOnCriticalHitEvent(attacker, target, result.Damage);
                manager.PlayCriticalEffect(combatUnitGameObject.transform);
            }
        }
    }

    /// <summary>
    /// Result of pure damage calculation, before application to target.
    /// </summary>
    public struct DamageResult
    {
        public int Damage;
        public bool IsCrit;
        public float ElementMultiplier;
    }
}
