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
        void SetTimeScale(float timeScale, CombatUnit except);
        /// <summary>Brings <paramref name="unit"/> into the running fight on the enemy side, as near <paramref name="summoner"/> as the space allows (the game decides where), and starts it.</summary>
        void Summon(CombatUnitReference unit, CombatUnit summoner);
    }
}
