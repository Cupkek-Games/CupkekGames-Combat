using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using CupkekGames.Units;

namespace CupkekGames.Combat.Tests
{
    /// <summary>
    /// The control seams, scene-free: additive status stacks with a cap, a level-up that
    /// refreshes the time, immunities (to a status, to the controls it carries, to a push),
    /// counted stuns, silences and roots, pushes, shields that know why they ended (and a
    /// broken shield no longer dropping the next), the mana gain multiplier and the ultimate gate.
    /// </summary>
    public class ControlSeamTests
    {
        private CombatTestWorld _world;

        [SetUp]
        public void SetUp() => _world = new CombatTestWorld();

        [TearDown]
        public void TearDown() => _world.Dispose();

        private StatusEffectSO Status(string name, StatusStacking stacking = StatusStacking.Level, int maxStacks = 1,
            bool stun = false, bool silence = false, bool root = false)
        {
            StatusEffectSO status = _world.Own(ScriptableObject.CreateInstance<StatusEffectSO>());
            status.name = name;
            SerializedObject so = new SerializedObject(status);
            so.FindProperty("_stacking").enumValueIndex = (int)stacking;
            so.FindProperty("_maxStacks").intValue = maxStacks;
            if (stun || silence || root)
            {
                SerializedProperty behaviors = so.FindProperty("_behaviors");
                behaviors.arraySize = 1;
                behaviors.GetArrayElementAtIndex(0).managedReferenceValue = new DisableBehavior();
                so.ApplyModifiedPropertiesWithoutUndo();
                so.Update();
                SerializedProperty disable = so.FindProperty("_behaviors").GetArrayElementAtIndex(0);
                disable.FindPropertyRelative("_stun").boolValue = stun;
                disable.FindPropertyRelative("_silence").boolValue = silence;
                disable.FindPropertyRelative("_root").boolValue = root;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return status;
        }

        private static StatusEffect On(StatusEffectSO status, float duration, int level = 1)
            => new StatusEffect(status, duration, level, CancellationToken.None);

        private sealed class Feature : IUnitFeature, IStatusImmunity, IControlImmunity, IManaGainModifier, IUltimateGate
        {
            public StatusEffectSO Refused;
            public CombatControl Immune;
            public float ManaGain = 1f;
            public bool Gate = true;

            public bool IsImmune(CombatUnit unit, StatusEffectSO status) => status == Refused;
            public CombatControl GetImmuneControls(CombatUnit unit) => Immune;
            public float GetManaGainMultiplier(CombatUnit unit) => ManaGain;
            public bool CanCastUltimate(CombatUnit unit) => Gate;
            public void OnInitialize(Unit unit) { }
            public void OnDispose(Unit unit) { }
        }

        // ── Statuses ─────────────────────────────────────────────────────

        [Test]
        public void AnAdditiveStatus_StacksUpToItsCap_AndKeepsTheLongerTime()
        {
            CombatUnit wolf = _world.Unit("Wolf", team: 1);
            StatusEffectSO burn = Status("Burn", StatusStacking.Additive, maxStacks: 3);

            Assert.IsTrue(wolf.StatusEffects.Add(null, On(burn, 2f, level: 2)));
            Assert.IsTrue(wolf.StatusEffects.Add(null, On(burn, 4f)));
            StatusEffect on = wolf.StatusEffects.All[burn];
            Assert.AreEqual(3, on.Level, "2 + 1 stacks");
            Assert.AreEqual(4f, on.Duration, 0.01f, "the longer time");

            wolf.StatusEffects.Add(null, On(burn, 1f, level: 2));
            Assert.AreEqual(3, on.Level, "capped");
            Assert.AreEqual(4f, on.Duration, 0.01f);

            CombatUnit slime = _world.Unit("Slime", team: 1);
            slime.StatusEffects.Add(null, On(burn, 2f, level: 5));
            Assert.AreEqual(3, slime.StatusEffects.All[burn].Level, "a first coat is capped too");
        }

        [Test]
        public void AHigherLevel_RefreshesTheTime_AndALowerOneIsRefused()
        {
            CombatUnit wolf = _world.Unit("Wolf", team: 1);
            StatusEffectSO slow = Status("Slow");

            wolf.StatusEffects.Add(null, On(slow, 1f));
            wolf.TimeBundle.TimeContext.Update(0.8f);
            StatusEffect on = wolf.StatusEffects.All[slow];
            Assert.Less(on.Duration, 0.5f, "most of its time is gone");

            Assert.IsTrue(wolf.StatusEffects.Add(null, On(slow, 2f, level: 2)));
            Assert.AreEqual(2, on.Level);
            Assert.AreEqual(2f, on.Duration, 0.01f, "a level-up refreshes the time (it used to keep the old one)");

            Assert.IsFalse(wolf.StatusEffects.Add(null, On(slow, 9f, level: 1)), "a lower level is refused");
            Assert.AreEqual(2f, on.Duration, 0.01f);
        }

        [Test]
        public void AnImmuneUnit_RefusesTheStatus_OrAnyStatusCarryingAControlItShrugsOff()
        {
            StatusEffectSO burn = Status("Burn");
            StatusEffectSO stun = Status("Stun", stun: true);
            StatusEffectSO slow = Status("Slow");
            Assert.AreEqual(CombatControl.Stun, stun.Controls);
            Assert.AreEqual(CombatControl.None, slow.Controls);

            CombatUnit saeko = _world.Unit("Saeko", features: new Feature { Refused = burn, Immune = CombatControl.Stun | CombatControl.Root });

            Assert.IsFalse(saeko.StatusEffects.Add(null, On(burn, 2f)), "refused by name");
            Assert.IsFalse(saeko.StatusEffects.Add(null, On(stun, 2f)), "its stun is shrugged off");
            Assert.IsFalse(saeko.IsStunned);
            Assert.IsTrue(saeko.StatusEffects.Add(null, On(slow, 2f)), "anything else lands");
        }

        // ── Counted controls ─────────────────────────────────────────────

        [Test]
        public void OverlappingStuns_HoldUntilTheLastEnds()
        {
            CombatUnit wolf = _world.Unit("Wolf", team: 1);
            StatusEffectSO shortStun = Status("Bash", stun: true);
            StatusEffectSO longStun = Status("Slam", stun: true, root: true);

            wolf.StatusEffects.Add(null, On(shortStun, 1f));
            wolf.StatusEffects.Add(null, On(longStun, 2f));
            Assert.IsTrue(wolf.IsStunned);
            Assert.IsTrue(wolf.IsRooted);

            wolf.TimeBundle.TimeContext.Update(1.2f);
            Assert.IsFalse(wolf.StatusEffects.All.ContainsKey(shortStun), "the short one ended");
            Assert.IsTrue(wolf.IsStunned, "the long one still holds (the first to end used to free it)");

            wolf.TimeBundle.TimeContext.Update(1f);
            Assert.IsFalse(wolf.IsStunned);
            Assert.IsFalse(wolf.IsRooted);
        }

        [Test]
        public void Controls_AreCounted_AndClearWithTheFightLife()
        {
            CombatUnit wolf = _world.Unit("Wolf", team: 1);
            wolf.Silence();
            wolf.Silence();
            wolf.Unsilence();
            Assert.IsTrue(wolf.IsSilenced, "one silence still on");
            wolf.Unsilence();
            Assert.IsFalse(wolf.IsSilenced);
            Assert.Throws<InvalidOperationException>(() => wolf.Unsilence(), "ending one it does not have is a bug");
            Assert.Throws<InvalidOperationException>(() => wolf.Unstun());
            Assert.Throws<InvalidOperationException>(() => wolf.Unroot());

            wolf.Stun();
            wolf.Root();
            wolf.KillAI();
            Assert.IsFalse(wolf.IsStunned, "a fight life's end clears its controls");
            Assert.IsFalse(wolf.IsRooted);
        }

        // ── Push ─────────────────────────────────────────────────────────

        [Test]
        public void APush_MovesALivingUnit_UnlessItShrugsPushesOff_AndIsReported()
        {
            FakeUnitManager field = new FakeUnitManager();
            FakeCombatManager manager = _world.Manager(field);
            CombatUnit carmen = _world.Unit("Carmen");
            CombatUnit wolf = _world.Unit("Wolf", team: 1);
            CombatUnit golem = _world.Unit("Golem", team: 1, features: new Feature { Immune = CombatControl.Push });
            wolf.SetupAI();
            golem.SetupAI();
            List<(CombatUnit unit, CombatUnit by, int tiles)> pushed = new List<(CombatUnit, CombatUnit, int)>();
            manager.EventDatabase.OnPushed += (unit, by, tiles) => pushed.Add((unit, by, tiles));

            Assert.AreEqual(2, CombatPush.Push(manager, wolf, carmen, 2));
            Assert.AreEqual(0, CombatPush.Push(manager, golem, carmen, 2), "it shrugs pushes off");
            Assert.AreEqual(0, CombatPush.Push(manager, _world.Unit("Fallen", team: 1), carmen, 2), "a unit out of the fight does not move");
            Assert.AreEqual(0, CombatPush.Push(manager, wolf, carmen, 0));

            Assert.AreEqual(1, field.FakeSpace.Pushes, "only the first reached the space");
            CollectionAssert.AreEqual(new[] { (wolf, carmen, 2) }, pushed);
        }

        // ── Shields ──────────────────────────────────────────────────────

        private static CombatUnitShieldNode Shield(int amount, CombatUnit caster = null)
            => new CombatUnitShieldNode(amount, Guid.NewGuid(), null, caster);

        [Test]
        public void ABrokenShield_EndsAlone_AndTheNextTakesTheRest()
        {
            CombatUnit elaine = _world.Unit("Elaine");
            CombatUnit saeko = _world.Unit("Saeko", hp: 100f);
            CombatUnitShieldNode first = Shield(10, elaine);
            CombatUnitShieldNode second = Shield(10, elaine);
            CombatUnitShieldNode third = Shield(10);
            saeko.Shield.AddShield(first, 1f, CancellationToken.None);
            saeko.Shield.AddShield(second, 2f, CancellationToken.None);
            saeko.Shield.AddShield(third, 3f, CancellationToken.None);
            List<CombatUnitShieldNode> ended = new List<CombatUnitShieldNode>();
            saeko.Shield.OnShieldNodeRemove += ended.Add;

            saeko.Health.TakeDamage(new CombatHit(null, new CombatSource(CombatSourceKind.Environment, "Rock"), 15));

            CollectionAssert.AreEqual(new[] { second, third }, saeko.Shield.Shields, "the next shield stays (a break used to drop it)");
            Assert.AreEqual(5, second.Shield, "it took the rest");
            Assert.AreEqual(100, saeko.Health.Current);
            CollectionAssert.AreEqual(new[] { first }, ended);
            Assert.AreEqual(CombatShieldEndReason.Broken, first.EndReason);
            Assert.AreEqual(10, first.Absorbed);
            Assert.AreSame(elaine, first.Caster);
            Assert.AreSame(saeko, first.Owner);
            Assert.AreEqual(CombatShieldEndReason.None, second.EndReason);

            saeko.Health.TakeDamage(new CombatHit(null, new CombatSource(CombatSourceKind.Environment, "Rock"), 40));
            Assert.AreEqual(75, saeko.Health.Current, "both left broke (15): 25 of 40 got through");
            Assert.AreEqual(3, ended.Count);
        }

        [Test]
        public void AShield_EndsExpired_OrWithItsOwnerGone_AndTheFightHearsIt()
        {
            FakeCombatManager manager = _world.Manager();
            CombatUnit saeko = _world.Unit("Saeko");
            manager.EventDatabase.InvokeOnUnitSpawned(saeko);
            List<CombatUnitShieldNode> ended = new List<CombatUnitShieldNode>();
            manager.EventDatabase.OnShieldEnded += (owner, shield) => ended.Add(shield);

            CombatUnitShieldNode brief = Shield(30);
            CombatUnitShieldNode lasting = Shield(20);
            saeko.Shield.AddShield(brief, 1f, CancellationToken.None);
            saeko.Shield.AddShield(lasting, 9f, CancellationToken.None);
            lasting.AddShield(5, false);
            Assert.AreEqual(25, lasting.Granted, "what it granted in all");

            saeko.TimeBundle.TimeContext.Update(1.5f);
            Assert.AreEqual(CombatShieldEndReason.Expired, brief.EndReason);
            Assert.AreEqual(30, brief.Shield, "what it had left");
            Assert.AreEqual(0, brief.Absorbed);

            saeko.KillAI();
            Assert.AreEqual(CombatShieldEndReason.OwnerGone, lasting.EndReason);
            CollectionAssert.IsEmpty(saeko.Shield.Shields);
            CollectionAssert.AreEqual(new[] { brief, lasting }, ended);
        }

        [Test]
        public void AShieldEndsHandler_MayShieldTheSameUnitAgain()
        {
            CombatUnit saeko = _world.Unit("Saeko");
            CombatUnitShieldNode first = Shield(10);
            CombatUnitShieldNode reward = Shield(7);
            saeko.Shield.AddShield(first, 5f, CancellationToken.None);
            saeko.Shield.OnShieldNodeRemove += node =>
            {
                if (node == first) saeko.Shield.AddShield(reward, 5f, CancellationToken.None);
            };

            saeko.Health.TakeDamage(new CombatHit(null, new CombatSource(CombatSourceKind.Environment, "Rock"), 12));

            CollectionAssert.AreEqual(new[] { reward }, saeko.Shield.Shields, "reported after the shields settled: the new one is untouched");
            Assert.AreEqual(7, reward.Shield);
        }

        // ── Mana and the ultimate ────────────────────────────────────────

        [Test]
        public void ManaGains_AreMultiplied_WithTheFractionCarried_AndASetAmountStaysExact()
        {
            ((List<ActionManaEffect>)_world.Settings.ActionManaEffects).Add(
                new ActionManaEffect { ActionTypeId = 0, Effect = ManaEffectType.GainAmount, Value = 15 });
            Feature tempo = new Feature { ManaGain = 0.5f };
            CombatUnit carmen = _world.Unit("Carmen", features: tempo);

            carmen.Mana.OnTakeAction(0);
            Assert.AreEqual(7, carmen.Mana.Current, "7.5: the half is carried");
            carmen.Mana.OnTakeAction(0);
            Assert.AreEqual(15, carmen.Mana.Current, "the carried half completes the next");

            carmen.Mana.Increase(10);
            Assert.AreEqual(25, carmen.Mana.Current, "a set amount is exact");

            tempo.ManaGain = 0f;
            carmen.Mana.OnTakeAction(0);
            Assert.AreEqual(25, carmen.Mana.Current, "no gain at 0");

            CombatUnit plain = _world.Unit("Plain");
            plain.Mana.OnTakeAction(0);
            Assert.AreEqual(15, plain.Mana.Current, "no modifier: unchanged");
        }

        [Test]
        public void TheUltimateGate_HoldsWhileAnyFeatureSaysNo()
        {
            Feature gate = new Feature();
            CombatUnit saeko = _world.Unit("Saeko", features: new IUnitFeature[] { new Feature(), gate });
            Assert.IsTrue(saeko.CanCastUltimate);

            gate.Gate = false;
            Assert.IsFalse(saeko.CanCastUltimate);
            Assert.IsTrue(_world.Unit("Plain").CanCastUltimate);
        }
    }
}
