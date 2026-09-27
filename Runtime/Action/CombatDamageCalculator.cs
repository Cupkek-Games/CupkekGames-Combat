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
        /// A damage or heal amount for one target: <c>Flat + base × Multiplier</c>,
        /// the base being the caster's <paramref name="attackAttribute"/> or, for
        /// <see cref="CombatValueScaling.TargetMaxHealth"/>, the target's max health.
        /// </summary>
        public static float CalculateScaledValue(CombatUnit caster, CombatUnit target, AttributeModifier modifier,
            CombatValueScaling scaling, AttributeDefinitionSO attackAttribute)
        {
            return scaling == CombatValueScaling.TargetMaxHealth
                ? modifier.Flat + target.GetAttributeValue(target.Attributes.HP) * modifier.Multiplier
                : CalculateScaledValue(caster, modifier, attackAttribute);
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
        /// Pure-math portion of dealing damage: applies crit, element (the
        /// attacker's attack element, <see cref="CombatUnit.GetAttackElement"/>),
        /// defense, and user-provided <see cref="IDamageModifier"/> hooks in order.
        /// Returns the final integer damage and whether a crit occurred.
        /// Does NOT apply damage to the target or trigger any visual effects.
        /// </summary>
        public static DamageResult CalculateAttackDamage(
          ICombatRules combatRules,
          CombatUnit attacker,
          CombatUnit target,
          float rawAttack,
          DamageTypeDefinitionSO damageType,
          CombatSource source)
        {
            if (source == null) throw new System.ArgumentNullException(nameof(source), "A hit needs its source.");

            bool isCrit = attacker.PowerLevel.TryCritical();
            float attack = isCrit ? attacker.PowerLevel.ApplyCritical(rawAttack) : rawAttack;

            float elementMultiplier = ElementMultiplier(combatRules, attacker, target);
            attack *= elementMultiplier;

            float defense = target.GetAttributeValue(damageType.DefenseAttribute);
            float damageTaken = combatRules.GetDamageTakenMultiplier(defense);

            DamageContext ctx = new DamageContext
            {
                IsCrit = isCrit,
                ElementMultiplier = elementMultiplier,
                DamageTakenMultiplier = damageTaken,
                DamageType = damageType,
                ActionSO = source.Action,
                Source = source,
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

            int damage = (int)(attack * damageTaken + 0.5f);

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
                Source = source,
            };
        }

        /// <summary>
        /// An exact amount: no crit, no defense and no damage modifiers; only the
        /// attacker's element counts (a share of max health, the elemental
        /// potion: chosen for the target's weakness and meant to be read off).
        /// </summary>
        public static DamageResult CalculateExactDamage(
          ICombatRules combatRules,
          CombatUnit attacker,
          CombatUnit target,
          float amount,
          CombatSource source)
        {
            if (source == null) throw new System.ArgumentNullException(nameof(source), "A hit needs its source.");

            float elementMultiplier = ElementMultiplier(combatRules, attacker, target);
            return new DamageResult
            {
                Damage = (int)(amount * elementMultiplier + 0.5f),
                IsCrit = false,
                ElementMultiplier = elementMultiplier,
                Source = source,
            };
        }

        private static float ElementMultiplier(ICombatRules combatRules, CombatUnit attacker, CombatUnit target)
            => combatRules.ElementRelationshipTable.GetMultiplier(attacker?.GetAttackElement(), target.CombatData?.Element);

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
            // A landing projectile can reach a unit that fell while it flew:
            // nothing is left to hurt or to show.
            if (target.Health.Current <= 0)
            {
                return;
            }

            target.Health.TakeDamage(new CombatHit(attacker, result.Source, result.Damage, result.IsCrit, result.ElementMultiplier));

            // A delayed hit (projectile in flight) can land after the target's
            // view is gone — died and was despawned/cleaned up meanwhile. The
            // damage state above still applies; there is nothing left to
            // visualize, and dereferencing the dead view was an every-battle
            // NRE surfacing as an unobserved UniTask exception (2026-08-16).
            CombatUnitView view = target.View;
            if (view == null)
            {
                return;
            }

            Vector3 targetPos = view.HealthBarTransform.position;

            manager.PopupManager.Show(
                PopupKinds.DamageVariant(result.ElementMultiplier),
                targetPos,
                result.Damage,
                new DamagePopupContext { IsCrit = result.IsCrit });

            view.ShaderColorController
              .AddColor(visualSettings.HitColor, visualSettings.HitColorWeight, visualSettings.HitColorDurationMS).Forget();
            view.ShaderEmissionController
              .AddColor(visualSettings.HitColorEmission, visualSettings.HitColorWeight, visualSettings.HitColorDurationMS).Forget();

            view.TakeDamageSquashAndStretch(visualSettings.HitSquashAndStretchBumpAmount);

            if (result.IsCrit)
            {
                manager.EventDatabase.InvokeOnCriticalHitEvent(attacker, target, result.Damage);
                manager.PlayCriticalEffect(view.transform);
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
        /// <summary>Where the damage comes from: the hit names it.</summary>
        public CombatSource Source;
    }
}
