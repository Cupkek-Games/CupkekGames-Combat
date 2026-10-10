# CupkekGames Combat

Turn-based combat framework. Composes `Unit` + `IUnitFeatureDefinition` (from `com.cupkekgames.units`) into a combat-ready `CombatUnit` with health/mana/buffs/shield/AI sub-systems and a behaviour-tree-driven action pipeline.

## What's inside

**Runtime** (`CupkekGames.Combat.asmdef`)

- `CombatUnit` (+ `CombatUnitView`) — combat-aware wrapper around `Unit`, with `CombatUnitHealth`, `CombatUnitMana`, `CombatUnitBuffSystem`, `CombatUnitShield`, `CombatUnitPowerLevel`, `CombatUnitAI`, `CombatUnitThreatTable`.
- `CombatAttributesDefinition` — `IUnitFeatureDefinition` carrying element/damageType/baseAttributes/levelScaling/tier/actionSlots.
- `CombatAttributeRegistrySO` — per-game attribute role registry (HP/MP/ATK/MATK/DEF/MDEF/SPEED/CritChance/CritDmg/Evasion/DamageDealt/DamageTaken slots wired to game's own `AttributeDefinitionSO` assets).
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
- Seams a game's run builds on (0.8.0), each a feature interface folded like `IAttributeModifier`, so units opt in by composition:
  - damage: `IDamageDealtModifier`, `IDamageTakenModifier` and the roles `DamageDealt` and `DamageTaken` (a share, read as `1 + value`), before defense; exact damage skips them;
  - crits: `ICritModifier` (`CombatCritPolicy` Roll, Always, Never; a call's policy wins, then Never over Always), `CombatUnit.TryCritical` and `ApplyCritical` (out of `IPowerLevelCalculator`);
  - chances: `CombatUnit.RollChance` on the fight's random, rolled twice for a lucky unit (`IChanceRollModifier`);
  - dodge: the `Evasion` role; an action's hit can miss (`DamageResult.IsMiss`, the `CombatPopupKinds.Miss` popup, `EventDatabaseCombat.OnMiss`);
  - statuses: `StatusStacking` (Level or Additive up to `MaxStacks`), the longer time kept on a level-up too, `IStatusImmunity`, and `IControlImmunity` for the controls a status carries (`StatusEffectSO.Controls`) and pushes;
  - controls: stuns, silences and roots counted on the unit (`CombatUnit.Stun`, `Silence`, `Root` and their ends), so overlaps hold until the last ends;
  - shields: a node knows its `Caster`, `Owner`, `Granted`, `Absorbed` and `EndReason` (Broken, Expired, OwnerGone); `EventDatabaseCombat.OnShieldEnded`;
  - push: `ICombatSpace.Push` and `CombatPush` (immunity, `EventDatabaseCombat.OnPushed`);
  - areas: `ICombatSpace.CollectAround` and `CollectLine`;
  - mana and the ultimate: `IManaGainModifier` (gains from actions and damage, the fraction carried; `Increase` stays exact) and `IUltimateGate` (holds a full bar to the normal action; a silence is apart).
- `CombatValueScaling` on damage and heal nodes (the caster's attribute, or a share of each target's max health) and exact damage (no crit, no defense).

**Editor** (`CupkekGames.Combat.Editor.asmdef`)

- Custom inspectors / drawers for combat assets.

## Dependencies

Asmdef references resolve via the CupkekGames scoped registry: `units`, `character`, `data`, `services`, `rpgstats`, `behaviourtrees`, `pool`, `addressableassets`, `audio`, `textpopup`, `vfx`, `shapedrawing`, `timesystem`, `transforms`, `keyvaluedatabases`, `fadeables`. Bring your own copy via the registry.
