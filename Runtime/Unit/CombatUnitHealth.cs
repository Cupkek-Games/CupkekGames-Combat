using System;
using UnityEngine;

namespace CupkekGames.Combat
{
    public class CombatUnitHealth
    {
        private readonly CombatUnit _owner;
        private int _current;

        public int Current
        {
            get => _current;
            set
            {
                _current = value;
                if (_current < 0)
                {
                    _current = 0;
                }
            }
        }

        // Events
        public event Action<int> OnChange;
        public event Action<CombatUnit, int, CombatUnit> OnTakeDamage;
        public event Action<int, CombatUnit> OnHeal;

        public CombatUnitHealth(CombatUnit owner)
        {
            _owner = owner;
        }

        public void Reset()
        {
            _current = (int)(_owner.GetAttributeValue(_owner.Attributes.HP) + 0.5f);
        }

        public void TakeDamage(int damage, CombatUnit damager)
        {
            if (_current <= 0)
            {
                return;
            }

            int remainingDamage = damage;
            int actualDamage = 0;

            if (_owner.Shield.TotalShield > 0)
            {
                remainingDamage = _owner.Shield.Damage(damage);
                actualDamage = damage - remainingDamage;
            }

            if (remainingDamage > 0)
            {
                int remainingHP = Mathf.Max(_current - remainingDamage, 0);
                actualDamage += _current - remainingHP;
                _current = remainingHP;
                OnChange?.Invoke(_current);
            }

            _owner.Mana.OnTakeDamageManaTrack(actualDamage);
            OnTakeDamage?.Invoke(_owner, actualDamage, damager);

            if (_current == 0)
            {
                _owner.OnDeath();
            }
        }

        public void Heal(int heal, CombatUnit healer)
        {
            _current += heal;
            int maxHP = (int)_owner.GetAttributeValue(_owner.Attributes.HP);
            if (_current > maxHP)
            {
                _current = maxHP;
            }

            OnChange?.Invoke(_current);
            OnHeal?.Invoke(heal, healer);
        }

        /// <summary>
        /// Invokes OnChange externally (e.g. when buffs modify HP).
        /// </summary>
        public void InvokeOnChange()
        {
            OnChange?.Invoke(_current);
        }
    }
}
