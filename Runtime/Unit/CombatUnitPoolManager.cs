using System;
using System.Collections.Generic;
using CupkekGames.Character;
using CupkekGames.Units;
using UnityEngine;

namespace CupkekGames.Combat
{
    public class CombatUnitPoolManager
    {
        private IUnitSOProvider _unitSOProvider;
        private Func<UnitDefinitionSO, int, int, bool, bool, CombatUnitPool> _poolFactory;
        private Dictionary<string, CombatUnitPool> _pools = new();

        public CombatUnitPoolManager(IUnitSOProvider unitSOProvider,
            Func<UnitDefinitionSO, int, int, bool, bool, CombatUnitPool> poolFactory)
        {
            _unitSOProvider = unitSOProvider;
            _poolFactory = poolFactory;
        }

        public void CreatePool(CombatUnitReference combatUnitRef, int defaultPoolCapacity, int maxPoolSize)
        {
            if (_pools.ContainsKey(combatUnitRef.Key))
                return;

            UnitDefinitionSO definition = combatUnitRef.GetUnitDefinition(_unitSOProvider);
            CharacterDefinition charDef = definition?.GetDefinition<CharacterDefinition>();

            if (charDef?.Model != null && charDef.Model.HasModel)
            {
                CombatUnitPool pool = _poolFactory(definition, defaultPoolCapacity, maxPoolSize, true, true);
                _pools.Add(combatUnitRef.Key, pool);
            }
            else
            {
                Debug.LogWarning($"CombatUnit model prefab not valid for key: {combatUnitRef.Key}");
            }
        }

        public void SpawnCombatUnitGameObject(CombatUnit combatUnit, Vector3 position, Quaternion rotation)
        {
            string key = combatUnit.DataReference.Key;
            if (!_pools.TryGetValue(key, out CombatUnitPool pool))
            {
                Debug.LogError($"CombatUnitPoolManager: No pool for key '{key}'. Call CreatePool first.");
                return;
            }
            GameObject instance = pool.Pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);

            CombatUnitView combatUnitGameObject = instance.GetComponent<CombatUnitView>();
            combatUnit.RegisterCombatUnitGameObject(combatUnitGameObject);
        }

        public void Dispose()
        {
            foreach (var pool in _pools)
                pool.Value.Dispose();
        }
    }
}
