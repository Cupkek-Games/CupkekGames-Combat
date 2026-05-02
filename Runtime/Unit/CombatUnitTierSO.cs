using UnityEngine;

namespace CupkekGames.Combat
{
  [CreateAssetMenu(fileName = "CombatUnitTier", menuName = "CupkekGames/Combat/Combat Unit Tier")]
  public class CombatUnitTierSO : ScriptableObject
  {
    [SerializeField] private int _sortOrder;
    public int SortOrder => _sortOrder;

    [SerializeField] private string _displayName;
    public string DisplayName => _displayName;

    [SerializeField] private string _ussClassName;
    public string USSClassName => _ussClassName;

    [SerializeField] private float _combatAttributeScalingMultiplier = 1f;
    public float CombatAttributeScalingMultiplier => _combatAttributeScalingMultiplier;
  }
}
