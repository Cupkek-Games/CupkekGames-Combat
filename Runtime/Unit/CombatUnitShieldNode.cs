using System;
using System.Threading;
using CupkekGames.Luna;
using CupkekGames.InventorySystem;
using CupkekGames.TimeSystem;
using UnityEngine;

namespace CupkekGames.Combat
{
    [Serializable]
    public class CombatUnitShieldNode
    {
        private Guid _id;
        public Guid ID => _id;
        private int _shield;
        public int Shield => _shield;

        private float _startDuration = 0;
        public float StartDuration => _startDuration;
        private CountdownTimeContext _countdown;
        public CountdownTimeContext Countdown => _countdown;
        public float Duration
        {
            get
            {
                return _countdown.Value;
            }
            set
            {
                _countdown.Value = value;
            }
        }
        public event Action<int> OnChange;
        public event Action<CombatUnitShieldNode> OnExpire;
        // Display
        public static UIColor Color = new UIColor("amber", UIColorValue.V_400);
        // Buff
        private CombatAttributeDataEffectRuntime _buff;
        public CombatAttributeDataEffectRuntime Buff => _buff;

        public CombatUnitShieldNode(int shield, Guid id, CombatAttributeDataEffectRuntime buff)
        {
            if (id == null)
            {
                _id = Guid.NewGuid();
            }
            else
            {
                _id = id;
            }

            _buff = buff;
            _shield = shield;
        }

        public int Damage(int damage)
        {
            int remainingDamage = Math.Max(0, damage - _shield);
            _shield = Math.Max(0, _shield - damage);

            OnChange?.Invoke(_shield);

            if (_shield == 0)
            {
                _countdown.Dispose();

                OnExpire?.Invoke(this);
            }

            return remainingDamage;
        }

        public void AddShield(int amount, bool notice)
        {
            _shield += amount;
            if (notice)
            {
                OnChange?.Invoke(_shield);
            }
        }
        public void SetShield(int amount, bool notice)
        {
            _shield = amount;

            if (notice)
            {
                OnChange?.Invoke(_shield);
            }
        }

        public void StartCooldown(CombatUnit caster, float duration, CancellationToken skillCancelToken)
        {
            _countdown?.Dispose();

            _startDuration = duration;

            _countdown = new CountdownTimeContext(caster.TimeBundle.TimeContext, _startDuration, 0, StatusEffect.INTERVAL_VISUAL, skillCancelToken);
            _countdown.OnComplete += OnComplete;
            _countdown.Start();
        }

        public void SetDuration(float duration)
        {
            _countdown.Value = duration;
        }

        private void OnComplete()
        {
            _countdown.Dispose();

            OnExpire?.Invoke(this);
        }
        public ItemStatDisplayLine GetShieldLine()
        {
            return new ItemStatDisplayLine
            {
                Label = "Shield",
                Value = _shield.ToString(),
                Color = new Color(0.569f, 0.784f, 1f)
            };
        }
    }
}
