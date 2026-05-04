using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.Combat
{
    [CreateAssetMenu(
        fileName = "FactionCatalog",
        menuName = "CupkekGames/Combat/Catalog/Factions")]
    public class FactionCatalog : AssetCatalog<FactionSO> { }
}
