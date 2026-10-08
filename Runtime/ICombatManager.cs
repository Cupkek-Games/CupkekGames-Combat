using System.Threading;
using CupkekGames.Cameras;
using CupkekGames.TextPopup;
using UnityEngine;

namespace CupkekGames.Combat
{
    public interface ICombatManager
    {
        ICombatUnitManager UnitManager { get; }
        IPopupManager PopupManager { get; }
        EventDatabaseCombat EventDatabase { get; }
        CombatUltimateManager CombatUltimateManager { get; }
        CinemachineManager CinemachineManager { get; }
        CancellationTokenSource CancelToken { get; }

        /// <summary>The fight's random numbers (crits, action ties), seeded per fight.</summary>
        CombatRandom Random { get; }
        /// <summary>A crit by <paramref name="attacker"/> (null for none) landed on <paramref name="target"/>: the game's crit feedback, if any.</summary>
        void PlayCriticalEffect(CombatUnit attacker, Transform target);
    }
}
