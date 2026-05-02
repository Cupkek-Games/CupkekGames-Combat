using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.Luna;
using CupkekGames.BehaviourTrees;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
  public class CombatActionNodeHeal : CombatActionNodeWithTarget, ICombatActionNodeDescription
  {
    [SerializeField] private AttributeModifier[] _damage;
    [SerializeField] private DamageTypeDefinitionSO _damageType;

    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {
      if (_damage.Length == 0)
      {
        return BTNodeRuntimeState.Success;
      }

      var ctx = CombatActionContext.From(Blackboard);

      AttributeModifier modifier = GetDamageValue(ctx.SkillLevel);

      int damage = (int)(CombatDamageCalculator.CalculateScaledValue(ctx.Caster, modifier, _damageType.AttackAttribute) + 0.5f);

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        ctx.CombatManager.HealPopup.ShowHeal(target.CombatUnitGameObject.HealthBarTransform.position, damage);
        target.Health.Heal(damage, ctx.Caster);
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
      if (modifier == null)
      {
        Debug.LogError($"Modifier is null for skill level {skillLevel}");
        return "";
      }

      int baseValue = (int)(modifier.Flat + 0.5f);

      if (caster == null)
      {
        return $"{RichTextColor.LIME}restores {baseValue} health{RichTextColor.CLOSING_TAG}";
      }

      int addition = (int)(CombatDamageCalculator.CalculateAttributeAddition(caster, modifier, _damageType.AttackAttribute) + 0.5f);
      int total = baseValue + addition;

      return
        $"{RichTextColor.LIME}restores {total}({baseValue}+{addition}{_damageType.IconRichText}) health{RichTextColor.CLOSING_TAG}";
    }

    public string GetDescriptionDuration(int skillLevel, CombatUnit caster)
    {
      // Heal is instant; no duration text.
      return string.Empty;
    }
  }
}