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
    [Tooltip("What the damage scales off: the caster's attack attribute, or a share of each target's max health.")]
    [SerializeField] private CombatValueScaling _scaling = CombatValueScaling.CasterAttribute;
    [Tooltip("Exact: no crit, no defense and no damage modifiers; the attacker's element still counts.")]
    [SerializeField] private bool _exact;

    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);

      AttributeModifier modifier = GetDamageValue(ctx.SkillLevel);

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        float damage = CombatDamageCalculator.CalculateScaledValue(ctx.Caster, target, modifier, _scaling, _damageType.AttackAttribute);
        if (_exact)
        {
          DamageResult result = CombatDamageCalculator.CalculateExactDamage(ctx.CombatSettings, ctx.Caster, target, damage, ctx.Source);
          CombatDamageCalculator.ApplyDamageAndVisuals(ctx.CombatSettings, ctx.CombatManager, ctx.Caster, target, result);
        }
        else
        {
          CombatActionSO.AttackTarget(ctx.CombatSettings, ctx.CombatManager, ctx.Caster, damage, target, _damageType, ctx.Source);
        }
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
      string number = _scaling == CombatValueScaling.TargetMaxHealth
        ? ShareNumber(rules.DescriptionStyle, _damageType.IconRichText, modifier)
        : ScaledNumber(rules.DescriptionStyle, _damageType.IconRichText, modifier, caster, _damageType);

      return $"{_damageType.RichTextColor}{number} {_damageType.DisplayName.ToLowerInvariant()} damage{CombatDescriptionStyleSO.CloseTag}";
    }

    /// <summary>
    /// The number a damage-like fragment shows: the fragment's icon, then the
    /// flat base alone without a caster, otherwise the total with the base +
    /// attribute breakdown. The damage type only supplies the scaling
    /// attribute here.
    /// </summary>
    public static string ScaledNumber(CombatDescriptionStyleSO style, string icon, AttributeModifier modifier,
      CombatUnit caster, DamageTypeDefinitionSO damageType)
    {
      int baseValue = (int)(modifier.Flat + 0.5f);
      if (caster == null)
      {
        return style.Number(icon, baseValue);
      }

      int addition = (int)(CombatDamageCalculator.CalculateAttributeAddition(caster, modifier, damageType.AttackAttribute) + 0.5f);
      return style.Number(icon, baseValue + addition, baseValue, addition);
    }

    /// <summary>
    /// The number a share-of-max-health fragment shows: "20% of max health",
    /// after the flat part when one is authored.
    /// </summary>
    public static string ShareNumber(CombatDescriptionStyleSO style, string icon, AttributeModifier modifier)
    {
      string share = $"{style.Number(icon, Mathf.RoundToInt(modifier.Multiplier * 100f))}% of max health";
      int flat = (int)(modifier.Flat + 0.5f);
      return flat != 0 ? $"{style.Number(string.Empty, flat)} + {share}" : share;
    }

    public string GetDescriptionDuration(int skillLevel, CombatUnit caster, ICombatRules rules)
    {
      // Damage is instant; no duration text.
      return string.Empty;
    }
  }
}