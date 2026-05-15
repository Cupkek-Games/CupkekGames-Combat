using UnityEngine;
using CupkekGames.BehaviourTrees;
using CupkekGames.Graphs;
using System.Threading;
using CupkekGames.Luna;

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

    public string GetDescription(int skillLevel, CombatUnit caster)
    {
      string name = _statusEffectSO.Name.ToLowerInvariant();

      if (_statusEffectSO.HasTier)
      {
        int level = GetEffectLevel(skillLevel);
        return $"{RichTextColor.PURPLE}{name} T{level}{RichTextColor.CLOSING_TAG}";
      }
      else
      {
        return $"{RichTextColor.PURPLE}{name}{RichTextColor.CLOSING_TAG}";
      }
    }

    public string GetDescriptionDuration(int skillLevel, CombatUnit caster)
    {
      float duration = GetDuration(skillLevel);
      string durationString = duration.ToString("0.##");

      return $"{RichTextColor.PURPLE}{durationString} seconds{RichTextColor.CLOSING_TAG}";
    }
  }
}