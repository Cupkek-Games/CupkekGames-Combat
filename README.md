# CupkekGames Combat

Turn-based combat framework. Composes `Unit` + `IUnitFeatureDefinition` (from `com.cupkekgames.units`) into a combat-ready `CombatUnit` with health/mana/buffs/shield/AI sub-systems and a behaviour-tree-driven action pipeline.

## What's inside

**Runtime** (`CupkekGames.Combat.asmdef`)

- `CombatUnit` (+ `CombatUnitView`) — combat-aware wrapper around `Unit`, with `CombatUnitHealth`, `CombatUnitMana`, `CombatUnitBuffSystem`, `CombatUnitShield`, `CombatUnitPowerLevel`, `CombatUnitAI`, `CombatUnitThreatTable`.
- `CombatAttributesDefinition` — `IUnitFeatureDefinition` carrying element/damageType/baseAttributes/levelScaling/tier/actionSlots.
- `CombatAttributeRegistrySO` — per-game attribute role registry (HP/MP/ATK/MATK/DEF/MDEF/SPEED/CritChance/CritDmg slots wired to game's own `AttributeDefinitionSO` assets).
- `CombatAttributeScaling`, `CombatAttributeScalingTierSO`, `CombatUnitTierSO` — per-attribute level scaling + tier multipliers.
- `CombatActionSO` + `CombatActionRunner` + behaviour-tree action nodes (`CombatActionNodeDamage`, `_Heal`, `_Shield`, `_StatusEffect`, `_Projectile`, `_Indicator`, `_WithTarget`).
- Target selection: `CombatTargetSelectionPrimaryTarget`, `_AreaCircle`, `_AreaLine`, `_AreaArc`.
- Status effects: `StatusEffectSO`, `StatusEffectController`, behaviours (DOT/HOT/Disable/...).
- Projectiles: `Projectile`, `ProjectileCollisionHandler`.
- Damage pipeline: `CombatDamageCalculator`, `IDamageModifier` (consumer-extensible).
- `ICombatRules` / `ICombatSettings` — extension contracts for game-specific damage formulas + attack-speed config.
- `IPowerLevelCalculator` — pluggable power-level estimation.
- Fight events: every hit goes through `CombatUnitHealth.TakeDamage(in CombatHit)`, which records `CombatUnit.LastHit` (a death's killer) and raises `OnHit` before the death; `CombatSource` names where a hit came from (a fresh instance per action run, kept by a projectile from its launch); `EventDatabaseCombat.OnHit` and `OnUltimate` (Selected, Completed, Cancelled).
- Unit seams: `CombatUnit.SkillRank` (0 follows the level), `IAttackElementModifier` (the element a unit attacks in), `IShieldCastModifier` (the shields a unit casts), `CombatUnitMana.Drain`, `CombatUnitAI.CancelSelectedUltimate` (a stun or a silence drops a selected ultimate), threat only from actions.
- `CombatValueScaling` on damage and heal nodes (the caster's attribute, or a share of each target's max health) and exact damage (no crit, no defense).

**Editor** (`CupkekGames.Combat.Editor.asmdef`)

- Custom inspectors / drawers for combat assets.

## Dependencies

Asmdef references resolve via the CupkekGames scoped registry: `units`, `character`, `data`, `services`, `rpgstats`, `behaviourtrees`, `pool`, `addressableassets`, `audio`, `textpopup`, `vfx`, `shapedrawing`, `timesystem`, `transforms`, `keyvaluedatabases`, `fadeables`. Bring your own copy via the registry.
