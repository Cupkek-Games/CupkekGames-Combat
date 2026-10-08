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
