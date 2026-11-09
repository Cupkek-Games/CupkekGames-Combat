using System;
using System.Collections.Generic;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Holds a set of units' clocks at one scale (0 freezes them) through a scale layer on each
    /// unit's time context, until disposed. Units already fallen are left out, and a unit that falls
    /// while held leaves the hold at once, so its fall plays out. Other holders' layers stay untouched.
    /// </summary>
    public sealed class CombatUnitTimeHold : IDisposable
    {
        private readonly Dictionary<CombatUnit, IDisposable> _layers = new();

        public CombatUnitTimeHold(object owner, IEnumerable<CombatUnit> units, float scale)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (units == null) throw new ArgumentNullException(nameof(units));

            foreach (CombatUnit unit in units)
            {
                if (unit == null || !unit.IsAlive || _layers.ContainsKey(unit)) continue;

                _layers.Add(unit, unit.TimeBundle.TimeContext.Scale(owner, scale));
                unit.OnDeathEvent += Release;
            }
        }

        /// <summary>The units this hold still holds.</summary>
        public IReadOnlyCollection<CombatUnit> Units => _layers.Keys;

        private void Release(CombatUnit unit)
        {
            if (!_layers.Remove(unit, out IDisposable layer)) return;

            unit.OnDeathEvent -= Release;
            layer.Dispose();
        }

        public void Dispose()
        {
            // A layer's release can wake handlers that touch this hold: empty it first.
            KeyValuePair<CombatUnit, IDisposable>[] held = new KeyValuePair<CombatUnit, IDisposable>[_layers.Count];
            ((ICollection<KeyValuePair<CombatUnit, IDisposable>>)_layers).CopyTo(held, 0);
            _layers.Clear();
            foreach (KeyValuePair<CombatUnit, IDisposable> pair in held)
            {
                pair.Key.OnDeathEvent -= Release;
                pair.Value.Dispose();
            }
        }
    }
}
