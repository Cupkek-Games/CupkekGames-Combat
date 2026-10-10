using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Combat
{
    /// <summary>
    /// A unit's mana. What its actions and the damage it takes give is multiplied by its
    /// <see cref="CombatUnit.ManaGainMultiplier"/>, the fraction carried to the next gain;
    /// <see cref="Increase"/> adds an exact amount.
    /// </summary>
    public class CombatUnitMana
    {
        private readonly CombatUnit _owner;
        private readonly ICombatSettings _combatSettings;
        private int _current;
        private int _takeDamageManaTrack;
        // The fraction of a gain the multiplier left over, carried to the next.
        private float _carry;

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
            _carry = 0f;
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
                        GainByAttribute();
                        break;
                    case ManaEffectType.GainAmount:
                        Gain((int)effect.Value);
                        break;
                    case ManaEffectType.DrainAll:
                        _current = 0;
                        OnChange?.Invoke(_current);
                        break;
                }
            }
        }

        /// <summary>A gain of the unit's MP attribute, multiplied by its gain multiplier.</summary>
        public void GainByAttribute()
        {
            if (_owner.Attributes.MP == null) return;
            float mpStat = _owner.GetAttributeValue(_owner.Attributes.MP);
            Gain((int)mpStat);
        }

        // A gain from what the unit does: multiplied, the fraction carried.
        private void Gain(int amount)
        {
            _carry += amount * _owner.ManaGainMultiplier;
            int whole = (int)_carry;
            _carry -= whole;
            if (whole > 0) Increase(whole);
        }

        /// <summary>Adds exactly <paramref name="amount"/> (a potion, a test), up to full.</summary>
        public void Increase(int amount)
        {
            _current += amount;
            _current = Mathf.Min(_current, _combatSettings.MaxMP);
            OnChange?.Invoke(_current);
        }

        /// <summary>
        /// Tracks mana gain from taking damage: every <see cref="IAutobattlerSettings.TakeDamageManaInterval"/>
        /// damage taken gains the MP attribute (multiplied).
        /// </summary>
        public void OnTakeDamageManaTrack(int damage)
        {
            _takeDamageManaTrack += damage;
            if (_takeDamageManaTrack >= _combatSettings.TakeDamageManaInterval)
            {
                _takeDamageManaTrack = 0;
                GainByAttribute();
            }
        }

        /// <summary>
        /// Takes up to <paramref name="amount"/> mana and returns what it took.
        /// A bar drained below full no longer selects the ultimate.
        /// </summary>
        public int Drain(int amount)
        {
            int taken = Mathf.Clamp(amount, 0, _current);
            if (taken == 0) return 0;

            _current -= taken;
            OnChange?.Invoke(_current);
            return taken;
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
