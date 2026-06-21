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
                // TryGetValue: a miss in one catalog of a multi-catalog probe
                // is expected and must not trigger GetValue's dev-build warning.
                if (catalogs[i] != null && catalogs[i].TryGetValue(key, out var obj) && obj is StatusEffectSO def)
                    return def;
            }
            return null;
        }
    }
}
