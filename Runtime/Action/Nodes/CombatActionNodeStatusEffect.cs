using UnityEngine;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using System.Threading;

namespace CupkekGames.Combat
{
  public class CombatActionNodeStatusEffect : CombatActionNodeWithTarget, ICombatActionNodeDescription
  {
    [SerializeField] private StatusEffectSO _statusEffectSO;
    [SerializeField] private int[] _level;
    [SerializeField] private float[] _duration;

    protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
    {
      var ctx = CombatActionContext.From(frame);
      if (ctx.IsCancelled) return BTNodeRuntimeState.Fail;

      int level = GetEffectLevel(ctx.SkillLevel);
      float duration = GetDuration(ctx.SkillLevel);

      foreach (CombatUnit target in GetTargetList(ctx.Caster, ctx.TargetList))
      {
        CancellationToken cancellationToken = ctx.CreateTargetLinkedToken(target.DeathToken.Token);

        if (duration <= 0)
        {
          duration = float.MaxValue;
          Debug.LogError("StatusEffect duration is 0, setting to float.MaxValue");
        }

        target.StatusEffects.Add(ctx.CombatManager, new StatusEffect(_statusEffectSO, duration, level, cancellationToken));
      }

      return BTNodeRuntimeState.Success;
    }

    private int GetEffectLevel(int skillLevel)
    {
      return CombatArrayUtils.GetArrayElementOrLast(_level, skillLevel - 1);
    }

    private float GetDuration(int skillLevel)
    {
      return CombatArrayUtils.GetArrayElementOrLast(_duration, skillLevel - 1);
    }

    protected override void OnReset()
    {
    }

    public string GetDescription(int skillLevel, CombatUnit caster, ICombatRules rules)
    {
      CombatDescriptionStyleSO style = rules.DescriptionStyle;
      string name = _statusEffectSO.Name.ToLowerInvariant();
      if (_statusEffectSO.HasTier)
      {
        name += $" T{GetEffectLevel(skillLevel)}";
      }

      return style.Colorize(CombatDescriptionRole.Status, style.Icon(CombatDescriptionRole.Status) + name);
    }

    public string GetDescriptionDuration(int skillLevel, CombatUnit caster, ICombatRules rules)
    {
      return DescribeDuration(GetDuration(skillLevel), rules.DescriptionStyle);
    }

    /// <summary>"5 seconds" in the duration colour, with its icon when authored.</summary>
    public static string DescribeDuration(float seconds, CombatDescriptionStyleSO style)
    {
      string text = $"{style.Icon(CombatDescriptionRole.Duration)}{seconds:0.##} seconds";
      return style.Colorize(CombatDescriptionRole.Duration, text);
    }
  }
}