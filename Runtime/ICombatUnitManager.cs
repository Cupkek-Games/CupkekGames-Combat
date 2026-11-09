using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CupkekGames.Combat
{
    public interface ICombatUnitManager
    {
        ReadOnlyCollection<CombatUnit> CombatUnitsAlly { get; }
        ReadOnlyCollection<CombatUnit> CombatUnitsEnemy { get; }

        /// <summary>The fight's space: distances, areas and movement. Never null during a fight.</summary>
        ICombatSpace Space { get; }

        /// <summary>How the fight's units pick whom they go after (<see cref="CombatThreatTargeting"/> is the package's own). Never null during a fight.</summary>
        ICombatTargeting Targeting { get; }

        /// <summary>
        /// Holds <paramref name="units"/>' clocks at <paramref name="scale"/> (0 freezes them) on a layer
        /// owned by <paramref name="owner"/>, until the handle is disposed. Other holders' layers stay;
        /// a unit that falls while held leaves the hold at once (<see cref="CombatUnitTimeHold"/>).
        /// </summary>
        IDisposable HoldUnits(object owner, IEnumerable<CombatUnit> units, float scale);

        /// <summary>Brings <paramref name="unit"/> into the running fight on the enemy side, as near <paramref name="summoner"/> as the space allows (the game decides where), and starts it.</summary>
        void Summon(CombatUnitReference unit, CombatUnit summoner);
    }
}
