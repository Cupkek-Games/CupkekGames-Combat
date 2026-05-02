using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
    public class CombatUnitMana
    {
        private readonly CombatUnit _owner;
        private readonly ICombatSettings _combatSettings;
        private int _current;
        private int _takeDamageManaTrack;

        public int Current => _current;

        // Events
        public event Action<int> OnChange;

        public CombatUnitMana(CombatUnit owner, ICombatSettings combatSettings)
        {
            _owner = owner;
            _combatSettings = combatSettings;
        }

        public void Reset()
        {
            _current = 0;
        }

        public void OnTakeAction(int actionType)
        {
            IReadOnlyList<ActionManaEffect> effects = _combatSettings.ActionManaEffects;
            for (int i = 0; i < effects.Count; i++)
            {
                ActionManaEffect effect = effects[i];
                if (effect.ActionTypeId != actionType) continue;

                switch (effect.Effect)
                {
                    case ManaEffectType.GainAttribute:
                        IncreaseByAttribute();
                        break;
                    case ManaEffectType.GainAmount:
                        Increase((int)effect.Value);
                        break;
                    case ManaEffectType.DrainAll:
                        _current = 0;
                        OnChange?.Invoke(_current);
                        break;
                }
            }
        }

        public void IncreaseByAttribute()
        {
            if (_owner.Attributes.MP == null) return;
            float mpStat = _owner.GetAttributeValue(_owner.Attributes.MP);
            Increase((int)mpStat);
        }

        public void Increase(int amount)
        {
            _current += amount;
            _current = Mathf.Min(_current, _combatSettings.MaxMP);
            OnChange?.Invoke(_current);
        }

        /// <summary>
        /// Tracks mana gain from taking damage. Every 100 damage taken grants mana equal to MP stat.
        /// </summary>
        public void OnTakeDamageManaTrack(int damage)
        {
            _takeDamageManaTrack += damage;
            if (_takeDamageManaTrack >= _combatSettings.TakeDamageManaInterval)
            {
                _takeDamageManaTrack = 0;
                IncreaseByAttribute();
            }
        }

        public int GetNextActionType()
        {
            if (_current >= _combatSettings.MaxMP)
                return _combatSettings.FullManaActionTypeId;
            return _combatSettings.DefaultActionTypeId;
        }

        /// <summary>
        /// Invokes OnChange externally (e.g. when buffs modify MP-related attributes).
        /// </summary>
        public void InvokeOnChange()
        {
            OnChange?.Invoke(_current);
        }
    }
}
