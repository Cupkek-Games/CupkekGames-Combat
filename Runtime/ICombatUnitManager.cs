using System.Collections.ObjectModel;
using UnityEngine;

namespace CupkekGames.Combat
{
    public interface ICombatUnitManager
    {
        ReadOnlyCollection<CombatUnit> CombatUnitsAlly { get; }
        ReadOnlyCollection<CombatUnit> CombatUnitsEnemy { get; }

        /// <summary>The fight's space: distances, areas and movement. Never null during a fight.</summary>
        ICombatSpace Space { get; }
        void SetTimeScale(float timeScale, CombatUnit except);
        void SpawnEnemy(CombatUnitReference enemy, Vector2Int? position = null);
    }
}
