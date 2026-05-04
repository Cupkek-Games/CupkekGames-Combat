using System.Collections.Generic;
using CupkekGames.Data;
using CupkekGames.Services;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Resolves <see cref="StatusEffectSO"/> assets by catalog key from any registered
    /// <see cref="StatusEffectCatalog"/> under <see cref="CombatConstants.StatusEffectsCatalogId"/>.
    /// </summary>
    public static class StatusEffectResolver
    {
        public static StatusEffectSO GetOrNull(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            IReadOnlyList<IAssetCatalog<StatusEffectSO>> catalogs =
                ServiceLocator.GetAll<IAssetCatalog<StatusEffectSO>>(CombatConstants.StatusEffectsCatalogId);
            for (int i = 0; i < catalogs.Count; i++)
            {
                StatusEffectSO def = catalogs[i]?.GetValue(key);
                if (def != null) return def;
            }
            return null;
        }
    }
}
