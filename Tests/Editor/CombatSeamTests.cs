using System;
using System.Collections.Generic;
using NUnit.Framework;
using CupkekGames.RPGStats;
using CupkekGames.Units;

namespace CupkekGames.Combat.Tests
{
    /// <summary>
    /// The seams a game builds on, scene-free: the skill rank's fallback to the
    /// level, value scaling off the caster or the target's max health, the
    /// attack-element fold, mana drain, the hit choke point (the last hit, the
    /// killer, the hit raised before the death), cancelling a queued ultimate,
    /// threat only from actions, and the shield-cast fold.
    /// </summary>
    public class CombatSeamTests
    {
        private CombatTestWorld _world;

        [SetUp]
        public void SetUp() => _world = new CombatTestWorld();

        [TearDown]
        public void TearDown() => _world.Dispose();

        private static CombatSource Action(CombatUnit owner) => new CombatSource(CombatSourceKind.Action, "Strike", owner);

        // ── Skill rank ───────────────────────────────────────────────────

        [Test]
        public void TheSkillRank_FollowsTheLevel_UntilOneIsSet()
        {
            CombatUnit unit = _world.Unit("Wolf", level: 4);
            Assert.AreEqual(4, unit.EffectiveSkillRank, "0 follows the level (an enemy)");

            unit.SkillRank = 1;
            Assert.AreEqual(1, unit.EffectiveSkillRank, "a set rank wins (a hero's path)");
            Assert.AreEqual(4, unit.Level, "the level is untouched");

            Assert.Throws<ArgumentOutOfRangeException>(() => unit.SkillRank = -1);
        }

        // ── Value scaling ────────────────────────────────────────────────

        [Test]
        public void AnAmount_ScalesOffTheCaster_OrAShareOfTheTargetsMaxHealth()
        {
            CombatUnit caster = _world.Unit("Healer", atk: 20f);
            CombatUnit target = _world.Unit("Knight", hp: 300f);
            AttributeModifier modifier = new AttributeModifier { Flat = 5f, Multiplier = 0.2f };

            Assert.AreEqual(9f, CombatDamageCalculator.CalculateScaledValue(caster, target, modifier,
                CombatValueScaling.CasterAttribute, _world.ATK), 0.001f, "5 + 20 × 0.2");
            Assert.AreEqual(65f, CombatDamageCalculator.CalculateScaledValue(caster, target, modifier,
                CombatValueScaling.TargetMaxHealth, _world.ATK), 0.001f, "5 + 300 × 0.2: a fifth of her max health");
        }

        // ── The attack element ───────────────────────────────────────────

        private sealed class Infusion : IUnitFeature, IAttackElementModifier
        {
            private readonly ElementTypeDefinitionSO _element;
            public Infusion(ElementTypeDefinitionSO element) => _element = element;
            public ElementTypeDefinitionSO ModifyAttackElement(CombatUnit unit, ElementTypeDefinitionSO current) => _element;
            public void OnInitialize(Unit unit) { }
            public void OnDispose(Unit unit) { }
        }

        [Test]
        public void AnInfusion_ChangesTheElementAUnitAttacksIn_NotTheOneItIsHitIn()
        {
            ElementTypeDefinitionSO water = _world.Element("Water");
            ElementTypeDefinitionSO fire = _world.Element("Fire");
            ElementTypeDefinitionSO grass = _world.Element("Grass");
            _world.Relate(fire, grass, 2f);

            CombatUnit plain = _world.Unit("Plain", element: water);
            CombatUnit infused = _world.Unit("Infused", element: water, features: new Infusion(fire));
            CombatUnit sapling = _world.Unit("Sapling", team: 1, element: grass, features: new Infusion(water));

            Assert.AreSame(water, plain.GetAttackElement());
            Assert.AreSame(fire, infused.GetAttackElement());

            // An exact amount: no crit and no defense, the attacker's element only.
            DamageResult neutral = CombatDamageCalculator.CalculateExactDamage(_world.Settings, plain, sapling, 30f, Action(plain));
            DamageResult weakness = CombatDamageCalculator.CalculateExactDamage(_world.Settings, infused, sapling, 30f, Action(infused));
            Assert.AreEqual(30, neutral.Damage);
            Assert.AreEqual(60, weakness.Damage, "fire on grass, though the attacker is water");
            Assert.IsFalse(weakness.IsCrit);
            Assert.AreEqual(2f, weakness.ElementMultiplier, "the target's own infusion does not change what it is hit in");
        }

