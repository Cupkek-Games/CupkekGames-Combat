using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.Combat
{
    [CreateAssetMenu(
        fileName = "CombatStageCatalog",
        menuName = "CupkekGames/Combat/Catalog/Combat Stages")]
    public class CombatStageCatalog : AssetCatalog<CombatStageSO> { }
}
