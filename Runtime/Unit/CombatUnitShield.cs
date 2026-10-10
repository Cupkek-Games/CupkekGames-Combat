using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CupkekGames.Combat
{
    /// <summary>
    /// A unit's shields, shortest-lived first: damage drains them in that order before health.
    /// A shield ends broken, expired or with its owner gone (<see cref="CombatShieldEndReason"/>),
    /// and <see cref="OnShieldNodeRemove"/> reports it once, after the shields have settled.
    /// </summary>
    [Serializable]
    public class CombatUnitShield
    {
        private List<CombatUnitShieldNode> _shields = new();
        private readonly List<CombatUnitShieldNode> _ended = new();
        public List<CombatUnitShieldNode> Shields => _shields;
        public event Action<int> OnShieldValueChange;
        public event Action<CombatUnitShieldNode> OnShieldNodeAdd;
        public event Action<CombatUnitShieldNode> OnShieldNodeUpdate;
        /// <summary>A shield ended; its <see cref="CombatUnitShieldNode.EndReason"/> says why.</summary>
        public event Action<CombatUnitShieldNode> OnShieldNodeRemove;
        public int TotalShield => _shields.Sum(s => s.Shield);
        private CombatUnit _owner;
        /// <summary>The unit the shields are on.</summary>
        public CombatUnit Owner => _owner;

        public CombatUnitShield(CombatUnit owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Drains <paramref name="damage"/> from the shields in order; each one emptied ends
        /// broken. Returns the damage left for health.
        /// </summary>
        public int Damage(int damage)
        {
            int i = 0;
            while (i < _shields.Count && damage > 0)
            {
                CombatUnitShieldNode shield = _shields[i];
                damage = shield.Absorb(damage);
                if (shield.Shield > 0)
                {
                    i++;
                    continue;
                }

                _shields.RemoveAt(i);
                _ended.Add(shield);
            }

            OnShieldValueChange?.Invoke(TotalShield);
            ReportEnded(CombatShieldEndReason.Broken);

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

                shield.Owner = _owner;
                shield.StartCooldown(_owner, duration, unitDeathToken);
                shield.Expired += OnExpire;

                OnShieldValueChange?.Invoke(TotalShield);
                OnShieldNodeAdd?.Invoke(shield);
            }
        }

        /// <summary>Ends every shield for <paramref name="reason"/> (the owner is gone).</summary>
        public void Clear(CombatShieldEndReason reason)
        {
            if (_shields.Count == 0) return;

            _ended.AddRange(_shields);
            _shields.Clear();
            OnShieldValueChange?.Invoke(0);
            ReportEnded(reason);
        }

        private void OnExpire(CombatUnitShieldNode shield)
        {
            if (!_shields.Remove(shield)) return;

            _ended.Add(shield);
            OnShieldValueChange?.Invoke(TotalShield);
            ReportEnded(CombatShieldEndReason.Expired);
        }

        // Ends the shields set aside, then reports each: a handler may add or drain shields on
        // this unit again, so nothing is reported mid-walk.
        private void ReportEnded(CombatShieldEndReason reason)
        {
            if (_ended.Count == 0) return;

            CombatUnitShieldNode[] ended = _ended.ToArray();
            _ended.Clear();
            foreach (CombatUnitShieldNode shield in ended)
            {
                shield.Expired -= OnExpire;
                shield.End(reason);
            }

            foreach (CombatUnitShieldNode shield in ended)
            {
                OnShieldNodeRemove?.Invoke(shield);
            }
        }
    }
}
