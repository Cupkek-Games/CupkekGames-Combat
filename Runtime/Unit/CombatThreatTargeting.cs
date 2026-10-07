using System;
using System.Collections.ObjectModel;

namespace CupkekGames.Combat
{
    /// <summary>
    /// The package's targeting: a unit goes after whoever tops its threat table
    /// (<see cref="CombatUnitAI.CombatUnitThreatTable"/>). Hits from actions draw threat on
    /// their attacker, and each ask adds threat on the nearest opponent, falling with the
    /// square of its distance, so a unit drifts toward whoever hurts it most.
    /// </summary>
    public sealed class CombatThreatTargeting : ICombatTargeting
    {
        private readonly ICombatUnitManager _units;
        private readonly int _distanceThreat;

        /// <param name="interval">Seconds between asks while a unit walks or waits.</param>
        /// <param name="distanceThreat">The threat each ask adds on the nearest opponent at one combat unit away (divided by the distance squared).</param>
        public CombatThreatTargeting(ICombatUnitManager units, float interval, int distanceThreat)
        {
            if (interval < 0f) throw new ArgumentOutOfRangeException(nameof(interval), interval, "An interval is never negative.");

            _units = units ?? throw new ArgumentNullException(nameof(units));
            Interval = interval;
            _distanceThreat = distanceThreat;
        }

        public float Interval { get; }

        public CombatUnit Pick(CombatUnit caster, CombatUnit current, float range)
        {
            ReadOnlyCollection<CombatUnit> opponents = _units.CombatUnitsAlly.Contains(caster) ? _units.CombatUnitsEnemy : _units.CombatUnitsAlly;
            if (opponents.Count == 0) return current;

            CombatUnitThreatTable table = caster.View.CombatUnitAI.CombatUnitThreatTable;
            if (opponents.Count == 1)
            {
                table.AddThreat(opponents[0], _distanceThreat);
            }
            else
            {
                (CombatUnit closest, float distanceSqr) = Closest(caster, opponents);
                table.AddThreat(closest, (int)(_distanceThreat / distanceSqr));
            }

            return table.GetHighestThreatTarget();
        }

        private (CombatUnit, float) Closest(CombatUnit caster, ReadOnlyCollection<CombatUnit> opponents)
        {
            ICombatSpace space = _units.Space;
            CombatUnit closest = null;
            float closestSqr = float.PositiveInfinity;
            foreach (CombatUnit opponent in opponents)
            {
                if (opponent?.View == null || opponent.Health.Current <= 0) continue;

                float distance = space.Distance(caster, opponent);
                float sqr = distance * distance;
                if (sqr < closestSqr)
                {
                    closestSqr = sqr;
                    closest = opponent;
                }
            }

            return (closest, closestSqr);
        }
    }
}
