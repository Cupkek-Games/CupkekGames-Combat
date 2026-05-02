using System;
using UnityEngine;
using CupkekGames.Pool;

namespace CupkekGames.Combat
{
    public class CombatUnitPool : GameObjectPoolBase
    {
        private Func<GameObject> _createFunc;
        private Action _disposeFunc;

        public CombatUnitPool(
            Func<GameObject> createFunc,
            Action disposeFunc,
            int defaultPoolCapacity,
            int maxPoolSize,
            bool prewarm,
            bool collectionCheck) :
            base(defaultPoolCapacity, maxPoolSize, collectionCheck)
        {
            _createFunc = createFunc;
            _disposeFunc = disposeFunc;

            if (prewarm)
            {
                Prewarm();
            }
        }

        public override GameObject CreateObject()
        {
            return _createFunc();
        }

        public override void OnReturnToPool(GameObject instance)
        {
            base.OnReturnToPool(instance);

            CombatUnitView combatUnitGameObject = instance.GetComponent<CombatUnitView>();
            combatUnitGameObject.UnRegisterCombatUnit();
        }

        public void Dispose()
        {
            Pool.Dispose();
            _disposeFunc?.Invoke();
        }
    }
}