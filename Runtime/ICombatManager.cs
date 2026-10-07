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
        void PlayCriticalEffect(Transform target);
    }
}
