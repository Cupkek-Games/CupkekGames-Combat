using System;
using System.Text.RegularExpressions;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using CupkekGames.RPGStats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CupkekGames.Combat.Tests
{
    public class BuffAndPayloadTests
    {
        private CombatTestWorld _world;

        [SetUp]
        public void SetUp() => _world = new CombatTestWorld();

        [TearDown]
        public void TearDown() => _world.Dispose();

        [Test]
        public void ABuffOnSeveralTargets_IsEachTargetsOwn()
        {
            CombatUnit saeko = _world.Unit("Saeko");
            CombatUnit carmen = _world.Unit("Carmen");
            Guid id = Guid.NewGuid();
            AttributeEffect effect = _world.Multiply(_world.ATK, 1.2f);

            CombatActionNodeBuff.GiveEach(new[] { saeko, carmen },
                () => CombatActionNodeBuff.GetCombatAttributeDataEffectRuntime(null, "PowerUp", "", id, effect, -1f, true));

            CombatAttributeDataEffectRuntime saekos = saeko.Buffs.TryGet(id);
            CombatAttributeDataEffectRuntime carmens = carmen.Buffs.TryGet(id);
            Assert.IsNotNull(saekos);
            Assert.IsNotNull(carmens);
            Assert.AreNotSame(saekos, carmens, "one instance each: a shared one would end for both with one countdown");

            saeko.Buffs.Remove(id);
            Assert.IsNull(saeko.Buffs.TryGet(id));
            Assert.AreSame(carmens, carmen.Buffs.TryGet(id), "hers outlives Saeko's");
        }

        [Test]
        public void AUnitsStatuses_EndQuietly_WhenItsAIIsKilled()
        {
            // A fight's end kills every unit's AI. A stun still on a hero ended on the cancelled
            // token and restarted the AI of a unit whose fight was over (the fight simulator).
            CombatUnit saeko = _world.Unit("Saeko");
            CombatUnit carmen = _world.Unit("Carmen");
            saeko.SetupAI();
            carmen.SetupAI();
            StatusEffectSO stun = _world.Own(ScriptableObject.CreateInstance<StatusEffectSO>());
            stun.name = "Stun";

            var hers = new StatusEffect(stun, 5f, 1, saeko.DeathToken.Token);
            var control = new StatusEffect(stun, 0.2f, 1, carmen.DeathToken.Token);
            int ended = 0, controlEnded = 0;
            hers.OnEnd += _ => ended++;
            control.OnEnd += _ => controlEnded++;
            saeko.StatusEffects.Add(null, hers);
            carmen.StatusEffects.Add(null, control);

            saeko.KillAI();
            saeko.TimeBundle.TimeContext.Update(0.1f);
            carmen.TimeBundle.TimeContext.Update(0.5f);

            Assert.AreEqual(0, ended, "killed with its unit: its end never runs");
            CollectionAssert.IsEmpty(saeko.StatusEffects.All);
            Assert.AreEqual(1, controlEnded, "a status that runs out still ends");
        }

        [Test]
        public void APayloadThatFinishesAtOnce_RunsQuietly()
        {
            var payload = ScriptableObject.CreateInstance<FinishingNode>();
            try
            {
                Projectile.RunPayload(payload, new GraphFrame(new GraphBlackboard()));
                Assert.AreEqual(BTNodeRuntimeState.Success, payload.State);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(payload);
            }
        }

        [Test]
        public void APayloadStillRunningAfterTheLanding_SaysSo()
        {
            var payload = ScriptableObject.CreateInstance<StillRunningNode>();
            try
            {
                LogAssert.Expect(LogType.Error, new Regex(@"\[Projectile\] the payload .* is still running after the shot landed"));
                Projectile.RunPayload(payload, new GraphFrame(new GraphBlackboard()));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(payload);
            }
        }

        private sealed class FinishingNode : BTNode
        {
            protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime) => BTNodeRuntimeState.Success;
            public override BTNode Clone() => this;
            protected override void OnReset() { }
        }

        private sealed class StillRunningNode : BTNode
        {
            protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime) => BTNodeRuntimeState.Running;
            public override BTNode Clone() => this;
            protected override void OnReset() { }
        }
    }
}
