using System;
using UnityEngine;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Drains its wearer's mana (<see cref="CombatUnitMana.Drain"/>): an amount per
    /// effect level when it lands, and another on every tick. A bar drained below
    /// full no longer selects the ultimate.
    /// </summary>
    [Serializable]
    public class ManaDrainBehavior : IStatusEffectBehaviorFeature
    {
        [Min(0)] [SerializeField] private int _onStartPerLevel = 25;
        [Min(0)] [SerializeField] private int _perTickPerLevel;

        public void OnStart(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer)
        {
            if (_onStartPerLevel > 0) wearer.Mana.Drain(_onStartPerLevel * effect.Level);
        }

        public void OnTick(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer)
        {
            if (_perTickPerLevel > 0) wearer.Mana.Drain(_perTickPerLevel * effect.Level);
        }

        public void OnEnd(ICombatSettings combatSettings, ICombatManager manager, StatusEffect effect, CombatUnit wearer) { }
    }
}
