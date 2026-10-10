using System;
using System.Threading;
using CupkekGames.Luna;
using CupkekGames.InventorySystem;
using CupkekGames.TimeSystem;
using UnityEngine;
using Unity.Scripting.LifecycleManagement;

namespace CupkekGames.Combat
{
    /// <summary>Why a shield ended.</summary>
    public enum CombatShieldEndReason
    {
        /// <summary>Still up.</summary>
        None,
        /// <summary>Damage emptied it.</summary>
        Broken,
        /// <summary>Its time ran out with some left.</summary>
        Expired,
        /// <summary>Its owner left the fight (fell, or the fight ended).</summary>
        OwnerGone,
    }

    /// <summary>
    /// One shield on a unit: who cast it, how much it granted, how much it has absorbed and
    /// has left, and, once over, why it ended.
    /// </summary>
    [Serializable]
    public partial class CombatUnitShieldNode
    {
        private Guid _id;
        public Guid ID => _id;
        private int _shield;
        /// <summary>What it has left.</summary>
        public int Shield => _shield;

        /// <summary>The unit that cast it; null when nobody did.</summary>
        public CombatUnit Caster { get; }

        /// <summary>The unit it is on; set when it is added.</summary>
        public CombatUnit Owner { get; internal set; }

        /// <summary>What it granted in all: its amount, and what was stacked on it since.</summary>
        public int Granted { get; private set; }

        /// <summary>The damage it has taken.</summary>
        public int Absorbed { get; private set; }

        /// <summary>Why it ended; <see cref="CombatShieldEndReason.None"/> while it is up.</summary>
        public CombatShieldEndReason EndReason { get; private set; }

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
        // Its countdown ran out; its owner's shields end it.
        internal event Action<CombatUnitShieldNode> Expired;
        // Display
        [NoAutoStaticsCleanup]
        public static UIColor Color = new UIColor("amber", UIColorValue.V_400);
        // Buff
        private CombatAttributeDataEffectRuntime _buff;
        public CombatAttributeDataEffectRuntime Buff => _buff;

        public CombatUnitShieldNode(int shield, Guid id, CombatAttributeDataEffectRuntime buff, CombatUnit caster = null)
        {
            _id = id == Guid.Empty ? Guid.NewGuid() : id;
            _buff = buff;
            _shield = shield;
            Granted = shield;
            Caster = caster;
        }

        // Takes what it can of the damage; returns what is left.
        internal int Absorb(int damage)
        {
            int taken = Math.Min(Math.Max(0, damage), _shield);
            _shield -= taken;
            Absorbed += taken;

            OnChange?.Invoke(_shield);

            return damage - taken;
        }

        public void AddShield(int amount, bool notice)
        {
            _shield += amount;
            Granted += amount;
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

        public void StartCooldown(CombatUnit owner, float duration, CancellationToken skillCancelToken)
        {
            _countdown?.Dispose();

            _startDuration = duration;

            _countdown = new CountdownTimeContext(owner.TimeBundle.TimeContext, _startDuration, 0, StatusEffect.INTERVAL_VISUAL, skillCancelToken);
            _countdown.OnComplete += OnComplete;
            _countdown.Start();
        }

        public void SetDuration(float duration)
        {
            _countdown.Value = duration;
        }

        internal void End(CombatShieldEndReason reason)
        {
            _countdown?.Dispose();
            EndReason = reason;
        }

        private void OnComplete()
        {
            Expired?.Invoke(this);
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
