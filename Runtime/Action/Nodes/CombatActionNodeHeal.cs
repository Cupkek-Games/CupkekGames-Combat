using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using CupkekGames.RPGStats;
using CupkekGames.TextPopup;

namespace CupkekGames.Combat
{
  public class CombatActionNodeHeal : CombatActionNodeWithTarget, ICombatActionNodeDescription
  {
    [SerializeField] private AttributeModifier[] _damage;
    [SerializeField] private DamageTypeDefinitionSO _damageType;
    [Tooltip("What the heal scales off: the caster's attack attribute, or a share of each target's max health.")]
    [SerializeField] private CombatValueScaling _scaling = CombatValueScaling.CasterAttribute;

    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      if (_damage.Length == 0)
      {
        return BTNodeRuntimeState.Success;
      }

      var ctx = CombatActionContext.From(frame);

      AttributeModifier modifier = GetDamageValue(ctx.SkillLevel);

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        int damage = (int)(CombatDamageCalculator.CalculateScaledValue(ctx.Caster, target, modifier, _scaling, _damageType.AttackAttribute) + 0.5f);
        ctx.CombatManager.PopupManager.Show(PopupKinds.Heal, target.View.HealthBarTransform.position, damage);
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

    public string GetDescription(int skillLevel, CombatUnit caster, ICombatRules rules)
    {
      AttributeModifier modifier = GetDamageValue(skillLevel);
      if (modifier == null)
      {
        Debug.LogError($"Modifier is null for skill level {skillLevel}");
        return "";
      }

      CombatDescriptionStyleSO style = rules.DescriptionStyle;
      string number = _scaling == CombatValueScaling.TargetMaxHealth
        ? CombatActionNodeDamage.ShareNumber(style, style.Icon(CombatDescriptionRole.Heal), modifier)
        : CombatActionNodeDamage.ScaledNumber(style, style.Icon(CombatDescriptionRole.Heal), modifier, caster, _damageType);

      return style.Colorize(CombatDescriptionRole.Heal, $"restores {number} health");
    }

    public string GetDescriptionDuration(int skillLevel, CombatUnit caster, ICombatRules rules)
    {
      // Heal is instant; no duration text.
      return string.Empty;
    }
  }
}