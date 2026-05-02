using UnityEngine;

namespace CupkekGames.Combat
{
  [CreateAssetMenu(fileName = "CombatAttributeScalingTier", menuName = "CupkekGames/Combat/Attribute Scaling Tier")]
  public class CombatAttributeScalingTierSO : ScriptableObject
  {
    [SerializeField] private float _growthRate;
    public float GrowthRate => _growthRate;

    public float GetMultiplier(int level, float tierMultiplier)
    {
      if (_growthRate == 0f) return 1f;
      return 1 + ((level - 1) * _growthRate * tierMultiplier);
    }
  }
}