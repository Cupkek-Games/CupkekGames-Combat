using System;
using UnityEngine;
using CupkekGames.Luna;
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

    public string GetDescription(int skillLevel, CombatUnit caster)
    {
      AttributeModifier modifier = GetDamageValue(skillLevel);
      int baseValue = (int)(modifier.Flat + 0.5f);

      if (caster == null)
      {
        return $"{_damageType.RichTextColor}{baseValue} {_damageType.DisplayName.ToLowerInvariant()} damage{RichTextColor.CLOSING_TAG}";
      }

      int addition = (int)(CombatDamageCalculator.CalculateAttributeAddition(caster, modifier, _damageType.AttackAttribute) + 0.5f);
      int total = baseValue + addition;

      return $"{_damageType.RichTextColor}{total}({baseValue}+{addition}{_damageType.IconRichText}) {_damageType.DisplayName} damage{RichTextColor.CLOSING_TAG}";
    }

    public string GetDescriptionDuration(int skillLevel, CombatUnit caster)
    {
      // Damage is instant; no duration text.
      return string.Empty;
    }
  }
}