using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.Combat
{
    [CreateAssetMenu(
        fileName = "CombatWaveCatalog",
        menuName = "CupkekGames/Combat/Catalog/Combat Waves")]
    public class CombatWaveCatalog : AssetCatalog<CombatWaveSO> { }
}
