using System;
using UnityEngine;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
  public class CombatActionNodeDamage : CombatActionNodeWithTarget, ICombatActionNodeDescription
  {
    [SerializeField] private AttributeModifier[] _damage;
    [SerializeField] private DamageTypeDefinitionSO _damageType;
    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);

      AttributeModifier modifier = GetDamageValue(ctx.SkillLevel);

      float damage = CombatDamageCalculator.CalculateScaledValue(ctx.Caster, modifier, _damageType.AttackAttribute);

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        CombatActionSO.AttackTarget(ctx.CombatSettings, ctx.CombatManager, ctx.Caster, damage, target, _damageType);
      }

      return BTNodeRuntimeState.Success;
    }

    private AttributeModifier GetDamageValue(int skillLevel)
    {
      return CombatArrayUtils.GetArrayElementOrLast(_damage, skillLevel - 1);
    }

    protected override void OnReset()
    {

    }

    public string GetDescription(int skillLevel, CombatUnit caster, ICombatRules rules)
    {
      AttributeModifier modifier = GetDamageValue(skillLevel);
      string number = ScaledNumber(rules.DescriptionStyle, modifier, caster, _damageType);

      return $"{_damageType.RichTextColor}{number} {_damageType.DisplayName.ToLowerInvariant()} damage{CombatDescriptionStyleSO.CloseTag}";
    }

    /// <summary>
    /// The number a damage-like fragment shows: the flat base alone without a
    /// caster, otherwise the total with the base + attribute breakdown, the
    /// attribute shown as the damage type's inline icon.
    /// </summary>
    public static string ScaledNumber(CombatDescriptionStyleSO style, AttributeModifier modifier, CombatUnit caster,
      DamageTypeDefinitionSO damageType)
    {
      int baseValue = (int)(modifier.Flat + 0.5f);
      if (caster == null)
      {
        return style.Number(baseValue);
      }

      int addition = (int)(CombatDamageCalculator.CalculateAttributeAddition(caster, modifier, damageType.AttackAttribute) + 0.5f);
      return style.Number(baseValue + addition, baseValue, addition, damageType.IconRichText);
    }

    public string GetDescriptionDuration(int skillLevel, CombatUnit caster, ICombatRules rules)
    {
      // Damage is instant; no duration text.
      return string.Empty;
    }
  }
}