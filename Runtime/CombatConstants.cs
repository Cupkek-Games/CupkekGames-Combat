using CupkekGames.Data;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Catalog id constants for combat content. Register an <see cref="AssetCatalog{T}"/> per id;
    /// content references via <see cref="CatalogKey"/> use these strings.
    /// </summary>
    public static class CombatConstants
    {
        public const string FactionsCatalogId = "Factions";
        public const string StatusEffectsCatalogId = "StatusEffects";
        public const string CombatStagesCatalogId = "CombatStages";
        public const string CombatTiersCatalogId = "CombatTiers";
        public const string CombatActionsCatalogId = "CombatActions";
        public const string CombatWavesCatalogId = "CombatWaves";
    }
}
