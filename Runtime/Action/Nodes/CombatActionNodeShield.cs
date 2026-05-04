using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.Luna;
using CupkekGames.BehaviourTrees;
using CupkekGames.Data;
using CupkekGames.RPGStats;
using CupkekGames.Data.Primitives;
using CupkekGames.TextPopup;

namespace CupkekGames.Combat
{
  public class CombatActionNodeShield : CombatActionNodeWithTarget, ICombatActionNodeDescription
  {
    [SerializeField] private AttributeModifier[] _damage;
    [SerializeField] private DamageTypeDefinitionSO _damageType;
    [SerializeField] private float[] _duration;
    [SerializeField] private SerializedGuid _id = new SerializedGuid(System.Guid.NewGuid());

    public SerializedGuid ID
    {
      get => _id;
      set => _id = value;
    }

    [SerializeField] private AttributeEffect[] _attributeEffect;

    public AttributeEffect[] AttributeEffect
    {
      get => _attributeEffect;
      set => _attributeEffect = value;
    }

    [SerializeField] private bool _show = true;

    public bool Show
    {
      get => _show;
      set => _show = value;
    }

    protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
    {
      var ctx = CombatActionContext.From(Blackboard);
      if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

      AttributeModifier modifier = GetDamageValue(ctx.SkillLevel);
      float duration = GetDuration(ctx.SkillLevel);

      int damage =
        (int)(CombatDamageCalculator.CalculateScaledValue(ctx.Caster, modifier, _damageType.AttackAttribute) + 0.5f);

      // buff
      AttributeEffect attributeEffect = GetAttributeDataEffect(ctx.SkillLevel);
      CombatAttributeDataEffectRuntime attributeEffectRuntime =
        CombatActionNodeBuff.GetCombatAttributeDataEffectRuntime(
          ctx.ActionSO.Icon,
          ctx.ActionSO.Name,
          ctx.ActionSO.Description,
          _id.Value(),
          attributeEffect,
          duration,
          _show,
          ctx.CombatSettings.AttributeDisplayConfig
        );

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        CancellationToken cancellationToken = ctx.CreateTargetLinkedToken(target.DeathToken.Token);

        ctx.CombatManager.PopupManager.Show(PopupKinds.Shield, target.CombatUnitGameObject.HealthBarTransform.position, damage);

        CombatUnitShieldNode shield = new CombatUnitShieldNode(damage, _id.Value(), attributeEffectRuntime);

        target.Shield.AddShield(shield, duration, cancellationToken);
      }

      return BTNodeRuntimeState.Success;
    }

    private AttributeModifier GetDamageValue(int skillLevel)
    {
      return CombatArrayUtils.GetArrayElementOrLast(_damage, skillLevel - 1);
    }

    private float GetDuration(int skillLevel)
    {
      return CombatArrayUtils.GetArrayElementOrLast(_duration, skillLevel - 1);
    }

    private AttributeEffect GetAttributeDataEffect(int skillLevel)
    {
      return CombatArrayUtils.GetArrayElementOrLast(_attributeEffect, skillLevel - 1);
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
        return $"{RichTextColor.AQUA}grants {baseValue} shield{RichTextColor.CLOSING_TAG}";
      }

      int addition =
        (int)(CombatDamageCalculator.CalculateAttributeAddition(caster, modifier, _damageType.AttackAttribute) + 0.5f);
      int total = baseValue + addition;

      return
        $"{RichTextColor.AQUA}grants {total}({baseValue}+{addition}{_damageType.IconRichText}) shield{RichTextColor.CLOSING_TAG}";
    }

    public string GetDescriptionDuration(int skillLevel, CombatUnit caster)
    {
      // Shield is instant; no duration text.
      return string.Empty;
    }
  }
}