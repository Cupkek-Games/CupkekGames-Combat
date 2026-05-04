using System;
using System.Collections.Generic;
using CupkekGames.RPGStats;
using CupkekGames.Data;
using CupkekGames.Units;
using UnityEngine;

namespace CupkekGames.Combat
{
    [Serializable]
    public class ActionSlot
    {
        public int ActionTypeId;
        public List<CombatActionSO> Actions;
    }

    /// <summary>
    /// Combat-specific feature definition for <see cref="UnitDefinitionSO"/>.
    /// Holds base attributes, scaling, tier, element, damage type, and action slots.
    /// </summary>
    [Serializable]
    public class CombatAttributesDefinition : IUnitFeatureDefinition
    {
        [SerializeField] private ElementTypeDefinitionSO _element;
        [SerializeField] private DamageTypeDefinitionSO _damageType;
        [SerializeField] private AttributeSet _baseAttributes = new();
        [SerializeField] private AttributeScaling _levelScaling = new();
        [SerializeField] private CombatUnitTierSO _tier;
        [SerializeField] private List<ActionSlot> _actionSlots = new();

        public ElementTypeDefinitionSO Element { get => _element; set => _element = value; }
        public DamageTypeDefinitionSO DamageType { get => _damageType; set => _damageType = value; }
        public AttributeSet BaseAttributes { get => _baseAttributes; set => _baseAttributes = value; }
        public AttributeScaling LevelScaling { get => _levelScaling; set => _levelScaling = value; }
        public CombatUnitTierSO Tier { get => _tier; set => _tier = value; }
        public List<ActionSlot> ActionSlots { get => _actionSlots; set => _actionSlots = value; }

        public List<CombatActionSO> GetActions(int actionTypeId)
        {
            foreach (ActionSlot slot in _actionSlots)
            {
                if (slot.ActionTypeId == actionTypeId)
                    return slot.Actions;
            }
            return null;
        }

        public IEnumerable<CombatActionSO> GetAllActions()
        {
            foreach (ActionSlot slot in _actionSlots)
            {
                if (slot.Actions == null) continue;
                foreach (CombatActionSO action in slot.Actions)
                    yield return action;
            }
        }

        public CombatActionSO GetCombatAction(int actionType, ICombatManager manager, CombatUnit caster, CombatUnit enemy)
        {
            List<CombatActionSO> actions = GetActions(actionType);
            if (actions == null) return null;
            return SelectAction(actions, manager, caster, enemy);
        }

        private CombatActionSO SelectAction(List<CombatActionSO> list, ICombatManager manager, CombatUnit caster, CombatUnit enemy)
        {
            if (list.Count == 0) return null;
            if (list.Count == 1) return list[0];

            float highestPercentage = 0.0f;
            List<CombatActionSO> best = new();

            foreach (CombatActionSO action in list)
            {
                float percentage = action.GetUtilityAIPercentage(manager.UnitManager, caster, enemy, false);
                if (percentage > highestPercentage)
                {
                    highestPercentage = percentage;
                    best.Clear();
                    best.Add(action);
                }
                else if (percentage == highestPercentage)
                {
                    best.Add(action);
                }
            }

            if (best.Count == 0) return null;
            return best[UnityEngine.Random.Range(0, best.Count)];
        }

        public IFeature CloneFeature() => this; // SO references, no mutable state to clone
    }
}
