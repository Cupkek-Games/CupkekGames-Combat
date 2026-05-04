using System.Collections.Generic;
using UnityEngine;
using CupkekGames.Luna;
using CupkekGames.Data.Primitives;
using CupkekGames.VFX;

namespace CupkekGames.Combat
{
  [CreateAssetMenu(fileName = "StatusEffect", menuName = "CupkekGames/Combat/StatusEffect/Status Effect")]
  public class StatusEffectSO : ScriptableObject
  {
    public SerializedGuid ID = new SerializedGuid(System.Guid.NewGuid());
    public string Name;
    [TextAreaAttribute] public string Description;
    public Sprite Icon;
    public int Priority = 0;
    public bool HasTier;
    public UIColor Color;
    [SerializeField] public VFXBundle VFXBundle;

    [SerializeReference] private List<IStatusEffectBehaviorFeature> _behaviors = new List<IStatusEffectBehaviorFeature>();

    public void OnStart(ICombatSettings combatSettings, ICombatManager manager,
      CombatUnit caster, CombatUnit primaryTarget, int skillLevel)
    {
      foreach (IStatusEffectBehaviorFeature b in _behaviors)
        b?.OnStart(combatSettings, manager, caster, primaryTarget, skillLevel);
    }

    public void OnTick(ICombatSettings combatSettings, ICombatManager manager,
      CombatUnit caster, CombatUnit primaryTarget, int skillLevel)
    {
      foreach (IStatusEffectBehaviorFeature b in _behaviors)
        b?.OnTick(combatSettings, manager, caster, primaryTarget, skillLevel);
    }

    public void OnEnd(ICombatSettings combatSettings, ICombatManager manager,
      CombatUnit caster, CombatUnit primaryTarget, int skillLevel)
    {
      foreach (IStatusEffectBehaviorFeature b in _behaviors)
        b?.OnEnd(combatSettings, manager, caster, primaryTarget, skillLevel);
    }

    public string GetName(int skillLevel)
    {
      return Name + " T" + skillLevel;
    }

    public string GetDescription(int skillLevel, CombatUnit caster = null)
    {
      return "StatusEffectSO GetDescription";
    }

    public void Prewarm(GameObject parent)
    {
      if (VFXBundle != null)
      {
        VFXBundle.Prewarm(parent);
      }
    }
  }
}