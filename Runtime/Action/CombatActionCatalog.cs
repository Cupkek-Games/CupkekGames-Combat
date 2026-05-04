using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.Combat
{
    [CreateAssetMenu(
        fileName = "CombatActionCatalog",
        menuName = "CupkekGames/Combat/Catalog/Combat Actions")]
    public class CombatActionCatalog : AssetCatalog<CombatActionSO> { }
}
