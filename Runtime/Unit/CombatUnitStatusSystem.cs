using System;
using System.Collections.Generic;

namespace CupkekGames.Combat
{
    public class CombatUnitStatusSystem
    {
        private readonly CombatUnit _owner;
        private readonly ICombatSettings _combatSettings;
        private readonly Dictionary<StatusEffectSO, StatusEffect> _effects = new();

        public Dictionary<StatusEffectSO, StatusEffect> All => _effects;

        // Events
        public event Action<StatusEffect> OnAdd;
        public event Action<StatusEffect> OnUpdate;
        public event Action<StatusEffect> OnRemove;

        public CombatUnitStatusSystem(
            CombatUnit owner,
            ICombatSettings combatSettings)
        {
            _owner = owner;
            _combatSettings = combatSettings;
        }

        public void Add(ICombatManager manager, StatusEffect statusEffect)
        {
            if (statusEffect.Duration <= 0)
            {
                throw new Exception("Status effect duration must be greater than 0");
            }

            StatusEffectSO definition = statusEffect.Definition;

            if (_effects.ContainsKey(definition))
            {
                StatusEffect currentStatusEffect = _effects[definition];

                if (currentStatusEffect.Level > statusEffect.Level)
                {
                    return;
                }
                else if (currentStatusEffect.Level == statusEffect.Level)
                {
                    _effects[definition].Duration = statusEffect.Duration > currentStatusEffect.Duration
                        ? statusEffect.Duration
                        : currentStatusEffect.Duration;
                    OnUpdate?.Invoke(_effects[definition]);
                    return;
                }
                else if (currentStatusEffect.Level < statusEffect.Level)
                {
                    _effects[definition].Level = statusEffect.Level;
                    OnUpdate?.Invoke(_effects[definition]);
                    return;
                }
            }
            else
            {
                _effects[definition] = statusEffect;

                statusEffect.OnEnd += OnStatusEffectEnd;
                statusEffect.StartExecuteLoop(_combatSettings, manager, _owner);

                OnAdd?.Invoke(statusEffect);
            }
        }

        private void OnStatusEffectEnd(StatusEffect statusEffect)
        {
            statusEffect.OnEnd -= OnStatusEffectEnd;
            statusEffect.Dispose();

            _effects.Remove(statusEffect.Definition);
            OnRemove?.Invoke(statusEffect);
        }

        /// <summary>
        /// Disposes all status effects without firing individual remove events.
        /// Used during death cleanup.
        /// </summary>
        public void DisposeAll()
        {
            foreach (StatusEffect statusEffect in _effects.Values)
            {
                statusEffect.Dispose();
            }

            _effects.Clear();
        }
    }
}
