using CupkekGames.Data;
using UnityEngine;

namespace CupkekGames.Combat
{
    [CreateAssetMenu(
        fileName = "StatusEffectCatalog",
        menuName = "CupkekGames/Combat/Catalog/Status Effects")]
    public class StatusEffectCatalog : AssetCatalog<StatusEffectSO> { }
}
