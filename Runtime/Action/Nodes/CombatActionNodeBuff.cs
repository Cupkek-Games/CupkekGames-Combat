using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CupkekGames.Luna;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
  public class CombatActionNodeBuff : CombatActionNodeWithTarget
  {
    [SerializeField] private Guid _id = System.Guid.NewGuid();

    public Guid ID
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

    [SerializeField] private float[] _duration;

    public float[] Duration
    {
      get => _duration;
      set => _duration = value;
    }

    [SerializeField] private bool _show = true;

    public bool Show
    {
      get => _show;
      set => _show = value;
    }

    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);

      AttributeEffect attributeEffect = GetAttributeDataEffect(ctx.SkillLevel);
      float duration = GetDuration(ctx.SkillLevel);
      CombatAttributeDataEffectRuntime attributeEffectRuntime = GetCombatAttributeDataEffectRuntime(
        ctx.ActionSO.Icon,
        ctx.ActionSO.Name,
        ctx.ActionSO.Description,
        _id,
        attributeEffect,
        duration,
        _show,
        ctx.CombatSettings.AttributeDisplayConfig
      );

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        target.Buffs.Add(attributeEffectRuntime);
      }

      return BTNodeRuntimeState.Success;
    }

    private AttributeEffect GetAttributeDataEffect(int skillLevel)
    {
      return CombatArrayUtils.GetArrayElementOrLast(_attributeEffect, skillLevel - 1);
    }

    private float GetDuration(int skillLevel)
    {
      return CombatArrayUtils.GetArrayElementOrLast(_duration, skillLevel - 1);
    }

    public static CombatAttributeDataEffectRuntime GetCombatAttributeDataEffectRuntime(
      Sprite icon, string name, string description, Guid id, AttributeEffect attributeEffect, float duration,
      bool show, AttributeDisplayConfigSO displayConfig = null)
    {
      UIColor color = displayConfig != null
        ? AttributeEffectColorHelper.GetColor(attributeEffect, displayConfig)
        : AttributeEffectColorHelper.GetColor(attributeEffect);

      return new CombatAttributeDataEffectRuntime(id,
        attributeEffect, duration, 1, icon, name, description,
        color, false, null, null, show);
    }

    protected override void OnReset()
    {
    }
  }
}