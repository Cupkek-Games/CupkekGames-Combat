using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace CupkekGames.Combat
{
    [Serializable]
    public class CombatUnitShield
    {
        private List<CombatUnitShieldNode> _shields = new();
        public List<CombatUnitShieldNode> Shields => _shields;
        public event Action<int> OnShieldValueChange;
        public event Action<CombatUnitShieldNode> OnShieldNodeAdd;
        public event Action<CombatUnitShieldNode> OnShieldNodeUpdate;
        public event Action<CombatUnitShieldNode> OnShieldNodeRemove;
        public int TotalShield => _shields.Sum(s => s.Shield);
        private CombatUnit _caster;
        public CombatUnit Caster => _caster;

        public CombatUnitShield(CombatUnit caster)
        {
            _caster = caster;
        }

        /// <summary>
        /// Applies damage to the combat unit's shields. The damage is distributed across
        /// the shields in the order they are stored. If a shield's health reaches zero,
        /// it is removed from the list of shields.
        /// </summary>
        /// <param name="damage">The amount of damage to apply.</param>
        /// <returns>The remaining damage after all shields have been damaged.</returns>
        public int Damage(int damage)
        {
            int removed = _shields.RemoveAll(shield =>
            {
                damage = shield.Damage(damage);

                bool remove = shield.Shield == 0;

                if (remove)
                {
                    OnShieldNodeRemove?.Invoke(shield);
                }

                return remove;
            });

            OnShieldValueChange?.Invoke(TotalShield);

            return damage;
        }

        public void AddShield(CombatUnitShieldNode shield, float duration, CancellationToken unitDeathToken)
        {
            CombatUnitShieldNode existingShield = null;

            for (int i = 0; i < _shields.Count; i++)
            {
                if (_shields[i].ID == shield.ID)
                {
                    existingShield = _shields[i];
                    break;
                }
            }

            if (existingShield != null)
            {
                if (duration > existingShield.Duration)
                {
                    existingShield.SetDuration(duration);
                }

                existingShield.AddShield(shield.Shield, false); // Stack shields

                OnShieldValueChange?.Invoke(TotalShield);
                OnShieldNodeUpdate?.Invoke(existingShield);
            }
            else
            {
                // Use duration parameter for the new shield since its countdown hasn't started yet.
                // Also guard against disposed/null countdowns on existing shields.
                float GetDuration(CombatUnitShieldNode node) =>
                    node == shield ? duration : (node.Countdown != null ? node.Duration : 0f);

                int index = _shields.BinarySearch(shield,
                    Comparer<CombatUnitShieldNode>.Create((a, b) => GetDuration(a).CompareTo(GetDuration(b))));

                if (index < 0) index = ~index; // Convert negative index to insertion point

                _shields.Insert(index, shield);

                shield.StartCooldown(_caster, duration, unitDeathToken);
                shield.OnExpire += OnExpire;

                OnShieldValueChange?.Invoke(TotalShield);
                OnShieldNodeAdd?.Invoke(shield);
            }
        }

        private void OnExpire(CombatUnitShieldNode shield)
        {
            _shields.Remove(shield);

            OnShieldValueChange?.Invoke(TotalShield);
            OnShieldNodeRemove?.Invoke(shield);
        }
    }
}