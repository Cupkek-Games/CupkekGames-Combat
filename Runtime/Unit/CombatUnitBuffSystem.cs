using System;
using System.Collections.Generic;

namespace CupkekGames.Combat
{
    public class CombatUnitBuffSystem
    {
        private readonly CombatUnit _owner;
        private readonly List<CombatAttributeDataEffectRuntime> _buffs = new();

        public List<CombatAttributeDataEffectRuntime> All => _buffs;

        // Events
        public event Action<CombatAttributeDataEffectRuntime> OnAdd;
        public event Action<CombatAttributeDataEffectRuntime> OnUpdate;
        public event Action<CombatAttributeDataEffectRuntime> OnRemove;
        public event Action OnClear;

        public CombatUnitBuffSystem(CombatUnit owner)
        {
            _owner = owner;
        }

        public void Add(CombatAttributeDataEffectRuntime effect)
        {
            CombatAttributeDataEffectRuntime current = TryGet(effect.ID);

            bool added = false;

            if (current != null)
            {
                if (current.Level > effect.Level)
                {
                    return;
                }
                else if (current.Level == effect.Level)
                {
                    if (current.Duration < 0)
                    {
                        return;
                    }
                    else if (effect.Duration < 0)
                    {
                        current.StopCooldown();
                        current.Duration = -1;
                        OnUpdate?.Invoke(current);
                    }
                    else
                    {
                        current.Duration = effect.Duration > current.Duration ? effect.Duration : current.Duration;
                        OnUpdate?.Invoke(current);
                    }
                }
                else
                {
                    current.Level = effect.Level;
                    current.Duration = effect.Duration;
                    OnUpdate?.Invoke(current);
                }
            }
            else
            {
                _buffs.Add(effect);
                added = true;
            }

            if (added)
            {
                if (effect.Duration > 0)
                {
                    effect.OnEnd += Remove;
                    effect.StartCooldown(_owner);
                }

                OnAdd?.Invoke(effect);
            }

            NotifyOwnerOfChange(effect);
        }

        public void Remove(Guid id)
        {
            CombatAttributeDataEffectRuntime buff = TryGet(id);
            if (buff != null)
            {
                Remove(buff);
            }
        }

        private void Remove(CombatAttributeDataEffectRuntime buff)
        {
            buff.OnEnd -= Remove;

            _buffs.Remove(buff);
            buff.StopCooldown();

            NotifyOwnerOfChange(buff);
            OnRemove?.Invoke(buff);
        }

        public void Clear()
        {
            foreach (CombatAttributeDataEffectRuntime buff in _buffs)
            {
                buff.OnEnd -= Remove;
                buff.StopCooldown();
            }

            _buffs.Clear();
            OnClear?.Invoke();
        }

        public CombatAttributeDataEffectRuntime TryGet(Guid id)
        {
            foreach (CombatAttributeDataEffectRuntime buff in _buffs)
            {
                if (buff.ID == id)
                {
                    return buff;
                }
            }

            return null;
        }

        /// <summary>
        /// Stops all buff cooldowns without firing individual remove events.
        /// Used during death cleanup.
        /// </summary>
        public void DisposeAll()
        {
            foreach (CombatAttributeDataEffectRuntime buff in _buffs)
            {
                buff.StopCooldown();
            }

            _buffs.Clear();
        }

        private void NotifyOwnerOfChange(CombatAttributeDataEffectRuntime buff)
        {
            if (buff.Data.DoesChange(_owner.Attributes.HP))
            {
                _owner.Health.InvokeOnChange();
            }

            if (_owner.Attributes.MP != null && buff.Data.DoesChange(_owner.Attributes.MP))
            {
                _owner.Mana.InvokeOnChange();
            }
        }
    }
}
