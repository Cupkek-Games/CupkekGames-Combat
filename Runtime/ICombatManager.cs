using System.Threading;
using CupkekGames.Cameras;
using CupkekGames.ShapeDrawing;
using CupkekGames.TextPopup;
using UnityEngine;

namespace CupkekGames.Combat
{
    public interface ICombatManager
    {
        ICombatUnitManager UnitManager { get; }
        IDamagePopup DamagePopup { get; }
        IHealPopup HealPopup { get; }
        IStatusPopup StatusPopup { get; }
        EventDatabaseCombat EventDatabase { get; }
        CombatUltimateManager CombatUltimateManager { get; }
        IIndicatorPool IndicatorPool { get; }
        CinemachineManager CinemachineManager { get; }
        CancellationTokenSource CancelToken { get; }
        void PlayCriticalEffect(Transform target);
    }
}
