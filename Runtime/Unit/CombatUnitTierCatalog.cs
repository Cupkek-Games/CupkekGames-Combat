using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.Combat
{
    [CreateAssetMenu(
        fileName = "CombatUnitTierCatalog",
        menuName = "CupkekGames/Combat/Catalog/Combat Unit Tiers")]
    public class CombatUnitTierCatalog : AssetCatalog<CombatUnitTierSO> { }
}
