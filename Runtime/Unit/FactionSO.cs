using UnityEngine;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Authored faction definition. Identity is the asset name (used as the catalog key);
    /// register `FactionCatalog` under <see cref="CombatConstants.FactionsCatalogId"/> and
    /// reference factions via <see cref="CupkekGames.Data.CatalogKey"/> from saveable data.
    /// </summary>
    [CreateAssetMenu(fileName = "Faction", menuName = "CupkekGames/Combat/Faction")]
    public class FactionSO : ScriptableObject
    {
        [SerializeField] private string _displayName;
        public string DisplayName => _displayName;

        [SerializeField] private Color _color = Color.white;
        public Color Color => _color;
    }
}