        // ── Mana ─────────────────────────────────────────────────────────

        [Test]
        public void ADrain_TakesUpToWhatIsThere_AndAFullBarStopsSelectingTheUltimate()
        {
            CombatUnit unit = _world.Unit("Caster");
            unit.Mana.Increase(CombatTestWorld.MaxMP);
            Assert.AreEqual(_world.Settings.FullManaActionTypeId, unit.Mana.GetNextActionType());

            Assert.AreEqual(30, unit.Mana.Drain(30));
            Assert.AreEqual(70, unit.Mana.Current);
            Assert.AreEqual(_world.Settings.DefaultActionTypeId, unit.Mana.GetNextActionType(), "drained below full");

            Assert.AreEqual(70, unit.Mana.Drain(500), "only what is there");
            Assert.AreEqual(0, unit.Mana.Current);
            Assert.AreEqual(0, unit.Mana.Drain(5));
        }

        // ── Hits ─────────────────────────────────────────────────────────

        [Test]
        public void EveryHit_IsTheLastHit_AndTheKillingOne_IsRaisedBeforeTheDeath()
        {
            CombatUnit attacker = _world.Unit("Saeko");
            CombatUnit target = _world.Unit("Slime", team: 1, hp: 100f);
            List<string> order = new List<string>();
            target.Health.OnHit += (unit, hit) => order.Add(hit.Killed ? "killing hit" : "hit");
            target.OnDeathEvent += unit => order.Add($"death by {unit.LastHit?.Attacker?.Key}");

            Assert.IsNull(target.LastHit, "no hit yet");

            CombatSource strike = Action(attacker);
            target.Health.TakeDamage(new CombatHit(attacker, strike, 40));
            Assert.AreEqual(40, target.LastHit.Value.Dealt);
            Assert.IsFalse(target.LastHit.Value.Killed);
            Assert.AreSame(strike, target.LastHit.Value.Source);

            target.Health.TakeDamage(new CombatHit(attacker, strike, 80, isCrit: true));
            Assert.AreEqual(60, target.LastHit.Value.Dealt, "only what it had left");
            Assert.IsTrue(target.LastHit.Value.Killed);
            Assert.IsTrue(target.LastHit.Value.IsCrit);
            CollectionAssert.AreEqual(new[] { "hit", "killing hit", "death by Saeko" }, order);

            target.Health.TakeDamage(new CombatHit(_world.Unit("Other"), strike, 10));
            Assert.AreSame(attacker, target.LastHit.Value.Attacker, "a fallen unit takes no more hits");
        }

        [Test]
        public void DamageOverTime_NamesItsApplier_AsTheKiller()
        {
            CombatUnit applier = _world.Unit("Carmen");
            CombatUnit wearer = _world.Unit("Bud", team: 1, hp: 10f);
            CombatSource burn = CombatSource.ForStatus(null, applier);

            wearer.Health.TakeDamage(new CombatHit(burn.Owner, burn, 10));

            Assert.IsTrue(wearer.LastHit.Value.Killed);
            Assert.AreSame(applier, wearer.LastHit.Value.Attacker);
            Assert.AreEqual(CombatSourceKind.Status, wearer.LastHit.Value.Source.Kind);
        }

        [Test]
        public void AHit_NeedsItsSource()
        {
            CombatUnit attacker = _world.Unit("Saeko");
            Assert.Throws<ArgumentNullException>(() => new CombatHit(attacker, null, 5));
        }

        [Test]
        public void AnActionSource_IsFreshForEachRun()
        {
            CombatUnit caster = _world.Unit("Elaine");
            CombatActionSO action = _world.Own(UnityEngine.ScriptableObject.CreateInstance<CombatActionSO>());
            action.name = "PowerShield";

            CombatSource first = CombatSource.ForAction(action, caster);
            CombatSource second = CombatSource.ForAction(action, caster);

            Assert.AreNotSame(first, second, "one instance per run: once per action is once per source");
            Assert.AreEqual("PowerShield", first.Tag);
            Assert.AreSame(action, first.Action);
            Assert.AreSame(caster, first.Owner);
        }

        // ── Ultimates ────────────────────────────────────────────────────

