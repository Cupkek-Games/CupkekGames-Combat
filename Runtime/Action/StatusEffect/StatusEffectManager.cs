using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace CupkekGames.Combat
{
  public class StatusEffectManager : MonoBehaviour
  {
    [SerializeField] private List<StatusEffectSO> _statusEffects = new();
    [SerializeField] private VisualTreeAsset _statusEffectTemplate;
    public VisualTreeAsset StatusEffectTemplate => _statusEffectTemplate;
    public IReadOnlyList<StatusEffectSO> All => _statusEffects;
  }
}