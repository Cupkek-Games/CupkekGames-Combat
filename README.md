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

**Editor** (`CupkekGames.Combat.Editor.asmdef`)

- Custom inspectors / drawers for combat assets.

## Dependencies

Asmdef references resolve via the CupkekGames scoped registry: `units`, `character`, `data`, `services`, `rpgstats`, `behaviourtrees`, `pool`, `addressableassets`, `audio`, `textpopup`, `vfx`, `shapedrawing`, `timesystem`, `transforms`, `keyvaluedatabases`, `fadeables`. Bring your own copy via the registry.