        [Test]
        public void CancellingAQueuedUltimate_TakesItOffTheQueue_AndTheNextOneIsUp()
        {
            FakeUnitManager field = new FakeUnitManager();
            CombatUltimateManager ultimates = new CombatUltimateManager(_world.Settings, null, field, null);
            CombatUnit first = _world.Unit("Wolf", team: 1);
            CombatUnit second = _world.Unit("Werewolf", team: 1);
            field.Enemies.Add(first);
            field.Enemies.Add(second);

            ultimates.Enqueue(first);
            ultimates.Enqueue(second);
            Assert.AreSame(first, ultimates.Peek());
            Assert.IsFalse(ultimates.EffectsActive, "an ordinary enemy's ultimate does not freeze time");

            ultimates.RemoveFromQueue(first);
            Assert.AreSame(second, ultimates.Peek(), "the next one is up");
            ultimates.RemoveFromQueue(first);
            Assert.AreSame(second, ultimates.Peek(), "a unit off the queue leaves it alone");

            second.Health.TakeDamage(new CombatHit(null, new CombatSource(CombatSourceKind.Environment, "Fall"), 1000));
            Assert.IsFalse(ultimates.HasNext, "a fallen caster leaves the queue");
        }

        // ── Threat ───────────────────────────────────────────────────────

        [Test]
        public void OnlyAnActionsHit_AddsThreat()
        {
            CombatUnit fighter = _world.Unit("Saeko");
            CombatUnit thrower = _world.Unit("PotionCaster");
            CombatUnit applier = _world.Unit("Carmen");
            CombatUnitThreatTable table = new CombatUnitThreatTable();

            table.AddThreat(new CombatHit(fighter, Action(fighter), 30).Landed(30, false));
            table.AddThreat(new CombatHit(thrower, new CombatSource(CombatSourceKind.Item, "Firebomb", thrower), 50).Landed(50, false));
            table.AddThreat(new CombatHit(applier, CombatSource.ForStatus(null, applier), 20).Landed(20, false));
            table.AddThreat(new CombatHit(applier, new CombatSource(CombatSourceKind.Proc, "Echo", applier), 20).Landed(20, false));

            Assert.AreEqual(30, table.GetThreat(fighter));
            Assert.AreEqual(0, table.GetThreat(thrower), "a potion's stand-in is never chased");
            Assert.AreEqual(0, table.GetThreat(applier), "damage over time and procs draw no attention");
        }

        // ── Shields ──────────────────────────────────────────────────────

        private sealed class Withdrawn : IUnitFeature, IShieldCastModifier
        {
            private readonly float _amount;
            private readonly AttributeEffect _carried;

            public Withdrawn(float amount, AttributeEffect carried)
            {
                _amount = amount;
                _carried = carried;
            }

            public int ModifyShieldAmount(CombatUnit caster, int amount) => (int)(amount * _amount + 0.5f);
            public AttributeEffect GetShieldEffect(CombatUnit caster) => _carried;
            public void OnInitialize(Unit unit) { }
            public void OnDispose(Unit unit) { }
        }

        [Test]
        public void TheShieldCast_FoldsTheCastersModifiers_InFeatureOrder()
        {
            AttributeEffect radiant = _world.Multiply(_world.ATK, 2f);
            AttributeEffect own = _world.Multiply(_world.HP, 1.5f);
            CombatUnit plain = _world.Unit("Plain");
            CombatUnit elaine = _world.Unit("Elaine", features: new IUnitFeature[] { new Withdrawn(0.5f, null), new Withdrawn(1f, radiant) });

            (int amount, AttributeEffect effect) = CombatActionNodeShield.ApplyCastModifiers(plain, 40, own);
            Assert.AreEqual(40, amount);
            Assert.AreSame(own, effect, "no modifiers: the action's own shield");

            (amount, effect) = CombatActionNodeShield.ApplyCastModifiers(elaine, 40, own);
            Assert.AreEqual(20, amount, "withdrawn: weaker shields");
            Assert.AreEqual(2f, effect.GetMultiplier(_world.ATK), 0.001f, "radiant rides the shield");
            Assert.AreEqual(1.5f, effect.GetMultiplier(_world.HP), 0.001f, "beside the action's own effect");

            (amount, effect) = CombatActionNodeShield.ApplyCastModifiers(elaine, 40, null);
            Assert.AreSame(radiant, effect, "an action with no effect of its own carries the caster's");
        }
    }
}
