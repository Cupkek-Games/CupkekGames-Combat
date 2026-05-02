using UnityEngine;

namespace CupkekGames.Combat
{
    [CreateAssetMenu(fileName = "Faction", menuName = "CupkekGames/Combat/Faction")]
    public class FactionSO : ScriptableObject
    {
        [SerializeField] private int _factionId;
        public int FactionId => _factionId;

        [SerializeField] private string _displayName;
        public string DisplayName => _displayName;

        [SerializeField] private Color _color = Color.white;
        public Color Color => _color;
    }
}
