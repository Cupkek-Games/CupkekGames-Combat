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

        /// <summary>
        /// Puts <paramref name="statusEffect"/> on the unit; false when it was refused (the unit
        /// is immune, <see cref="CombatUnit.IsImmuneTo"/>, or a higher level is on). One it has
        /// already stacks as its <see cref="StatusEffectSO.Stacking"/> says, and keeps the longer
        /// time.
        /// </summary>
        public bool Add(ICombatManager manager, StatusEffect statusEffect)
        {
            if (statusEffect.Duration <= 0)
            {
                throw new Exception("Status effect duration must be greater than 0");
            }

            StatusEffectSO definition = statusEffect.Definition;
            if (_owner.IsImmuneTo(definition)) return false;

            bool additive = definition.Stacking == StatusStacking.Additive;

            if (_effects.TryGetValue(definition, out StatusEffect current))
            {
                if (additive)
                {
                    current.Level = Math.Min(definition.MaxStacks, current.Level + statusEffect.Level);
                }
                else if (current.Level > statusEffect.Level)
                {
                    return false;
                }
                else
                {
                    current.Level = statusEffect.Level;
                }

                current.Duration = Math.Max(current.Duration, statusEffect.Duration);
                OnUpdate?.Invoke(current);
                return true;
            }

            if (additive) statusEffect.Level = Math.Min(definition.MaxStacks, statusEffect.Level);
            _effects[definition] = statusEffect;

            statusEffect.OnEnd += OnStatusEffectEnd;
            statusEffect.StartExecuteLoop(_combatSettings, manager, _owner);

            OnAdd?.Invoke(statusEffect);
            return true;
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
