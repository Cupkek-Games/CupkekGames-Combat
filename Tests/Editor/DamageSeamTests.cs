using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using CupkekGames.RPGStats;
using CupkekGames.Units;

namespace CupkekGames.Combat.Tests
{
    /// <summary>
    /// The damage seams, scene-free: each unit's share of damage dealt and taken (roles and
    /// features), the crit policy (a call's, then the features': Never beats Always, then the
    /// roll), a dodged action hit, and a lucky unit's chances rolled twice.
    /// </summary>
    public class DamageSeamTests
    {
        private CombatTestWorld _world;
        private DamageTypeDefinitionSO _physical;

        [SetUp]
        public void SetUp()
        {
            _world = new CombatTestWorld();
            _physical = _world.Own(ScriptableObject.CreateInstance<DamageTypeDefinitionSO>());
        }

        [TearDown]
        public void TearDown() => _world.Dispose();

        private static CombatSource Action(CombatUnit owner) => new CombatSource(CombatSourceKind.Action, "Strike", owner);
        private static CombatSource Proc(CombatUnit owner) => new CombatSource(CombatSourceKind.Proc, "Echo", owner);

        private DamageResult Hit(CombatUnit attacker, CombatUnit target, float attack, CombatSource source,
            CombatCritPolicy policy = CombatCritPolicy.Roll, int seed = 1)
            => CombatDamageCalculator.CalculateAttackDamage(_world.Settings, attacker, target, attack, _physical, source, new CombatRandom(seed), policy);

        private sealed class Feature : IUnitFeature, IDamageDealtModifier, IDamageTakenModifier, ICritModifier, IChanceRollModifier
        {
            public float Dealt = 1f;
            public float TakenMinus;
            public CombatCritPolicy Crit = CombatCritPolicy.Roll;
            public bool Lucky;

            public float ModifyDamageDealt(CombatUnit attacker, CombatUnit target, float damage, DamageContext ctx) => damage * Dealt;
            public float ModifyDamageTaken(CombatUnit target, CombatUnit attacker, float damage, DamageContext ctx) => damage - TakenMinus;
            public CombatCritPolicy GetCritPolicy(CombatUnit attacker, CombatUnit target, CombatSource source) => Crit;
            public bool RollsTwice(CombatUnit unit) => Lucky;
            public void OnInitialize(Unit unit) { }
            public void OnDispose(Unit unit) { }
        }

        [Test]
        public void DamageDealtAndTaken_FoldEachUnitsShareAndFeatures_ButNotExactDamage()
        {
            CombatUnit saeko = _world.Unit("Saeko", features: new Feature { Dealt = 2f });
            CombatUnit wolf = _world.Unit("Wolf", team: 1, hp: 1000f, features: new Feature { TakenMinus = 50f });
            _world.Set(saeko, _world.DamageDealt, 0.25f);
            _world.Set(wolf, _world.DamageTaken, -0.2f);

            // 100 × 1.25 (her share) × 2 (her feature) × 0.8 (its share) − 50 (its feature)
            Assert.AreEqual(150, Hit(saeko, wolf, 100f, Action(saeko)).Damage);

            DamageResult exact = CombatDamageCalculator.CalculateExactDamage(_world.Settings, saeko, wolf, 100f, Action(saeko));
            Assert.AreEqual(100, exact.Damage, "an exact amount is read off: no shares, no features");

            CombatUnit plain = _world.Unit("Plain");
            CombatUnit dummy = _world.Unit("Dummy", team: 1);
            Assert.AreEqual(100, Hit(plain, dummy, 100f, Action(plain)).Damage, "no share and no feature: unchanged");
        }

        [Test]
        public void TheCrit_IsTheCallsPolicy_ThenTheFeatures_NeverOverAlways_ThenTheRoll()
        {
            Feature always = new Feature { Crit = CombatCritPolicy.Always };
            CombatUnit carmen = _world.Unit("Carmen", features: always);
            CombatUnit wolf = _world.Unit("Wolf", team: 1, hp: 1000f);
            _world.Set(carmen, _world.CritDmg, 2f);

            DamageResult forced = Hit(carmen, wolf, 100f, Action(carmen));
            Assert.IsTrue(forced.IsCrit, "a feature's sure crit, at 0 chance");
            Assert.AreEqual(200, forced.Damage, "times her critical damage");

            DamageResult called = Hit(carmen, wolf, 100f, Proc(carmen), CombatCritPolicy.Never);
            Assert.IsFalse(called.IsCrit, "the call's policy wins (a second strike that never crits)");

            CombatUnit cursed = _world.Unit("Cursed", features: new IUnitFeature[] { new Feature { Crit = CombatCritPolicy.Always }, new Feature { Crit = CombatCritPolicy.Never } });
            Assert.IsFalse(Hit(cursed, wolf, 100f, Action(cursed)).IsCrit, "Never beats Always");

            CombatUnit sure = _world.Unit("Sure");
            _world.Set(sure, _world.CritChance, 1f);
            Assert.IsTrue(Hit(sure, wolf, 100f, Action(sure)).IsCrit, "no policy: the roll, on her chance");
            Assert.IsFalse(Hit(_world.Unit("Plain"), wolf, 100f, Action(sure)).IsCrit, "0 chance never crits");
        }

        [Test]
        public void AnActionsHit_CanMiss_AndAMissLandsNothing_AndIsReported()
        {
            CombatUnit wolf = _world.Unit("Wolf", team: 1);
            CombatUnit saeko = _world.Unit("Saeko", hp: 100f);
            _world.Set(saeko, _world.Evasion, 1f);

            DamageResult miss = Hit(wolf, saeko, 30f, Action(wolf));
            Assert.IsTrue(miss.IsMiss);
            Assert.AreEqual(0, miss.Damage);
            Assert.IsFalse(Hit(wolf, saeko, 30f, Proc(wolf)).IsMiss, "only an action's hit can be dodged");

            FakeCombatManager manager = _world.Manager();
            List<CombatUnit> dodgers = new List<CombatUnit>();
            manager.EventDatabase.OnMiss += (attacker, target, source) => dodgers.Add(target);

            CombatDamageCalculator.ApplyDamageAndVisuals(_world.Settings, manager, wolf, saeko, miss);

            Assert.AreEqual(100, saeko.Health.Current, "nothing lands");
            Assert.IsNull(saeko.LastHit, "a miss is no hit");
            CollectionAssert.AreEqual(new[] { saeko }, dodgers);
        }

        [Test]
        public void ALuckyUnit_RollsEveryChanceTwice_AndKeepsTheBetter()
        {
            CombatUnit plain = _world.Unit("Plain");
            CombatUnit lucky = _world.Unit("Lucky", features: new Feature { Lucky = true });
            CombatRandom forPlain = new CombatRandom(7);
            CombatRandom forLucky = new CombatRandom(7);

            int plainHits = 0, luckyHits = 0;
            for (int i = 0; i < 2000; i++)
            {
                if (plain.RollChance(0.5f, forPlain)) plainHits++;
                if (lucky.RollChance(0.5f, forLucky)) luckyHits++;
            }

            Assert.AreEqual(1000, plainHits, 80, "about half");
            Assert.AreEqual(1500, luckyHits, 80, "about three in four: 1 − 0.5²");
            Assert.IsFalse(lucky.RollChance(0f, forLucky), "no chance stays no chance");
            Assert.IsTrue(lucky.RollChance(1f, forLucky));
        }
    }
}
